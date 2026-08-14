using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;

using Module.Billing.Domain.PaymentMethods;

namespace Module.Billing.Domain.PaymentCaptures;

/// <summary>Represents a payment transaction within an order, managing state transitions, capture, and refund.</summary>
// @CAT-10 Invariant: Amount > 0; State progresses Checkout->Processing->Pending->Completed or ->Failed->Void; CapturedTotal <= Amount; RefundedTotal <= CapturedTotal
public sealed partial class PaymentCapture : Entity, IAuditable
{
    #region Properties
    public string Number { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = PaymentConstant.Defaults.Currency;
    public PaymentRecordState State { get; set; } = PaymentRecordState.Checkout;
    public string? ResponseCode { get; set; }
    public string? AvsResponse { get; set; }
    public string? CvvResponseCode { get; set; }
    public string? CvvResponseMessage { get; set; }
    public string? IntentClientSecret { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? PaymentStatus { get; set; }
    public decimal RefundedAmount { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    /// <summary>Stripe event IDs already processed for this payment — prevents duplicate webhook handling.</summary>
    public List<string> ProcessedStripeEventIds { get; set; } = [];
    /// <summary>The last Stripe webhook event applied to this payment (observability + reconciliation).</summary>
    public string? LastStripeEventId { get; set; }
    /// <summary>Stripe creation time of the last applied webhook event — used to drop stale out-of-order events.</summary>
    public DateTimeOffset? LastStripeEventCreatedAtUtc { get; set; }
    /// <summary>When the payment transitioned to Completed.</summary>
    public DateTimeOffset? CompletedAtUtc { get; set; }
    /// <summary>When the payment transitioned to Failed.</summary>
    public DateTimeOffset? FailedAtUtc { get; set; }
    /// <summary>When the payment transitioned to Void.</summary>
    public DateTimeOffset? VoidedAtUtc { get; set; }
    /// <summary>When the payment transitioned to Disputed.</summary>
    public DateTimeOffset? DisputedAtUtc { get; set; }
    /// <summary>When a refund was last recorded for this payment.</summary>
    public DateTimeOffset? RefundedAtUtc { get; set; }
    /// <summary>Optimistic concurrency token — prevents race conditions between admin operations and webhooks.</summary>
    public uint RowVersion { get; set; }
    #endregion Properties

    #region Identifiers
    public Guid? PaymentMethodId { get; set; }
    public Guid OrderId { get; set; }
    public string? SourceId { get; set; }
    public string? SourceType { get; set; }
    #endregion Identifiers

    #region Relationships
    public PaymentMethod PaymentMethod { get; set; } = null!;
    #endregion Relationships

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Constructor
    internal PaymentCapture() { }
    #endregion Constructor
}