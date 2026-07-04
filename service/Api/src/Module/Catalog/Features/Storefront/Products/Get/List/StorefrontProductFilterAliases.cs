using System.Linq.Expressions;

using Module.Catalog.Domain.Products;

namespace Module.Catalog.Features.Storefront.Products.Get.List;

public static class StorefrontProductFilterAliases
{
    public static readonly IReadOnlyList<IStorefrontProductAlias> All =
    [
        new OptionValueAlias(),
        new OptionTypeAlias(),
        new TaxonAlias(),
        new MinPriceAlias(),
        new MaxPriceAlias(),
    ];

    public static IReadOnlySet<string> CanonicalFields { get; } = All
        .SelectMany(a => a.CanonicalPaths)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static string BuildFilter(ListProducts.Parameters parameters)
    {
        List<string> conditions = [];

        foreach (IStorefrontProductAlias alias in All)
        {
            string? rendered = alias.RenderDslFragment(parameters);
            if (!string.IsNullOrEmpty(rendered))
                conditions.Add(rendered);
        }

        return string.Join(",", conditions);
    }
}

public interface IStorefrontProductAlias
{
    IReadOnlyList<string> CanonicalPaths { get; }

    Expression<Func<Product, bool>>? BuildPredicate(ListProducts.Parameters parameters);

    string? RenderDslFragment(ListProducts.Parameters parameters);
}

internal sealed class OptionValueAlias : IStorefrontProductAlias
{
    public IReadOnlyList<string> CanonicalPaths { get; } =
        ["Variants.OptionValueVariants.OptionValue.Name"];

    public Expression<Func<Product, bool>>? BuildPredicate(ListProducts.Parameters parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.OptionValue))
            return null;

        string needle = parameters.OptionValue;
        return product => product.Variants
            .Any(v => v.OptionValueVariants
                .Any(ov => ov.OptionValue != null
                    && EF.Functions.ILike(ov.OptionValue.Name, $"%{needle}%")));
    }

    public string? RenderDslFragment(ListProducts.Parameters parameters) =>
        string.IsNullOrWhiteSpace(parameters.OptionValue)
            ? null
            : $"{CanonicalPaths[0]}=*{parameters.OptionValue}*";
}

internal sealed class OptionTypeAlias : IStorefrontProductAlias
{
    public IReadOnlyList<string> CanonicalPaths { get; } =
        ["Variants.OptionValueVariants.OptionValue.OptionType.Name"];

    public Expression<Func<Product, bool>>? BuildPredicate(ListProducts.Parameters parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.OptionType))
            return null;

        string needle = parameters.OptionType;
        return product => product.Variants
            .Any(v => v.OptionValueVariants
                .Any(ov => ov.OptionValue != null
                    && ov.OptionValue.OptionType != null
                    && EF.Functions.ILike(ov.OptionValue.OptionType.Name, $"%{needle}%")));
    }

    public string? RenderDslFragment(ListProducts.Parameters parameters) =>
        string.IsNullOrWhiteSpace(parameters.OptionType)
            ? null
            : $"{CanonicalPaths[0]}=*{parameters.OptionType}*";
}

internal sealed class TaxonAlias : IStorefrontProductAlias
{
    public IReadOnlyList<string> CanonicalPaths { get; } =
        ["Classifications.Taxon.Name"];

    public Expression<Func<Product, bool>>? BuildPredicate(ListProducts.Parameters parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.Taxon))
            return null;

        string needle = parameters.Taxon;
        return product => product.Classifications
            .Any(c => c.Taxon != null
                && EF.Functions.ILike(c.Taxon.Name, $"%{needle}%"));
    }

    public string? RenderDslFragment(ListProducts.Parameters parameters) =>
        string.IsNullOrWhiteSpace(parameters.Taxon)
            ? null
            : $"{CanonicalPaths[0]}=*{parameters.Taxon}*";
}

internal sealed class MinPriceAlias : IStorefrontProductAlias
{
    public IReadOnlyList<string> CanonicalPaths { get; } =
        ["Variants.Prices.Amount"];

    public Expression<Func<Product, bool>>? BuildPredicate(ListProducts.Parameters parameters)
    {
        if (!parameters.MinPrice.HasValue)
            return null;

        decimal min = parameters.MinPrice.Value;
        return product => product.Variants
            .Any(v => v.Prices
                .Any(p => p.Amount >= min));
    }

    public string? RenderDslFragment(ListProducts.Parameters parameters) =>
        parameters.MinPrice.HasValue
            ? $"{CanonicalPaths[0]}>={parameters.MinPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
            : null;
}

internal sealed class MaxPriceAlias : IStorefrontProductAlias
{
    public IReadOnlyList<string> CanonicalPaths { get; } =
        ["Variants.Prices.Amount"];

    public Expression<Func<Product, bool>>? BuildPredicate(ListProducts.Parameters parameters)
    {
        if (!parameters.MaxPrice.HasValue)
            return null;

        decimal max = parameters.MaxPrice.Value;
        return product => product.Variants
            .Any(v => v.Prices
                .Any(p => p.Amount <= max));
    }

    public string? RenderDslFragment(ListProducts.Parameters parameters) =>
        parameters.MaxPrice.HasValue
            ? $"{CanonicalPaths[0]}<={parameters.MaxPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
            : null;
}
