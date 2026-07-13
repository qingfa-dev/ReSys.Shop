namespace Module.Catalog.Features.Admin.Products.Variants.OptionValues.Revoke;

public static partial class RevokeVariantOptionValues
{
    public sealed record Request
    {
        public IEnumerable<Guid> OptionValueIds { get; init; } = [];
    }
}