namespace Module.Catalog.Features.Admin.Variants.Values.Shared.Models;


public abstract record VariantOptionValueCollectionParameters
{
    public Guid VariantId { get; init; }
    public IEnumerable<Guid> OptionValueIds { get; init; } = [];
}
