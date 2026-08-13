using Module.Billing.Domain.PaymentCaptures;

namespace Module.Billing.Features.Storefront.MarkPaymentPaid;

public sealed class MarkPaymentPaidCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<MarkPaymentPaidCommand>
{
    public async Task<Result> Handle(
        MarkPaymentPaidCommand command, CancellationToken cancellationToken)
    {
        Guid? parsedId = Guid.TryParse(command.PaymentIntentId, out var g) ? g : null;

        var payment = await dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(
                p => p.OrderId == command.OrderId
                     && ((parsedId.HasValue && p.Id == parsedId.Value)
                          || p.ResponseCode == command.PaymentIntentId),
                cancellationToken);

        if (payment is null)
            return PaymentCaptureResult.Failure.NotFound;

        if (payment.State != PaymentRecordState.Completed)
        {
            payment.State = PaymentRecordState.Completed;
            payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Ok();
    }
}
