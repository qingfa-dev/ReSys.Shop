using Module.Billing.Features.Admin.Shared.Models;

namespace Module.Billing.Features.Storefront.Shared.Models;

public record StorePaymentRequest : PaymentParameters;

public abstract record PaymentConfirmationParameters
{
    public Guid PaymentId { get; init; }
    public Guid? PaymentMethodId { get; init; }
}

public record StorePaymentDetailResponse : PaymentParameters
{
    public Guid Id { get; init; }
    public string? ClientSecret { get; init; }
    public string? CheckoutUrl { get; init; }
    public string? ResponseCode { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
}

public record StorePaymentListItemResponse : PaymentParameters
{
    public Guid Id { get; init; }
}
