namespace Module.Catalog.Features.Admin.Products.Classifications.Shared.Models;

public abstract record ProductClassificationParameters
{
    public Guid TaxonId { get; init; }
    public int Position { get; init; }
}

public record ClassificationItemResponse : ProductClassificationParameters
{
    public string Name { get; init; } = string.Empty;
        public string? PrettyName { get; init; }
    public bool IsAssigned { get; init; }
}


public record ProductClassificationAssignmentItem : ProductClassificationParameters;