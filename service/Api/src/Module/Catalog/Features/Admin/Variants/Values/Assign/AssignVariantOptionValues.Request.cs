namespace Module.Catalog.Features.Admin.Variants.Values.Assign;

public static partial class AssignVariantOptionValues
{
    public sealed record Request
    {
        public Guid VariantId { get; init; }
        public IEnumerable<Guid> OptionValueIds { get; init; } = [];
    }
}