using Shared.Application.Mediators.Queries;

namespace Shared.Application.Contracts.Payment;

public sealed record GetPaymentForCheckoutQuery : IQuery<PaymentForCheckoutResponse>
{
    public string PaymentIntentId { get; init; } = default!;
    public Guid OrderId { get; init; }
}

public sealed record PaymentForCheckoutResponse
{
    public decimal Amount { get; init; }
    public bool IsCompleted { get; init; }
}
