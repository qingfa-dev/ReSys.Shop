using Microsoft.AspNetCore.Mvc;

namespace Shared.Operational.Persistence.Specifications.Querying;

/// <summary>
/// Defines the flat query-string surface for a search expression.
/// </summary>
/// <remarks>
/// Implemented by any request DTO that accepts search input from the HTTP layer.
/// Search is always a case-insensitive substring (<em>contains</em>) match applied across
/// one or more named fields. For operator-specific matching, use <see cref="IFilteringParameters"/>.
/// <para>
/// Parsed by <c>SearchingModelExtensions.FromQueryString</c> in the handler.
/// </para>
/// </remarks>
public interface ISearchingParameters
{
    /// <summary>
    /// Gets the plain search term, or <see langword="null"/> when no search was requested.
    /// Example: <c>?search=bolt</c>
    /// </summary>
    [FromQuery(Name = "search")]
    string? Search { get; }

    /// <summary>
    /// Gets the explicit target field names to search across.
    /// When absent, the handler falls back to the entity's <c>AllowedSearchFields</c> constant.
    /// Example: <c>?searchFields=Name&amp;searchFields=Description</c>
    /// </summary>
    [FromQuery(Name = "searchFields")]
    string[]? SearchFields { get; }

    /// <summary>
    /// Gets the match mode controlling how the term is combined across multiple fields.
    /// Accepted values: <c>any</c> (default, OR semantics) or <c>all</c> (AND semantics).
    /// Example: <c>?searchMode=any</c>
    /// </summary>
    [FromQuery(Name = "searchMode")]
    string? SearchMode { get; }
}
