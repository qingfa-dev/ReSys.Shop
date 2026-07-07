using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;

namespace Module.Shipping.Domain.ShippingRates;
/// <summary>Represents a Shipping Rate.</summary>

// @CAT-10 Invariant: Cost >= 0; FinalPrice >= 0; Selected is unique per Shipment; Name is required
public sealed partial class ShippingRate : Entity, IAuditable
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public bool Selected { get; set; } = ShippingRateConstant.Defaults.Selected;
    public decimal Cost { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal AdditionalTaxTotal { get; set; }
    public decimal IncludedTaxTotal { get; set; }
    public string DisplayPrice { get; set; } = string.Empty;
    public string? DeliveryRange { get; set; }
    public decimal? MinWeight { get; set; }
    public decimal? MaxWeight { get; set; }
    public decimal? FreeShippingThreshold { get; set; }
    #endregion Properties

    #region Relationships
    public Guid ShippingMethodId { get; set; }
    #endregion Relationships

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Constructor
    internal ShippingRate() { } // For EF Core
    #endregion Constructor
}