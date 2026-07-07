using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Mappings;

namespace Module.Shipping.Features.Admin.ShippingMethods.Create;
/// <summary>Creates a new shipping method.</summary>
public static partial class CreateShippingMethod
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Handles creating a shipping method.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created shipping method response.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            var request = command.Request;

            // Check: Verify no duplicate name exists.
            var nameExists = await dbContext.Set<ShippingMethod>()
                .AnyAsync(x => x.Name == request.Name, cancellationToken);
            if (nameExists)
                return ShippingMethodResult.Errors.CodeDuplicate;

            // Create: Build the shipping method via mapping.
            var createResult = request.MapToDomain();
            if (createResult.IsFailure)
                return createResult.Failures;

            var method = createResult.Value;

            // Assign: Additional properties not in Create factory.
            method.TrackingUrl = request.TrackingUrl;
            method.AdminName = request.AdminName;
            method.Position = request.Position;

            // Persist: Save changes.
            dbContext.Set<ShippingMethod>().Add(method);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Operation success.
            ShippingMethodLoggers.Created(logger, method.Name, method.Id);

            // Map: Return response.
            return Result<Response>.Ok(method.MapToDetail<Response>(), ShippingMethodResult.Success.Created);
        }
    }
}
