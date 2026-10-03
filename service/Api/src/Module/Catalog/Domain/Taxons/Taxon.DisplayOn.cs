namespace Module.Catalog.Domain.Taxons;

public enum TaxonDisplayContext
{
    All,
    Storefront,
    Admin,
    None,
}

public static class TaxonDisplayOnExtensions
{
    // Check: Determine if taxon is visible in the given display context
    public static bool IsVisibleIn(this Taxon taxon, TaxonDisplayContext context)
    {
        if (taxon.HideFromNav && context == TaxonDisplayContext.Storefront)
        {
            return false;
        }

        return context switch
        {
            TaxonDisplayContext.None => false,
            TaxonDisplayContext.All => !taxon.HideFromNav,
            _ => true,
        };
    }

    // Check: Determine if taxon is visible on the storefront
    public static bool IsVisibleInStorefront(this Taxon taxon)
    {
        return !taxon.HideFromNav;
    }

    // Check: Taxons are always visible in the admin panel
    public static bool IsVisibleInAdmin(this Taxon taxon)
    {
        return true;
    }
}