using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Concerns.Parameterizable;
using Shared.Application.Domain.Concerns.SoftDeletable;
using Shared.Application.Domain.Models;

using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockMovements;

namespace Module.Inventory.Domain.StockLocations;

/// <summary>
/// Represents a physical or virtual stock location where inventory is stored.
/// </summary>
// Invariant: Active is true when IsDeleted is false; only one location has Default=true
// Contract: pre=name != null, post=Id != Guid.Empty
// Boundary: Domain entity — do not import EF Core types below this line
public sealed partial class StockLocation : Entity, IAuditable, IParameterizable, ISoftDeletable
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public string? Presentation { get; set; }
    public string? Code { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public Guid? CountryId { get; set; }
    public Guid? StateId { get; set; }
    public bool Active { get; set; } = StockLocationConstant.Defaults.Active;
    public bool Default { get; set; } = StockLocationConstant.Defaults.Default;
    public bool BackorderableDefault { get; set; } = StockLocationConstant.Defaults.BackorderableDefault;
    public bool PropagateAllVariants { get; set; } = StockLocationConstant.Defaults.PropagateAllVariants;
    public string? AdminName { get; set; }
    public int Position { get; set; }
    public int LowStockThreshold { get; set; } = StockLocationConstant.Defaults.LowStockThreshold;
    public bool NotifyOnLowStock { get; set; } = StockLocationConstant.Defaults.NotifyOnLowStock;
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

    #region Navigation
    public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    // public ICollection<Shipment> Shipments { get; set; } = [];
    #endregion Navigation

    #region Constructor
    internal StockLocation() { }
    #endregion Constructor
}