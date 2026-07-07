namespace Module.Shipping.Features.Admin.ShippingMethods.Shared.Models;

/// <summary>Detail response for a shipping method, including audit fields.</summary>
public class ShippingMethodDetailResponse : ShippingMethodParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}

/// <summary>List item response for a shipping method.</summary>
public class ShippingMethodListItemResponse : ShippingMethodDetailResponse;
