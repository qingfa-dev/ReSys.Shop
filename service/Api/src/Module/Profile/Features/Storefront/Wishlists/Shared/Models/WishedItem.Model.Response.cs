namespace Module.Profile.Features.Storefront.Wishlists.Shared.Models;

public record WishedItemResponse
{
    public Guid Id { get; init; }
    public Guid VariantId { get; init; }
    public int Quantity { get; init; }
    public DateTimeOffset AddedAtUtc { get; init; }
}