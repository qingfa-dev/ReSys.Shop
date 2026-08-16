using Module.Customer.Domain.Addresses;
using Module.Customer.Domain.Notifications;
using Module.Customer.Domain.Preferences;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;
using Shared.Security.Identity.Domain.Users;

namespace Module.Customer.Domain;

/// <summary>Represents a user profile with personal, commerce, and preference data.</summary>
// Invariant: FirstName != null && LastName != null; Email != null; UserId != Guid.Empty; OrdersCount >= 0; TotalSpent >= 0
public sealed class UserProfile : Entity, IAuditable
{
    #region Properties

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    public DateTimeOffset? DateOfBirth { get; set; }
    public string? Gender { get; set; }

    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }

    public UserPreferences Preferences { get; set; } = new();
    public NotificationPreferences Notifications { get; set; } = NotificationPreferences.Default;

    public bool IsActive { get; set; } = UserProfileConstant.Defaults.IsActive;

    #region Commerce

    public bool AcceptsEmailMarketing { get; set; }
    public string? InternalNoteHtml { get; set; }
    public Guid? DefaultBillingAddressId { get; set; }
    public Guid? DefaultShippingAddressId { get; set; }
    public int OrdersCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTimeOffset? LastOrderCompletedAtUtc { get; set; }

    #endregion Commerce

    #region Auditable

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }

    #endregion Auditable

    #endregion Properties

    #region Relationships

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Address? DefaultBillingAddress { get; set; }
    public Address? DefaultShippingAddress { get; set; }
    public ICollection<Address> Addresses { get; set; } = [];

    #endregion Relationships
}