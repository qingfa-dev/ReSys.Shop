namespace Module.Catalog.Features.Storefront.Products.Get.Collections.Shared.Models;

public class StoreCollectionPageResponse
{
    public string Season { get; init; } = string.Empty;
    public string? StyleCode { get; init; }
    public int TotalProducts { get; init; }
}
