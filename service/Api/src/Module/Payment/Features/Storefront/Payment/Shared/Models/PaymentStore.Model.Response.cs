using Module.Payment.Features.Admin.Payments.Shared.Models;

namespace Module.Payment.Features.Storefront.Payment.Shared.Models;

public record StorePaymentDetailResponse : PaymentParameters
{
    public Guid Id { get; init; }
    public string? ClientSecret { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
}

public record StorePaymentListItemResponse : PaymentParameters
{
    public Guid Id { get; init; }
}
