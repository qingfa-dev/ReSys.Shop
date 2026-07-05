using Module.Inventory.Domain.StockLocations;

namespace Module.Inventory.Features.Admin.StockLocations.Shared.Models;

public abstract record class StockLocationParameters
{
    // Validate: Name is required and limited to max length
    public string Name { get; init; } = string.Empty;
    public string? Presentation { get; init; }
    public string? Code { get; init; }
    public string? Address1 { get; init; }
    public string? Address2 { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
    public string? Phone { get; init; }
    public bool Active { get; init; } = StockLocationConstant.Defaults.Active;
    public bool Default { get; init; } = StockLocationConstant.Defaults.Default;
    public bool BackorderableDefault { get; init; } = StockLocationConstant.Defaults.BackorderableDefault;
    public bool PropagateAllVariants { get; init; } = StockLocationConstant.Defaults.PropagateAllVariants;
    public int Position { get; init; }
}
