using System.Globalization;

namespace Module.Catalog.Domain.Taxons;

public record TaxonTranslation
{
    public string Name { get; init; } = string.Empty;
    public string? PrettyName { get; init; }
    public string? Description { get; init; }
    public string? Permalink { get; init; }
    public string CultureCode { get; init; } = "en";
}

public static class TaxonTranslatableExtensions
{
    private static readonly Dictionary<Guid, List<TaxonTranslation>> TranslationsStore = new();

    // Add: Associate a translation with the taxon for the given culture
    public static void AddTranslation(this Taxon taxon, TaxonTranslation translation)
    {
        if (!TranslationsStore.TryGetValue(taxon.Id, out var translations))
        {
            translations = new List<TaxonTranslation>();
            TranslationsStore[taxon.Id] = translations;
        }

        translations.Add(translation);
    }

    // Filter: Retrieve a specific translation by culture code
    public static TaxonTranslation? GetTranslation(this Taxon taxon, string cultureCode)
    {
        if (!TranslationsStore.TryGetValue(taxon.Id, out var translations))
        {
            return null;
        }

        return translations.FirstOrDefault(t =>
            t.CultureCode.Equals(cultureCode, StringComparison.OrdinalIgnoreCase));
    }

    // Fallback: Return the requested translation or the first available
    public static TaxonTranslation? GetTranslationOrDefault(this Taxon taxon, string? cultureCode = null)
    {
        var code = cultureCode ?? CultureInfo.CurrentCulture.Name;

        return taxon.GetTranslation(code)
            ?? TranslationsStore.GetValueOrDefault(taxon.Id)?.FirstOrDefault();
    }

    // Compute: Get translated name with fallback to the default name
    public static string TranslatedName(this Taxon taxon, string? cultureCode = null)
    {
        return taxon.GetTranslationOrDefault(cultureCode)?.Name ?? taxon.Name;
    }

    // Compute: Get translated description with fallback to the default
    public static string TranslatedDescription(this Taxon taxon, string? cultureCode = null)
    {
        return taxon.GetTranslationOrDefault(cultureCode)?.Description ?? taxon.Description ?? string.Empty;
    }
}