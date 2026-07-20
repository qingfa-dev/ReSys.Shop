using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;

namespace Module.Profile.Domain.Wishlists.WishedItems;

/// <summary>Represents an item in a wishlist with a specific variant and quantity.</summary>
// Invariant: Quantity >= 1; VariantId != Guid.Empty; WishlistId != Guid.Empty
public sealed partial class WishedItem : Entity, IAuditable
{
    #region Properties
    public int Quantity { get; set; }
    public Guid VariantId { get; set; }
    public Guid WishlistId { get; set; }

    #region Relationships
    public Wishlist? Wishlist { get; set; }
    #endregion Relationships

    #region Auditable
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditable
    #endregion Properties
}