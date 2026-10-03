namespace Module.Billing.Features.Storefront.MarkPaymentPaid;

public sealed record MarkPaymentPaidCommand : ICommand
{
    public Guid OrderId { get; init; }
    public string PaymentIntentId { get; init; } = default!;
}
