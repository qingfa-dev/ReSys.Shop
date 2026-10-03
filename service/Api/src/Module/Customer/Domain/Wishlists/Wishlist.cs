using Module.Customer.Domain.Wishlists.WishedItems;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Concerns.SoftDeletable;
using Shared.Application.Domain.Models;
using Shared.Security.Identity.Domain.Users;

namespace Module.Customer.Domain.Wishlists;

// Invariant: Token is unique; IsPrivate implies not shareable; Name is non-null and non-empty
/// <summary>Represents a wishlist owned by a user with a collection of wished items.</summary>
public sealed partial class Wishlist : Entity, IAuditable, ISoftDeletable
{
    #region Properties

    public string Name { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsPrivate { get; set; }
    public Guid UserId { get; set; }

    #region Auditable

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }

    #endregion Auditable

    #region Soft Deletion

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    #endregion Soft Deletion

    #endregion Properties

    #region Relationships

    public User? User { get; set; }
    public ICollection<WishedItem> WishedItems { get; set; } = [];

    #endregion Relationships
}