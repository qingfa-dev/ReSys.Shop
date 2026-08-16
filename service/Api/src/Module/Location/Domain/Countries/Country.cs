using Module.Inventory.Domain.StockLocations;
using Module.Location.Domain.States;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;

namespace Module.Location.Domain.Countries;

/// <summary>
/// Represents a country entity mapped from ISO 3166-1 standard.
/// </summary>
// Context: Implements ISO 3166-1 country code standard; see https://www.iso.org/iso-3166-country-codes.html
// Invariant: IsoCode must be a valid 2-letter ISO 3166-1 code; Iso3Code must be valid 3-letter code
public sealed partial class Country : Entity, IAuditable
{
    #region Properties

    // Assign: Core country identity fields mapped from ISO 3166-1 standard
    /// <summary>Official name of the country.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>ISO 3166-1 alpha-2 country code (2 letters).</summary>
    public string IsoCode { get; set; } = string.Empty;
    /// <summary>ISO 3166-1 alpha-3 country code (3 letters).</summary>
    public string? Iso3Code { get; set; }
    /// <summary>ISO 3166-1 country name as defined by the ISO standard.</summary>
    public string? IsoName { get; set; }

    /// <summary>International dialing code for the country (e.g., +1).</summary>
    public string? CallingCode { get; set; }

    // Assign: Regional validation requirements — used by address forms
    /// <summary>Indicates whether states/provinces are required for addresses in this country.</summary>
    public bool StatesRequired { get; set; }

    /// <summary>Indicates whether zip/postal codes are required for addresses in this country.</summary>
    public bool ZipcodeRequired { get; set; }

    // Check: Marked inactive when country is no longer served
    /// <summary>Indicates whether the country is active and available for use.</summary>
    public bool IsActive { get; set; } = true;

    #endregion Properties

    #region Auditable

    // Log: Timestamps for creation and last modification — audit trail
    /// <summary>Timestamp when the country was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
    /// <summary>Timestamp when the country was last modified.</summary>
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }

    #endregion Auditable

    #region Relationships

    // Aggregate: States belonging to this country, ordered by name for consistent display
    /// <summary>Collection of states/provinces within this country.</summary>
    public ICollection<State> States { get; set; } = [];
    public ICollection<StockLocation> StockLocations {get;set;} = [];
    #endregion Relationships
}