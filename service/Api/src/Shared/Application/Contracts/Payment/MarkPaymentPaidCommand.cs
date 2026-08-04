using Shared.Application.Mediators.Commands;

namespace Shared.Application.Contracts.Payment;

public sealed record MarkPaymentPaidCommand : ICommand
{
    public Guid OrderId { get; init; }
    public string PaymentIntentId { get; init; } = default!;
}
