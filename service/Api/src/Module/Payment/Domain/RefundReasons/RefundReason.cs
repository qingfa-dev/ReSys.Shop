using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Concerns.Parameterizable;
using Shared.Application.Domain.Concerns.SoftDeletable;
using Shared.Application.Domain.Models;

namespace Module.Payment.Domain.RefundReasons;

/// <summary>Represents a reason for issuing a refund, supporting activation, parameterization, and soft deletion.</summary>
// Invariant: Name is required and non-empty; Active defaults to true; Code is unique when set
public sealed partial class RefundReason : Entity, IAuditable, IParameterizable, ISoftDeletable
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Presentation { get; set; }
    public bool Active { get; set; }
    #endregion Properties

    #region Soft Deletion
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
    #endregion Soft Deletion

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Constructor
    internal RefundReason() { }
    #endregion Constructor
}