using Microsoft.AspNetCore.Mvc;

namespace Shared.Operational.Persistence.Specifications.Querying;

/// <summary>
/// Defines the flat query-string surface for a sort expression.
/// </summary>
/// <remarks>
/// Implemented by any request DTO that accepts sort input from the HTTP layer.
/// Each value in <see cref="Sort"/> is one sort clause in one of three forms:
/// <list type="bullet">
///   <item><description>Bare field: <c>Name</c> (defaults to ascending)</description></item>
///   <item><description>Colon-separated: <c>Name:asc</c> or <c>CreatedAtUtc:desc</c></description></item>
///   <item><description>Direction prefix: <c>+Name</c> (ascending) or <c>-CreatedAtUtc</c> (descending)</description></item>
/// </list>
/// Multiple values are accepted and set sort priority in the order they appear.
/// Parsed by <c>SortModelExtensions.FromQueryString</c> in the handler.
/// </remarks>
public interface ISortingParameters
{
    /// <summary>
    /// Gets the sort clause values, or <see langword="null"/> when no sort was requested.
    /// Example: <c>?sort=Name:asc&amp;sort=-CreatedAtUtc</c>
    /// </summary>
    [FromQuery(Name = "sort")]
    string[]? Sort { get; }
}
