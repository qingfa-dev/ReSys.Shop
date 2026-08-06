namespace Module.Shipping.Features.Admin.ShippingMethods.Shared.Models;

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