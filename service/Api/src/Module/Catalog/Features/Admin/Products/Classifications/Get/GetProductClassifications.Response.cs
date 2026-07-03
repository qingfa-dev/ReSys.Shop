using Module.Catalog.Features.Admin.Products.Classifications.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.Classifications.Get;

public static partial class GetProductClassifications
{
    public sealed record Response
    {
        public List<ClassificationItem> Items { get; init; } = [];

        public sealed record ClassificationItem : ClassificationItemResponse;
    }
}
