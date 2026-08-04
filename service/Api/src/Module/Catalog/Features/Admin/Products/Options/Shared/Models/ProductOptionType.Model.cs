namespace Module.Catalog.Features.Admin.Products.Options.Shared.Models;

public abstract record ProductOptionTypeParameters
{
    public Guid OptionTypeId { get; init; }
    public int Position { get; init; }
}

public record ProductOptionTypeAssignmentItem : ProductOptionTypeParameters;