namespace Module.Catalog.Features.Storefront.Products.Get.Similar;

public static partial class GetSimilarProducts
{
    public sealed record Parameters
    {
        public Guid Id { get; init; }
        public int TopK { get; init; } = 20;
    }
}
