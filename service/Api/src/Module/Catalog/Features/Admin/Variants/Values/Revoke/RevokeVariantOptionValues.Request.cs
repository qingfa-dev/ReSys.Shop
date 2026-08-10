namespace Module.Catalog.Features.Admin.Variants.Values.Revoke;

public static partial class RevokeVariantOptionValues
{
    public sealed record Request
    {
        public Guid VariantId { get; init; }
        public IEnumerable<Guid> OptionValueIds { get; init; } = [];
    }
}