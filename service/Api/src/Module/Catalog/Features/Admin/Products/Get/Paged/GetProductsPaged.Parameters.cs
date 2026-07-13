using Module.Catalog.Domain.Products;

namespace Module.Catalog.Features.Admin.Products.Get.Paged;

public static partial class GetProductsPagedList
{
    public record Parameters : QueryingParameters
    {
        public ProductStatus? Status { get; init; }
        public Guid? TaxonId { get; init; }
        public string? Season { get; init; }
    }
}