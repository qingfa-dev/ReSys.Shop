namespace Module.Payment.Features.Admin.Payments.Shared.Models;

public abstract class PaymentParameters
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public Guid OrderId { get; init; }
    public Guid PaymentMethodId { get; init; }
    public string State { get; init; } = string.Empty;
}
