using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Mappings;

namespace Module.Shipping.Features.Admin.ShippingMethods.Update;

/// <summary>Updates an existing shipping method with PATCH semantics.</summary>
public static partial class UpdateShippingMethod
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Applies partial updates to the shipping method and persists changes.</summary>
        /// <param name="command">The command containing the shipping method ID and update data.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the updated shipping method details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=method!=null, post=method updated, throws=DbUpdateException
            // Load: Find the shipping method by ID
            var method = await dbContext.Set<ShippingMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

            if (method is null)
                return ShippingMethodResult.Errors.NotFound;

            // Update: Apply partial update mapping
            var result = command.Request.MapToDomain(method);
            if (result.IsFailure)
                return result.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the updated method as response
            return method.MapToDetail<Response>();
        }
    }
}