namespace Module.Ordering.Features.Storefront.CompleteCheckoutForPayment;

public sealed record CompleteCheckoutForPaymentCommand : ICommand<CompleteCheckoutForPaymentResponse>
{
    public Guid CartId { get; init; }
    public Guid PaymentId { get; init; }
}

public sealed record CompleteCheckoutForPaymentResponse
{
    public Guid OrderId { get; init; }
}
