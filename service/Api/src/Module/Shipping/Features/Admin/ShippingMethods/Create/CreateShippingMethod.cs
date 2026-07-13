using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Mappings;

namespace Module.Shipping.Features.Admin.ShippingMethods.Create;

/// <summary>Creates a new shipping method.</summary>
public static partial class CreateShippingMethod
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Maps the request to a domain entity and persists the new shipping method.</summary>
        /// <param name="command">The command containing the shipping method data.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the created shipping method details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null && command.Request!=null, post=shippingMethod persisted,
            //           throws=DbUpdateException
            var request = command.Request;

            // Create: Map the request to a domain entity
            var createResult = request.MapToDomain();
            if (createResult.IsFailure)
                return createResult.Errors;

            var method = createResult.Value;

            dbContext.Set<ShippingMethod>().Add(method);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the created method as response
            return method.MapToDetail<Response>();
        }
    }
}