using Module.Catalog.Features.Admin.Products.Shared.Models;
using Module.Catalog.Features.Storefront.Classifications.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Shared.Models;

#region List Response

public record StoreProductListItemResponse : ProductListItemResponse
{
    #region Storefront
    public StoreProductVariantResponse? MasterVariant { get; init; }
    public List<StoreTaxonListItemResponse> Classifications { get; init; } = [];
    #endregion
}

#endregion

#region Detail Response

public record StoreProductDetailResponse : ProductDetailResponse
{
    #region Storefront
    public StoreProductVariantResponse? MasterVariant { get; init; }
    public List<StoreProductVariantResponse> Variants { get; init; } = [];
    public List<StoreTaxonListItemResponse> Classifications { get; init; } = [];
    public int VariantsCount { get; init; }
    #endregion
}

#endregion
