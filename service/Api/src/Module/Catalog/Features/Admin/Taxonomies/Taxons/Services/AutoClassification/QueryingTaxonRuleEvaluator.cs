using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Domain.Taxonomies.Taxons.Rules;
using Module.Catalog.Features.Admin.Taxons.Services.AutoClassification.Abstractions;

using Shared.Operational.Persistence.Specifications.Helpers;

namespace Module.Catalog.Features.Admin.Taxons.Services.AutoClassification;

/// <summary>
/// Expression-tree based evaluation of a product against a taxon's rule set.
/// Leverages BuildingBlocks.Querying.Helpers for expression caching.
/// Dual-layer caching: Caches expression trees (QueryHelper) and compiled delegates (local ConcurrentDictionary).
/// </summary>
public sealed class QueryingTaxonRuleEvaluator : ITaxonRuleEvaluator
{
    private static readonly ConcurrentDictionary<Guid, Func<Product, bool>> CompiledRulesCache = new();
    private static readonly ConcurrentDictionary<Guid, DateTimeOffset?> TaxonModifiedCache = new();

    /// <summary>Evaluates a product against a taxon's rule set using cached expression trees for high throughput.</summary>
    /// <param name="product">The product to evaluate.</param>
    /// <param name="taxon">The taxon with automatic classification rules.</param>
    /// <returns>True if the product matches the taxon's rule set; otherwise false.</returns>
    public bool Evaluate(Product product, Taxon taxon)
    {
        // Guard: Skip non-automatic taxons or those without rules
        if (!taxon.Automatic || taxon.TaxonRules.Count == 0)
        {
            return false;
        }

        // Cache: Atomic retrieve or rebuild the compiled delegate if the taxon was modified
        if (!CompiledRulesCache.TryGetValue(taxon.Id, out var compiled) ||
            !TaxonModifiedCache.TryGetValue(taxon.Id, out var modifiedAt) ||
            modifiedAt != taxon.ModifiedAtUtc)
        {
            compiled = BuildAndCacheEvaluator(taxon);
        }

        return compiled?.Invoke(product) ?? false;
    }

    private static Func<Product, bool>? BuildAndCacheEvaluator(Taxon taxon)
    {
        // Compute: Unique cache key based on identity and modification timestamp
        string cacheKey = $"Taxon_{taxon.Id}_{taxon.ModifiedAtUtc?.Ticks ?? 0}";

        // Call: Retrieve or generate the LambdaExpression from Querying building block
        var lambda = QueryHelper.GetCachedExpression<Product>(
            cacheKey,
            "TaxonRuleEvaluation",
            () => BuildTaxonExpression(taxon));

        if (lambda is Expression<Func<Product, bool>> expr)
        {
            // Compute: Compile the expression into a high-performance delegate
            var compiled = expr.Compile();

            // Store: Update local caches
            CompiledRulesCache[taxon.Id] = compiled;
            TaxonModifiedCache[taxon.Id] = taxon.ModifiedAtUtc;

            return compiled;
        }

        return null;
    }

    private static Expression<Func<Product, bool>>? BuildTaxonExpression(Taxon taxon)
    {
        ParameterExpression param = Expression.Parameter(typeof(Product), "p");
        Expression? body = null;

        // Traverse: Process each rule to build the combined boolean expression
        foreach (var rule in taxon.TaxonRules)
        {
            Expression? condition = BuildRuleCondition(param, rule);
            if (condition == null) continue;

            // Merge: Combine conditions using the Taxon's MatchPolicy (All = AND, Any = OR)
            if (body == null)
            {
                body = condition;
            }
            else
            {
                body = taxon.RulesMatchPolicy == TaxonMatchPolicy.All
                    ? Expression.AndAlso(body, condition)
                    : Expression.OrElse(body, condition);
            }
        }

        return body == null ? null : Expression.Lambda<Func<Product, bool>>(body, param);
    }

    private static Expression? BuildRuleCondition(ParameterExpression param, TaxonRule rule)
    {
        // Route: Evaluation logic based on rule type
        return rule.Type switch
        {
            TaxonRuleType.ProductName => BuildStringComparison(Expression.Property(param, "Name"), rule),
            TaxonRuleType.ProductDescription => BuildStringComparison(Expression.Property(param, "Description"), rule),
            TaxonRuleType.ProductStatus => BuildEnumComparison(Expression.Property(param, "Status"), rule),
            TaxonRuleType.ProductAvailable => BuildBooleanStatusComparison(Expression.Property(param, "Status"), rule, ProductStatus.Active),
            TaxonRuleType.ProductArchived => BuildBooleanStatusComparison(Expression.Property(param, "Status"), rule, ProductStatus.Archived),
            TaxonRuleType.ProductSku => BuildMasterVariantStringComparison(param, "Sku", rule),
            TaxonRuleType.ProductPrice => BuildMasterVariantDecimalComparison(param, "Price", rule),
            TaxonRuleType.ProductWeight => BuildMasterVariantDecimalComparison(param, "Weight", rule),
            TaxonRuleType.VariantSku => BuildAnyVariantStringComparison(param, "Sku", rule),
            TaxonRuleType.VariantPrice => BuildAnyVariantDecimalComparison(param, "Price", rule),
            _ => null
        };
    }

    private static Expression? BuildStringComparison(Expression left, TaxonRule rule)
    {
        ConstantExpression right = Expression.Constant(rule.Value, typeof(string));

        MethodInfo equalsMethod = typeof(string).GetMethod("Equals", [typeof(string), typeof(StringComparison)])!;
        MethodInfo containsMethod = typeof(string).GetMethod("Contains", [typeof(string), typeof(StringComparison)])!;
        MethodInfo startsWithMethod = typeof(string).GetMethod("StartsWith", [typeof(string), typeof(StringComparison)])!;
        MethodInfo endsWithMethod = typeof(string).GetMethod("EndsWith", [typeof(string), typeof(StringComparison)])!;
        ConstantExpression comparison = Expression.Constant(StringComparison.OrdinalIgnoreCase);

        Expression? expr = rule.MatchPolicy switch
        {
            TaxonRuleMatchPolicy.IsEqualTo => Expression.Call(left, equalsMethod, right, comparison),
            TaxonRuleMatchPolicy.IsNotEqualTo => Expression.Not(Expression.Call(left, equalsMethod, right, comparison)),
            TaxonRuleMatchPolicy.Contains => Expression.Call(left, containsMethod, right, comparison),
            TaxonRuleMatchPolicy.DoesNotContain => Expression.Not(Expression.Call(left, containsMethod, right, comparison)),
            TaxonRuleMatchPolicy.StartsWith => Expression.Call(left, startsWithMethod, right, comparison),
            TaxonRuleMatchPolicy.EndsWith => Expression.Call(left, endsWithMethod, right, comparison),
            TaxonRuleMatchPolicy.IsNull => Expression.OrElse(
                Expression.Equal(left, Expression.Constant(null, typeof(string))),
                Expression.Equal(left, Expression.Constant(string.Empty, typeof(string)))
            ),
            TaxonRuleMatchPolicy.IsNotNull => Expression.AndAlso(
                Expression.NotEqual(left, Expression.Constant(null, typeof(string))),
                Expression.NotEqual(left, Expression.Constant(string.Empty, typeof(string)))
            ),
            TaxonRuleMatchPolicy.In => BuildInComparison(left, rule.Value),
            TaxonRuleMatchPolicy.NotIn => Expression.Not(BuildInComparison(left, rule.Value)),
            _ => null
        };

        // Guard: Handle null string on non-null check policies
        if (expr != null && rule.MatchPolicy != TaxonRuleMatchPolicy.IsNull && rule.MatchPolicy != TaxonRuleMatchPolicy.IsNotNull)
        {
            var notNull = Expression.NotEqual(left, Expression.Constant(null, typeof(string)));
            expr = Expression.AndAlso(notNull, expr);
        }

        return expr;
    }

    private static Expression BuildInComparison(Expression left, string value)
    {
        string[] items = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (items.Length == 0) return Expression.Constant(false);

        MethodInfo equalsMethod = typeof(string).GetMethod("Equals", [typeof(string), typeof(StringComparison)])!;
        ConstantExpression comparison = Expression.Constant(StringComparison.OrdinalIgnoreCase);

        Expression? combined = null;
        foreach (var item in items)
        {
            var eq = Expression.Call(left, equalsMethod, Expression.Constant(item, typeof(string)), comparison);
            combined = combined == null ? eq : Expression.OrElse(combined, eq);
        }

        return combined!;
    }

    private static Expression? BuildDecimalComparison(Expression left, TaxonRule rule)
    {
        if (rule.MatchPolicy == TaxonRuleMatchPolicy.IsNull)
            return Expression.Equal(left, Expression.Constant(null, left.Type));
        if (rule.MatchPolicy == TaxonRuleMatchPolicy.IsNotNull)
            return Expression.NotEqual(left, Expression.Constant(null, left.Type));

        if (!decimal.TryParse(rule.Value, out decimal expectedValue)) return Expression.Constant(false);

        ConstantExpression right = Expression.Constant(expectedValue, typeof(decimal));
        MemberExpression leftValue = Expression.Property(left, "Value");

        Expression? comparison = rule.MatchPolicy switch
        {
            TaxonRuleMatchPolicy.IsEqualTo => Expression.Equal(leftValue, right),
            TaxonRuleMatchPolicy.IsNotEqualTo => Expression.NotEqual(leftValue, right),
            TaxonRuleMatchPolicy.GreaterThan => Expression.GreaterThan(leftValue, right),
            TaxonRuleMatchPolicy.LessThan => Expression.LessThan(leftValue, right),
            TaxonRuleMatchPolicy.GreaterThanOrEqual => Expression.GreaterThanOrEqual(leftValue, right),
            TaxonRuleMatchPolicy.LessThanOrEqual => Expression.LessThanOrEqual(leftValue, right),
            _ => null
        };

        if (comparison == null) return null;

        // Guard: Ensure Nullable<decimal> has a value before comparison
        return Expression.AndAlso(Expression.Property(left, "HasValue"), comparison);
    }

    private static Expression? BuildEnumComparison(Expression left, TaxonRule rule)
    {
        if (!Enum.TryParse<ProductStatus>(rule.Value, true, out var expectedValue)) return Expression.Constant(false);
        ConstantExpression right = Expression.Constant(expectedValue, typeof(ProductStatus));

        return rule.MatchPolicy switch
        {
            TaxonRuleMatchPolicy.IsEqualTo => Expression.Equal(left, right),
            TaxonRuleMatchPolicy.IsNotEqualTo => Expression.NotEqual(left, right),
            _ => null
        };
    }

    private static Expression? BuildBooleanStatusComparison(Expression left, TaxonRule rule, ProductStatus targetStatus)
    {
        if (!bool.TryParse(rule.Value, out bool expectedValue)) return Expression.Constant(false);

        BinaryExpression isStatus = Expression.Equal(left, Expression.Constant(targetStatus, typeof(ProductStatus)));
        ConstantExpression expectedBool = Expression.Constant(expectedValue, typeof(bool));

        return rule.MatchPolicy switch
        {
            TaxonRuleMatchPolicy.IsEqualTo => Expression.Equal(isStatus, expectedBool),
            TaxonRuleMatchPolicy.IsNotEqualTo => Expression.NotEqual(isStatus, expectedBool),
            _ => null
        };
    }

    private static BinaryExpression? BuildMasterVariantStringComparison(ParameterExpression param, string propertyName, TaxonRule rule)
    {
        MemberExpression variantsProp = Expression.Property(param, "Variants");

        // Use: Correct FirstOrDefault overload with predicate
        MethodInfo firstOrDefaultMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "FirstOrDefault" &&
                       m.IsGenericMethod &&
                       m.GetParameters().Length == 2 &&
                       m.GetParameters()[1].ParameterType.Name.StartsWith("Func", StringComparison.Ordinal))
            .MakeGenericMethod(typeof(Variant));

        ParameterExpression vParam = Expression.Parameter(typeof(Variant), "v");
        Expression isMasterExpr = Expression.Equal(Expression.Property(vParam, "IsMaster"), Expression.Constant(true));
        LambdaExpression isMasterLambda = Expression.Lambda(isMasterExpr, vParam);

        MethodCallExpression masterVariant = Expression.Call(null, firstOrDefaultMethod, variantsProp, isMasterLambda);

        Expression? comparison = BuildStringComparison(Expression.Property(masterVariant, propertyName), rule);
        if (comparison == null) return null;

        // Guard: Check if master variant exists
        return Expression.AndAlso(Expression.NotEqual(masterVariant, Expression.Constant(null, typeof(Variant))), comparison);
    }

    private static BinaryExpression? BuildMasterVariantDecimalComparison(ParameterExpression param, string propertyName, TaxonRule rule)
    {
        MemberExpression variantsProp = Expression.Property(param, "Variants");

        // Use: Correct FirstOrDefault overload with predicate
        MethodInfo firstOrDefaultMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "FirstOrDefault" &&
                       m.IsGenericMethod &&
                       m.GetParameters().Length == 2 &&
                       m.GetParameters()[1].ParameterType.Name.StartsWith("Func", StringComparison.Ordinal))
            .MakeGenericMethod(typeof(Variant));

        ParameterExpression vParam = Expression.Parameter(typeof(Variant), "v");
        Expression isMasterExpr = Expression.Equal(Expression.Property(vParam, "IsMaster"), Expression.Constant(true));
        LambdaExpression isMasterLambda = Expression.Lambda(isMasterExpr, vParam);

        MethodCallExpression masterVariant = Expression.Call(null, firstOrDefaultMethod, variantsProp, isMasterLambda);

        Expression? comparison = BuildDecimalComparison(Expression.Property(masterVariant, propertyName), rule);
        if (comparison == null) return null;

        // Guard: Check if master variant exists
        return Expression.AndAlso(Expression.NotEqual(masterVariant, Expression.Constant(null, typeof(Variant))), comparison);
    }

    private static MethodCallExpression? BuildAnyVariantStringComparison(ParameterExpression param, string propertyName, TaxonRule rule)
    {
        ParameterExpression vParam = Expression.Parameter(typeof(Variant), "v");
        Expression? comparison = BuildStringComparison(Expression.Property(vParam, propertyName), rule);
        if (comparison == null) return null;

        MethodInfo anyMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(Variant));

        return Expression.Call(null, anyMethod, Expression.Property(param, "Variants"), Expression.Lambda(comparison, vParam));
    }

    private static MethodCallExpression? BuildAnyVariantDecimalComparison(ParameterExpression param, string propertyName, TaxonRule rule)
    {
        ParameterExpression vParam = Expression.Parameter(typeof(Variant), "v");
        Expression? comparison = BuildDecimalComparison(Expression.Property(vParam, propertyName), rule);
        if (comparison == null) return null;

        MethodInfo anyMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(Variant));

        return Expression.Call(null, anyMethod, Expression.Property(param, "Variants"), Expression.Lambda(comparison, vParam));
    }
}