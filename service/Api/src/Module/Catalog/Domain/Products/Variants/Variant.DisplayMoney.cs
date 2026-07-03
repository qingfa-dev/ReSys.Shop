using System.Globalization;

namespace Module.Catalog.Domain.Products.Variants;

public static class VariantDisplayMoneyExtensions
{
    // Compute: Display price formatted in the variant's currency
    public static string DisplayPrice(this Variant variant, string? currency = null)
    {
        var cur = currency ?? variant.CostCurrency ?? "USD";
        var amount = variant.Price ?? 0m;
        return FormatCurrency(amount, cur);
    }

    // Compute: Display cost price formatted in the variant's currency
    public static string DisplayCostPrice(this Variant variant, string? currency = null)
    {
        var cur = currency ?? variant.CostCurrency ?? "USD";
        var amount = variant.CostPrice ?? 0m;
        return FormatCurrency(amount, cur);
    }

    // Compute: Display compare-at price formatted in the variant's currency
    public static string DisplayCompareAtPrice(this Variant variant, string? currency = null)
    {
        var cur = currency ?? variant.CostCurrency ?? "USD";
        var price = variant.Prices.FirstOrDefault(p => p.Currency == cur);
        var amount = price?.CompareAtAmount ?? 0m;
        return FormatCurrency(amount, cur);
    }

    // Format: Format amount as currency string using the appropriate culture
    private static string FormatCurrency(decimal amount, string currencyCode)
    {
        try
        {
            var culture = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .FirstOrDefault(c =>
                {
                    try
                    {
                        var region = new RegionInfo(c.Name);
                        return region.ISOCurrencySymbol == currencyCode;
                    }
                    catch
                    {
                        return false;
                    }
                });

            if (culture != null)
            {
                return string.Format(culture, "{0:C}", amount);
            }
        }
        catch
        {
        }

        return $"{amount:N2} {currencyCode}";
    }
}
