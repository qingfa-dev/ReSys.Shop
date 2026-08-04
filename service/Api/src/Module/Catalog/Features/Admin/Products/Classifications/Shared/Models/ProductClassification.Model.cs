namespace Module.Catalog.Features.Admin.Products.ProductClassifications.Shared.Models;

public abstract record ProductClassificationParameters
{
    public Guid TaxonId { get; init; }
    public int Position { get; init; }
}

public record ProductClassificationAssignmentItem : ProductClassificationParameters;