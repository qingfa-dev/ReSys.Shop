using Module.Payment.Domain.PaymentCaptures;

namespace Module.Payment.Features.Storefront.MarkPaymentPaid;

public sealed class MarkPaymentPaidCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<MarkPaymentPaidCommand>
{
    public async Task<Result> Handle(
        MarkPaymentPaidCommand command, CancellationToken cancellationToken)
    {
        var payment = await dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(
                p => p.ResponseCode == command.PaymentIntentId
                     && p.OrderId == command.OrderId,
                cancellationToken);

        if (payment is null)
            return PaymentCaptureResult.Failure.NotFound;

        payment.State = PaymentRecordState.Completed;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
