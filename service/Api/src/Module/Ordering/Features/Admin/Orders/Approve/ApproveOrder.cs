using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.Approve;

/// <summary>Approves a placed order.</summary>
public static partial class ApproveOrder
{
    public class Response { public Guid Id { get; init; } public Guid? ApprovedById { get; init; } public DateTimeOffset? ApprovedAtUtc { get; init; } }
    public sealed record Command(Guid Id) : ICommand<Response>;
    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null) return (Result<Response>)OrderResult.Errors.NotFound(command.Id);

            var approvedById = Guid.TryParse(currentUser.UserId, out var parsedId) ? parsedId : Guid.Empty;
            var result = order.Approve(approvedById);
            if (result.IsFailure) return (Result<Response>)result.Failures;

            order.ApprovedAtUtc = DateTimeOffset.UtcNow;
            order.ModifiedAtUtc = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response { Id = order.Id, ApprovedById = order.ApprovedById, ApprovedAtUtc = order.ApprovedAtUtc };
        }
    }
}
