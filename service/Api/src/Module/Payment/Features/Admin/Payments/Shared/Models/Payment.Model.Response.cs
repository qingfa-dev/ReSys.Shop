namespace Module.Payment.Features.Admin.Payments.Shared.Models;

public class PaymentDetailResponse : PaymentParameters
{
    public Guid Id { get; init; }
    public string? ClientSecret { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}

public class PaymentListItemResponse : PaymentParameters
{
    public Guid Id { get; init; }
}
