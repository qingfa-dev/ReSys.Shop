using Shared.Application.Mediators.Commands;

namespace Module.Payment.Features.Storefront.MarkPaymentPaid;

public sealed record MarkPaymentPaidCommand : ICommand
{
    public Guid OrderId { get; init; }
    public string PaymentIntentId { get; init; } = default!;
}
