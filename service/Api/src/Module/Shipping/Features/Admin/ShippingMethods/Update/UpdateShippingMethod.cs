using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Mappings;

namespace Module.Shipping.Features.Admin.ShippingMethods.Update;
/// <summary>Updates an existing shipping method.</summary>
public static partial class UpdateShippingMethod
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Handles updating a shipping method.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated shipping method response.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            var request = command.Request;

            // Check: Get shipping method by ID.
            var method = await dbContext.Set<ShippingMethod>().FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);
            if (method is null)
                return ShippingMethodResult.Errors.NotFound;

            // Update: Apply PATCH changes to the shipping method.
            var result = request.MapUpdateToDomain(method);
            if (result.IsFailure)
                return result.Failures;

            // Validate: Business rules.
            if (request.Presentation is not null)
                method.Presentation = request.Presentation;

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Operation success.
            ShippingMethodLoggers.Updated(logger, method.Name, method.Id);
            // Map: Return response.
            return Result<Response>.Ok(method.MapToDetail<Response>(), ShippingMethodResult.Success.Updated);
        }
    }
}
