namespace Module.Shipping.Features.Admin.Shared.Models;

#region Parameters
public abstract record ShippingMethodParameters : INamedParameters, ISortableParameters
{
    public string Name { get; init; } = string.Empty;
    public string? Code { get; init; }
    public string? TrackingUrl { get; init; }
    public string? AdminName { get; init; }
    public int Position { get; init; }
    public bool AvailableToUsers { get; init; }
    public string CalculatorType { get; init; } = string.Empty;
    public string? Presentation { get; init; }
}
#endregion

#region Request
public record ShippingMethodRequest : ShippingMethodParameters;
#endregion

#region Response
public record ShippingMethodDetailResponse : ShippingMethodParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
    public bool IsDeleted { get; init; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
}

public record ShippingMethodListItemResponse : ShippingMethodDetailResponse;
#endregion
