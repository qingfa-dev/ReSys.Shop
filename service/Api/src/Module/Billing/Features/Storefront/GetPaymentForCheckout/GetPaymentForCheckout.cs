using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Services.Provider;


namespace Module.Billing.Features.Storefront.GetPaymentForCheckout;

public sealed class GetPaymentForCheckoutQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetPaymentForCheckoutQuery, PaymentForCheckoutResponse>
{
    public async Task<Result<PaymentForCheckoutResponse>> Handle(
        GetPaymentForCheckoutQuery query, CancellationToken cancellationToken)
    {
        Guid? parsedId = Guid.TryParse(query.PaymentIntentId, out var g) ? g : null;

        var payment = await dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(
                p => p.OrderId == query.OrderId
                     && ((parsedId.HasValue && p.Id == parsedId.Value)
                          || p.ResponseCode == query.PaymentIntentId),
                cancellationToken);

        return new PaymentForCheckoutResponse
        {
            Amount = payment?.Amount ?? 0m,
            IsCompleted = payment?.State == PaymentRecordState.Completed,
            IsPending = payment?.State == PaymentRecordState.Pending,
            CompletedAtUtc = payment?.CompletedAtUtc,
            IsOffline = payment is not null
                && GatewayConstants.Providers.IsOffline(payment.ProviderKey)
        };
    }
}
