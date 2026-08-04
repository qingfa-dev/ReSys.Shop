using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Options;
using Module.Catalog.Features.Admin.Products.Variants.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.Variants.Add;

/// <summary>
/// Defines the use case for adding a new variant to a product.
/// </summary>
public static partial class AddVariant
{
    public sealed record Command(Request Request) : ICommand<Response>;

    /// <summary>
    /// Adds a new variant to a product. Supports option-value assignment
    /// for non-master variants and enforces that master variants cannot
    /// have option values.
    /// </summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Handles the add-variant command — validates product existence, creates
        /// the variant entity with optional option-value junctions, persists,
        /// and returns the created variant detail.
        /// </summary>
        /// <param name="command">The command containing the parent product ID and variant request payload.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A created result with the new variant detail.</returns>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.ProductId!=Guid.Empty, post=variant.Id!=null, throws=DbUpdateException
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var productId = request.ProductId;

            // Check: Parent product must exist before adding variant
            var productExists = await dbContext.Set<Product>()
                .AnyAsync(x => x.Id == productId, cancellationToken);
            if (!productExists)
                return ProductResult.Errors.NotFound(productId);

            // Check: SKU must be unique across all variants
            var skuExists = await dbContext.Set<Variant>()
                .AnyAsync(x => x.Sku == request.Sku, cancellationToken);

            if (skuExists)
                return VariantResult.Errors.SkuAlreadyExists(request.Sku);

            // Create: Variant domain entity from request via mapping
            var result = request.MapToDomain(productId);
            if (result.IsFailure)
                return result.Errors;
            var variant = result.Value;

            if (request.OptionValueIds is { Count: > 0 })
            {
                // Enforce: Master variant cannot have option values assigned
                if (variant.IsMaster)
                    return VariantResult.Errors.MasterCannotHaveOptions;

                // Create: Junction entities linking variant to each option value
                foreach (var optionValueId in request.OptionValueIds)
                {
                    var junctionResult = OptionValueVariantMethod.Create(variant.Id, optionValueId);
                    if (junctionResult.IsFailure)
                        return junctionResult.Errors;

                    variant.OptionValueVariants.Add(junctionResult.Value);
                }
            }

            dbContext.Set<Variant>().Add(variant);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record variant creation event for observability
            VariantLoggers.Created(logger, Sku: variant.Sku!, Id: variant.Id, ProductId: variant.ProductId, ActionBy: currentUser.UserName);

            // Map: Return created variant as detail DTO with 201 response
            return Result<Response>.Created(
                variant.MapToDetail<Response>(),
                VariantResult.Success.Created(variant.Id));
        }
    }
}