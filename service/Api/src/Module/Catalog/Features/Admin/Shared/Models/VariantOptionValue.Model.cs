namespace Module.Catalog.Features.Admin.Shared.Models;

public abstract record VariantOptionValueCollectionParameters
{
    public Guid VariantId { get; init; }
    public IEnumerable<Guid> OptionValueIds { get; init; } = [];
}
