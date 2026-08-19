namespace Module.Catalog.Features.Admin.Shared.Models;

public abstract record ProductOptionTypeParameters
{
    public Guid OptionTypeId { get; init; }
    public int Position { get; init; }
}

public abstract record ProductOptionTypeCollectionParameters
{
    public Guid ProductId { get; init; }
    public IEnumerable<ProductOptionTypeAssignmentItem> Items { get; init; } = [];
}

public record ProductOptionTypeAssignmentItem : ProductOptionTypeParameters;
