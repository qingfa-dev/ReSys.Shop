using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Admin.Products.Shared.Mappings;
using Module.Catalog.Features.Admin.Products.Variants.Add;

namespace Module.Catalog.Features.Admin.Products.Create;

/// <summary>
/// Defines the use case for creating a new product with a master variant.
/// </summary>
public static partial class CreateProduct
{
    public sealed record Command(Request Request) : ICommand<Response>;

    /// <summary>
    /// Creates a new product with a master variant. Validates slug uniqueness,
    /// persists the product, creates the master variant via command dispatch,
    /// and links the master variant ID back to the product.
    /// </summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser,
        ISender sender)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Handles the creation command — validates slug, creates product,
        /// dispatches master variant creation, and persists the final state.
        /// </summary>
        /// <param name="command">The command containing the create request payload.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A created result with the new product detail.</returns>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.Request!=null, post=product.Id!=null && product.MasterVariantId!=null,
        //           throws=DbUpdateException
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Validate: Product slug must be unique to prevent duplicate URL routes
            var slugExists = await dbContext.Set<Product>()
                .AnyAsync(x => x.Slug == request.Slug, cancellationToken);
            if (slugExists)
                return ProductResult.Errors.DuplicateSlug;

            // Create: Product entity from validated request via factory method
            var result = request.MapToDomain();
            if (result.IsFailure)
                return result.Errors;
            var product = result.Value;

            dbContext.Set<Product>().Add(product);

            await dbContext.SaveChangesAsync(cancellationToken);

            var variantRequest = new AddVariant.Request
            {
                Sku = $"{product.Slug}-master",
                IsMaster = true,
                TrackInventory = request.TrackInventory,
            };

            // Call: Create master variant via AddVariant command — establishes master-variant link
            var addVariantResult = await sender.Send(
                new AddVariant.Command(product.Id, variantRequest), cancellationToken);

            if (addVariantResult.IsFailure)
                return addVariantResult.Errors;
            product.MasterVariantId = addVariantResult.Value.Id;
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record product creation event for observability
            ProductLoggers.Created(logger, Name: product.Name, Id: product.Id, ActionBy: currentUser.UserName);

            return Result<Response>.Created(
                product.MapToDetail<Response>(),
                ProductResult.Success.Created(product.Id));
        }
    }
}
