namespace Module.Catalog.Features.Admin.Products.Classifications.Shared.Models;

public abstract record ProductClassificationParameters
{
    public Guid TaxonId { get; init; }
    public int Position { get; init; }
}

public record ProductClassificationAssignmentItem : ProductClassificationParameters;
