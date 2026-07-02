using Module.Location.Domain.Countries;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;

namespace Module.Location.Domain.States;

/// <summary>
/// Represents a state or province belonging to a country.
/// </summary>
// Context: Implements ISO 3166-2 subdivision codes; see https://www.iso.org/iso-3166-country-codes.html
// Invariant: Abbreviation must be unique per country; Name is required
public sealed partial class State : Entity, IAuditable
{
    #region Properties

    // Assign: Display name of the state or province
    /// <summary>Official name of the state or province.</summary>
    public string Name { get; set; } = string.Empty;

    // Assign: Short code (e.g., "CA" for California) — unique within its country
    /// <summary>ISO 3166-2 subdivision code or standard abbreviation.</summary>
    public string Abbreviation { get; set; } = string.Empty;

    // Aggregate: Foreign key to the parent country aggregate
    /// <summary>Foreign key to the parent country.</summary>
    public Guid CountryId { get; set; }

    // Check: Marked inactive when state is no longer served
    /// <summary>Indicates whether the state is active and available for use.</summary>
    public bool IsActive { get; set; } = true;

    #endregion Properties

    #region Auditable

    // Log: Timestamps for creation and last modification — audit trail
    /// <summary>Timestamp when the state was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
    /// <summary>Timestamp when the state was last modified.</summary>
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }

    #endregion Auditable

    #region Navigation

    // Aggregate: Reference to parent country aggregate
    /// <summary>Navigation property to the parent country.</summary>
    public Country Country { get; set; } = null!;

    #endregion Navigation
}