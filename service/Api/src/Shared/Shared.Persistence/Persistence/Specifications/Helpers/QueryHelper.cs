using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using Shared.Governance.Conventions;

namespace Shared.Operational.Persistence.Specifications.Helpers;

/// <summary>
/// High-performance property resolver and expression builder with multi-level caching.
/// Supports case-insensitive matching and multiple naming conventions (snake_case, camelCase).
/// </summary>
public static class QueryHelper
{
    // Caches for PropertyInfo to avoid expensive repeated reflection across different naming styles
    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> ExactMatchCache = new();
    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> ResolvedCache = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> TypePropertiesCache = new();

    // Cache for compiled LambdaExpressions to avoid repeated DSL parsing and expression tree building
    // Key: (EntityType, OperationType, DSLString)
    private static readonly ConcurrentDictionary<(Type, string, string), LambdaExpression?> ExpressionCache = new();

    /// <summary>
    /// Resolves a PropertyInfo case-insensitively, supporting multiple naming conventions.
    /// </summary>
    /// <param name="type">The type to reflect upon.</param>
    /// <param name="propertyName">The name of the property to find.</param>
    /// <returns>The <see cref="PropertyInfo"/> if found, otherwise null.</returns>
    public static PropertyInfo? GetPropertyCaseInsensitive(Type type, string propertyName)
    {
        // Guard: Verify required parameters
        ArgumentNullException.ThrowIfNull(type);
        if (string.IsNullOrWhiteSpace(propertyName)) return null;

        // Normalize: Standardize input for cache lookup
        propertyName = propertyName.Trim();
        (Type type, string propertyName) exactKey = (type, propertyName);

        // Cache: Quick lookup for exact property matches in local cache
        if (ExactMatchCache.TryGetValue(exactKey, out PropertyInfo? cachedExact))
            return cachedExact;

        // Cache: Lookup for previously resolved heuristic matches (snake_case, etc.)
        if (ResolvedCache.TryGetValue(exactKey, out PropertyInfo? cachedResolved))
            return cachedResolved;

        // Call: Execute expensive reflection and convention matching logic
        PropertyInfo? result = ResolveProperty(type, propertyName);

        if (result != null)
        {
            // Cache: Store successful exact match for future hits
            ExactMatchCache.TryAdd(exactKey, result);
        }

        // Cache: Store result (even null) to prevent repeat resolution
        ResolvedCache.TryAdd(exactKey, result);
        return result;
    }

    /// <summary>
    /// Gets or creates a cached lambda expression for a specific query operation.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="cacheKey">A unique key representing the query logic (e.g., the filter string).</param>
    /// <param name="operationType">The type of operation (e.g., "Filter", "Search").</param>
    /// <param name="factory">The factory function to create the expression if not cached.</param>
    /// <returns>The cached or newly created <see cref="LambdaExpression"/>.</returns>
    public static LambdaExpression? GetCachedExpression<T>(string cacheKey, string operationType, Func<LambdaExpression?> factory)
    {
        (Type, string operationType, string cacheKey) key = (typeof(T), operationType, cacheKey);

        // Cache: Atomically retrieve or build the compiled expression tree
        return ExpressionCache.GetOrAdd(key, _ => factory());
    }

    /// <summary>
    /// Clears all internal caches.
    /// </summary>
    public static void ClearCache()
    {
        // Purge: Flush all concurrent resolution and expression caches
        ExactMatchCache.Clear();
        ResolvedCache.Clear();
        TypePropertiesCache.Clear();
        ExpressionCache.Clear();
    }

    private static PropertyInfo? ResolveProperty(Type type, string propertyName)
    {
        // Receive: Get all public instance properties for the type
        PropertyInfo[] properties = GetTypeProperties(type);

        // Check: Attempt case-insensitive exact match
        PropertyInfo? exact = Array.Find(properties, p =>
            string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

        if (exact != null) return exact;

        // Transform: Handle snake_case or kebab-case identifiers
        if (propertyName.Contains('_') || propertyName.Contains('-'))
        {
            var pascalCase = ConvertToPascalCase(propertyName);
            PropertyInfo? pascalMatch = Array.Find(properties, p =>
                string.Equals(p.Name, pascalCase, StringComparison.OrdinalIgnoreCase));

            if (pascalMatch != null) return pascalMatch;

            // Compute: Fallback to a "cleaned" string without separators
            var cleaned = propertyName.Replace("_", "").Replace("-", "");
            PropertyInfo? cleanedMatch = Array.Find(properties, p =>
                string.Equals(p.Name, cleaned, StringComparison.OrdinalIgnoreCase));

            if (cleanedMatch != null) return cleanedMatch;
        }

        // Transform: Handle camelCase to PascalCase transition
        if (propertyName.Length > 0 && char.IsLower(propertyName[0]))
        {
            // Compute: Capitalize first letter
            var pascalVersion = string.Concat(stackalloc[] { char.ToUpperInvariant(propertyName[0]) }, propertyName.AsSpan(1));
            PropertyInfo? camelMatch = Array.Find(properties, p =>
                string.Equals(p.Name, pascalVersion, StringComparison.OrdinalIgnoreCase));

            if (camelMatch != null) return camelMatch;
        }

        return null;
    }

    private static PropertyInfo[] GetTypeProperties(Type type)
    {
        // Cache: Retrieve properties for a type from memory
        return TypePropertiesCache.GetOrAdd(type, t =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
    }

    private static string ConvertToPascalCase(string input)
    {
        return input.ToPascalCase();
    }

    /// <summary>
    /// Gets the default value for a specified type.
    /// </summary>
    /// <param name="type">The type to get the default for.</param>
    /// <returns>The default value (null for reference types, zeroed instance for value types).</returns>
    public static object? GetDefault(Type type)
    {
        // Create: Instantiate default value for value types
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
