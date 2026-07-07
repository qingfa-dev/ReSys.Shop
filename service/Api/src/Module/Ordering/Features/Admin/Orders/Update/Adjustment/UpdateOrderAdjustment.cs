using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Adjustments;
using Module.Ordering.Domain.Orders;
using AdjustmentDomain = Module.Ordering.Domain.Adjustments.Adjustment;

namespace Module.Ordering.Features.Admin.Orders.Update.Adjustment;

/// <summary>Updates an adjustment's state or eligibility.</summary>
public static partial class UpdateOrderAdjustment
{
    public class Response
    {
        public Guid Id { get; init; }
        public string State { get; init; } = string.Empty;
        public bool Eligible { get; init; }
        public DateTimeOffset? ModifiedAtUtc { get; init; }
    }

    public sealed record Command(Guid OrderId, Guid AdjustmentId, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var adjustment = await dbContext.Set<AdjustmentDomain>()
                .FirstOrDefaultAsync(
                    a => a.Id == command.AdjustmentId && a.OrderId == command.OrderId,
                    cancellationToken);

            if (adjustment is null)
                return (Result<Response>)AdjustmentResult.Errors.NotFound(command.AdjustmentId);

            Result actionResult = command.Request.Action switch
            {
                AdjustmentAction.Close => adjustment.Close(),
                AdjustmentAction.Open => adjustment.Open(),
                AdjustmentAction.MarkEligible => adjustment.MarkEligible(),
                AdjustmentAction.MarkIneligible => adjustment.MarkIneligible(),
                _ => Result.Failure(AdjustmentResult.Errors.ActionInvalid)
            };

            if (actionResult.IsFailure)
                return (Result<Response>)actionResult.Failures;

            var order = await dbContext.Set<Order>()
                .Include(o => o.LineItems)
                .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);
            if (order is not null)
                order.RecalculateTotals();

            adjustment.ModifiedAtUtc = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Id = adjustment.Id,
                State = adjustment.State,
                Eligible = adjustment.Eligible,
                ModifiedAtUtc = adjustment.ModifiedAtUtc
            };
        }
    }
}
