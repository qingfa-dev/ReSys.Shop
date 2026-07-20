namespace Module.Shipping.Features.Storefront.Shared.Models;

public record ShippingMethodDetailResponse : ShippingMethodParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}

public record ShippingMethodListItemResponse : ShippingMethodParameters
{
    public Guid Id { get; init; }
}