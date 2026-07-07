using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Concerns.Parameterizable;
using Shared.Application.Domain.Concerns.SoftDeletable;
using Shared.Application.Domain.Models;

namespace Module.Shipping.Domain.ShippingMethods;
/// <summary>Represents a Shipping Method.</summary>

// Invariant: CalculatorType is required; AvailableToUsers implies not IsDeleted; Code is unique when set; TrackingUrl contains :tracking placeholder
public sealed partial class ShippingMethod : Entity, IAuditable, IParameterizable, ISoftDeletable
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? TrackingUrl { get; set; }
    public string? AdminName { get; set; }
    public int Position { get; set; }
    public bool AvailableToUsers { get; set; } = ShippingMethodConstant.Defaults.AvailableToUsers;
    public string CalculatorType { get; set; } = string.Empty;
    public Guid? TaxCategoryId { get; set; }
    public string? Presentation { get; set; }
    #endregion Properties

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Soft Deletion
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
    #endregion Soft Deletion

    #region Constructor
    internal ShippingMethod() { }
    #endregion Constructor
}