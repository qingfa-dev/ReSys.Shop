namespace Module.Catalog.Domain.Products.Variants;

public static class VariantNumberIdentifierExtensions
{
    // Compute: Return the best display identifier for the variant
    public static string DisplayNumber(this Variant variant)
    {
        if (!string.IsNullOrEmpty(variant.Sku))
        {
            return variant.Sku;
        }

        return $"V-{variant.Id:N}".ToUpperInvariant();
    }

    // Compute: Return a short display identifier for the variant
    public static string ShortDisplayNumber(this Variant variant)
    {
        if (!string.IsNullOrEmpty(variant.Sku))
        {
            return variant.Sku;
        }

        return variant.Id.ToString("N")[..8].ToUpperInvariant();
    }

    public static string NumberIdentifierPrefix => "V";

    // Generate: Create a new unique number identifier
    public static string GenerateNumberIdentifier()
    {
        return $"{NumberIdentifierPrefix}-{Guid.NewGuid():N}".ToUpperInvariant();
    }
}
