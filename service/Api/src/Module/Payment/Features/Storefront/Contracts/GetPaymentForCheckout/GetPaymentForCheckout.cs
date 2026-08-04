using Module.Payment.Domain.PaymentCaptures;
using Shared.Application.Contracts.Payment;

namespace Module.Payment.Features.Storefront.Contracts.GetPaymentForCheckout;

public sealed class GetPaymentForCheckoutQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetPaymentForCheckoutQuery, PaymentForCheckoutResponse>
{
    public async Task<Result<PaymentForCheckoutResponse>> Handle(
        GetPaymentForCheckoutQuery query, CancellationToken cancellationToken)
    {
        var payment = await dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(
                p => p.ResponseCode == query.PaymentIntentId
                     && p.OrderId == query.OrderId,
                cancellationToken);

        return new PaymentForCheckoutResponse
        {
            Amount = payment?.Amount ?? 0m,
            IsCompleted = payment?.State == PaymentRecordState.Completed
        };
    }
}
