using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;

namespace Module.Payment.Domain.PaymentCaptureEvents;

/// <summary>Represents a capture event against a payment, recording the captured amount and associated payment reference.</summary>
// Invariant: Amount > 0; PaymentId != default; CreatedAtUtc is set on creation
public sealed partial class PaymentCaptureEvent : Entity, IAuditable
{
    #region Properties
    public decimal Amount { get; set; }
    public Guid PaymentId { get; set; }
    #endregion Properties

    #region Relationships
    public Payments.Payment Payment { get; set; } = null!;
    #endregion Relationships

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Constructor
    internal PaymentCaptureEvent() { }
    #endregion Constructor
}