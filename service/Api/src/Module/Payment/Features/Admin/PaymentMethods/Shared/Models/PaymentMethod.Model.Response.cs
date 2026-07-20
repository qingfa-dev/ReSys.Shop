namespace Module.Payment.Features.Admin.PaymentMethods.Shared.Models;

/// <summary>Detail response for a payment method, including audit timestamps.</summary>
public record PaymentMethodDetailResponse : PaymentMethodParameters, IResponse
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the UTC timestamp when created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>Gets or sets the UTC timestamp when last modified.</summary>
    public DateTimeOffset? ModifiedAtUtc { get; set; }

    /// <summary>Gets or sets the user who created this entity.</summary>
    public string? CreatedBy { get; init; }

    /// <summary>Gets or sets the user who last modified this entity.</summary>
    public string? ModifiedBy { get; init; }
}

/// <summary>List item response for a payment method.</summary>
public record PaymentMethodListItemResponse : PaymentMethodParameters, IResponse
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
}