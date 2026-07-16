using Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.OptionTypes.Get;

public static partial class GetProductOptionTypes
{
    // EXCEPTION: collection wrapper — inner items are OptionTypeDetailResponse
    public sealed record Response
    {
        public List<OptionTypeItem> Items { get; init; } = [];

        public sealed record OptionTypeItem : ProductOptionTypeItemResponse;
    }
}