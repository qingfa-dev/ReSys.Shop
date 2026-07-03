namespace Module.Catalog.Features.Admin.Products.Variants.OptionValues.Assign;

public static partial class AssignVariantOptionValues
{
    public sealed record Request
    {
        public IEnumerable<Guid> OptionValueIds { get; init; } = [];
    }
}
