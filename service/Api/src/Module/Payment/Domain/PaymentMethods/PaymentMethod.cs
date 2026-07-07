using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Concerns.Parameterizable;
using Shared.Application.Domain.Concerns.SoftDeletable;
using Shared.Application.Domain.Models;

namespace Module.Payment.Domain.PaymentMethods;

/// <summary>Represents a payment method (e.g., credit card, check, gateway) with provider configuration and display preferences.</summary>
// @CAT-10 Invariant: Name is required and non-empty; ProviderType is required; Active defaults to true; Position >= 0
public sealed partial class PaymentMethod : Entity, IAuditable, IParameterizable, ISoftDeletable
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public bool Active { get; set; } = PaymentMethodConstant.Defaults.Active;
    public bool AutoCapture { get; set; } = PaymentMethodConstant.Defaults.AutoCapture;
    public DisplayOn DisplayOn { get; set; } = PaymentMethodConstant.Defaults.DisplayOn;
    public int Position { get; set; } = PaymentMethodConstant.Defaults.Position;
    public string? Presentation { get; set; }
    public Dictionary<string, string> Preferences { get; set; } = [];
    #endregion Properties

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Relationships
    public ICollection<Payments.Payment> Payments { get; set; } = [];
    #endregion Relationships

    #region Soft Deletion
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
    #endregion Soft Deletion

    #region Constructor
    internal PaymentMethod() { }
    #endregion Constructor
}