using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Features.Admin.ShippingMethods.Delete;
/// <summary>Soft-deletes a shipping method by ID.</summary>
public static partial class DeleteShippingMethod
{
    public sealed record Command(Guid Id) : ICommand;
    public sealed class CommandHandler(IApplicationDbContext dbContext, ILogger<CommandHandler> logger) : ICommandHandler<Command>
    {
        /// <summary>Handles deleting a shipping method.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Ok result.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Query: Get shipping method by ID.
            var method = await dbContext.Set<ShippingMethod>().FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);
            if (method is null) return ShippingMethodResult.Errors.NotFound;
            // Remove: Soft-delete the shipping method.
            method.IsDeleted = true;
            method.DeletedAtUtc = DateTimeOffset.UtcNow;
            method.DeletedBy = "System";
            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);
            // Log: Operation success.
            ShippingMethodLoggers.Deleted(logger, method.Name, method.Id);
            return Result.Ok();
        }
    }
}
