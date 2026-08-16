using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Shared.Mappings;

namespace Module.Ordering.Features.Admin.Orders.Approve;

/// <summary>Transitions a placed order to approved state, recording the approver identity and timestamp for audit trail.</summary>
public static partial class ApproveOrder
{
    public sealed record Command(Guid Id) : ICommand<Response>;
    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : ICommandHandler<Command, Response>
    {
        /// <summary>Approves an order by invoking the domain approval logic and persisting the state change.</summary>
        /// <param name="command">The command containing the order ID to approve.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The approval response with approver details.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            // Check: Find the order to approve.
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null) 
                return OrderResult.Errors.NotFound(command.Id);

            var approvedById = Guid.TryParse(currentUser.UserId, out var parsedId) ? parsedId : Guid.Empty;
            var result = order.Approve(approvedById);
            if (result.IsFailure) 
                return result.Errors;
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the approval response with full order detail.
            return Result<Response>.Ok(order.MapToDetail<Response>(), OrderResult.Success.Approved(order.Id));
        }
    }
}