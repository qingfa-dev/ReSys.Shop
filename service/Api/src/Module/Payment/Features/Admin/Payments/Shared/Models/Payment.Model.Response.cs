namespace Module.Payment.Features.Admin.Payments.Shared.Models;

public record PaymentDetailResponse : PaymentParameters, IResponse
{
    public Guid Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public string? ResponseCode { get; init; }
    public string? PaymentMethodName { get; init; }
    public string? ClientSecret { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}

public record PaymentListItemResponse : PaymentParameters, IResponse
{
    public Guid Id { get; init; }
}