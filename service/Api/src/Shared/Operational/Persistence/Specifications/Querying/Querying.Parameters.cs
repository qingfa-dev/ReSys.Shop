using Microsoft.AspNetCore.Mvc;

namespace Shared.Operational.Persistence.Specifications.Querying;

/// <summary>
/// A flat, fully bindable query-string parameter record that combines all four
/// querying concerns: filtering, searching, sorting, and pagination.
/// </summary>
/// <remarks>
/// <para>
/// Designed for use with <c>[AsParameters]</c> in minimal API endpoints. Because
/// <c>[AsParameters]</c> cannot bind nested complex objects, all four concerns are
/// represented as primitive types directly on this record.
/// </para>
/// <para>
/// Pass a <c>QueryingParameters</c> instance to the handler inside the feature
/// <c>Query</c> record. The handler calls <c>parameters.ParseAll(...)</c> (from
/// <c>QueryingParametersExtensions</c>) to convert the raw values into typed,
/// validated models in one step.
/// </para>
/// <para>
/// When only a subset of concerns is needed, implement the corresponding interface
/// directly on a custom request record:
/// <code>
/// public record Request : IFilteringParameters, IPagingParameters
/// {
///     public string? Filter   { get; init; }
///     public int?    Page     { get; init; }
///     public int?    PageSize { get; init; }
/// }
/// </code>
/// </para>
/// </remarks>
public record QueryingParameters :
    IFilteringParameters,
    ISearchingParameters,
    ISortingParameters,
    IPagingParameters
{
    #region Filter

    /// <inheritdoc cref="IFilteringParameters.Filter"/>
    /// <example><c>?filter=Name=*bolt*,IsDeleted=false</c></example>
    [FromQuery(Name = "filter")]
    public string? Filter { get; init; }

    #endregion Filter

    #region Search

    /// <inheritdoc cref="ISearchingParameters.Search"/>
    /// <example><c>?search=bolt</c></example>
    [FromQuery(Name = "search")]
    public string? Search { get; init; }

    /// <inheritdoc cref="ISearchingParameters.SearchFields"/>
    /// <example><c>?searchFields=Name&amp;searchFields=Description</c></example>
    [FromQuery(Name = "searchFields")]
    public string[]? SearchFields { get; init; }

    /// <inheritdoc cref="ISearchingParameters.SearchMode"/>
    /// <example><c>?searchMode=any</c></example>
    [FromQuery(Name = "searchMode")]
    public string? SearchMode { get; init; }

    #endregion Search

    #region Sort

    /// <inheritdoc cref="ISortingParameters.Sort"/>
    /// <example><c>?sort=Name:asc&amp;sort=-CreatedAtUtc</c></example>
    [FromQuery(Name = "sort")]
    public string[]? Sort { get; init; }

    #endregion Sort

    #region Page

    /// <inheritdoc cref="IPagingParameters.PageNumber"/>
    /// <example><c>?page=2</c></example>
    [FromQuery(Name = "page")]
    public int? PageNumber { get; init; }

    /// <inheritdoc cref="IPagingParameters.PageSize"/>
    /// <example><c>?pageSize=25</c></example>
    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; init; }

    #endregion Page

    #region Static Instances

    /// <summary>
    /// Returns a <see cref="QueryingParameters"/> with all properties set to
    /// <see langword="null"/> — equivalent to a request with no query-string parameters.
    /// </summary>
    public static QueryingParameters Empty { get; } = new();

    #endregion Static Instances
}
