namespace Module.Catalog.Features.Admin.Variants.Shared.Models;

public record VariantRequest : VariantParameters
{
    public bool IsMaster { get; init; }
    public List<Guid>? OptionValueIds { get; init; }
}