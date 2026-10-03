namespace Module.Catalog.Domain.Taxons;

public enum TaxonMatchPolicy
{
    All,
    Any,
}
public enum TaxonSortOrder
{
    Manual,
    BestSelling,
    AlphabeticallyAZ,
    AlphabeticallyZA,
    PriceHigh2Low,
    PriceLow2High,
    Newest,
    Oldest,
}