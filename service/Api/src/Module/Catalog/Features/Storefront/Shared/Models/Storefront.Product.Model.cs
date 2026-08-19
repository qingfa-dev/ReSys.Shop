using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Storefront.Shared.Models;

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
