using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.Approve;

/// <summary>Transitions a placed order to approved state, recording the approver identity and timestamp for audit trail.</summary>
public static partial class ApproveOrder
{
    public class Response
    {
        public Guid Id { get; init; }
        /// <summary>Identity of the administrator who approved the order — null if approval was system-initiated.</summary>
        public Guid? ApprovedById { get; init; }
        /// <summary>UTC timestamp of when the approval was recorded.</summary>
        public DateTimeOffset? ApprovedAtUtc { get; init; }
    }
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
            if (order is null) return (Result<Response>)OrderResult.Errors.NotFound(command.Id);

            var approvedById = Guid.TryParse(currentUser.UserId, out var parsedId) ? parsedId : Guid.Empty;
            var result = order.Approve(approvedById);
            if (result.IsFailure) return (Result<Response>)result.Errors;
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the approval response with approver details.
            return new Response { Id = order.Id, ApprovedById = order.ApprovedById, ApprovedAtUtc = order.ApprovedAtUtc };
        }
    }
}
