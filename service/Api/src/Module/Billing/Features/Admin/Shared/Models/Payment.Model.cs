namespace Module.Billing.Features.Admin.Shared.Models;

#region  Parameters
public abstract record PaymentActionParameters
{
    public decimal? Amount { get; init; }
}

public abstract record PaymentParameters
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public Guid OrderId { get; init; }
    public Guid PaymentMethodId { get; init; }
    public string State { get; init; } = string.Empty;
    public string? PaymentStatus { get; init; }
}
#endregion

#region  Request
public record PaymentRequest : PaymentParameters;
#endregion

#region Response
public record PaymentDetailResponse : PaymentParameters
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

public record PaymentListItemResponse : PaymentParameters
{
    public Guid Id { get; init; }
}
#endregion
