namespace Module.Catalog.Domain.Products;

public static partial class ProductMethod
{
    #region Search Methods
    public static string SearchIndexText(this Product product)
    {
        var parts = new List<string>
        {
            product.Name,
            product.Description,
            product.Slug,
        };

        if (!string.IsNullOrEmpty(product.MetaKeywords))
        {
            parts.Add(product.MetaKeywords);
        }

        if (!string.IsNullOrEmpty(product.MetaDescription))
        {
            parts.Add(product.MetaDescription);
        }

        return string.Join(" ", parts.Where(p => !string.IsNullOrEmpty(p)));
    }

    public static string[] SearchTokens(this Product product)
    {
        return product.SearchIndexText()
            .ToLowerInvariant()
            .Split([' ', ',', '.', '-', '_', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries)
            .Distinct()
            .ToArray();
    }

    public static bool MatchesSearchQuery(this Product product, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        var lowerQuery = query.ToLowerInvariant();
        var indexText = product.SearchIndexText().ToLowerInvariant();

        return indexText.Contains(lowerQuery);
    }
    #endregion
}