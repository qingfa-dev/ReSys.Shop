using Module.Inventory.Domain.StockLocations;

namespace Module.Inventory.Features.Admin.Shared.Models;

public abstract record class StockLocationParameters : INamedParameters, IActivatableParameters, ISortableParameters
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

    bool IActivatableParameters.IsActive { get => Active; init => Active = value; }
}

public record StockLocationRequest : StockLocationParameters;

public record StockLocationDetailResponse : StockLocationParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}

public record StockLocationListItemResponse : StockLocationParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}
