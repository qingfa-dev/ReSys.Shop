namespace Module.Catalog.Features.Admin.Products.Variants.Get.PagedOrAll;

public static partial class GetVariantsPagedOrAll
{
    public sealed record Parameters : QueryingParameters
    {
        public Guid? ProductId { get; init; }
    }
}
