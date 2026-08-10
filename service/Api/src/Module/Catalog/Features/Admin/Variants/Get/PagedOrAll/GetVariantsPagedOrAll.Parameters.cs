namespace Module.Catalog.Features.Admin.Variants.Get.PagedOrAll;

public static partial class GetVariantsPagedOrAll
{
    public sealed record Parameters : QueryingParameters
    {
        public Guid? ProductId { get; init; }
    }
}
