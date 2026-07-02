using Shared.Operational.Persistence.Specifications.Filtering;
using Shared.Operational.Persistence.Specifications.Filtering.Extensions;
using Shared.Operational.Persistence.Specifications.Paging;
using Shared.Operational.Persistence.Specifications.Searching;
using Shared.Operational.Persistence.Specifications.Sorting;

namespace Shared.Operational.Persistence.Specifications.Querying;

/// <summary>
/// Factory extensions that parse <see cref="QueryingParameters"/> into a fully validated <see cref="QueryingModel"/>.
/// </summary>
public static class QueryingParametersExtensions
{
    /// <summary>
    /// Parses all four querying concerns at once, returning a
    /// <see cref="QueryingModel"/> on success or all accumulated validation
    /// errors on failure.
    /// </summary>
    /// <param name="parameters">The raw query-string parameters.</param>
    /// <param name="allowedFilterFields">Optional whitelist of permitted filter field names.</param>
    /// <param name="allowedSearchFields">Optional whitelist of permitted search field names.</param>
    /// <param name="allowedSortFields">Optional whitelist of permitted sort field names.</param>
    /// <param name="pageBounds">Optional page bounds. Uses <see cref="PageBounds.Default"/> when <see langword="null"/>.</param>
    /// <returns>
    /// A <see cref="QueryingModel"/> on success, or a failure with accumulated errors
    /// when filter, search, or sort fail to parse.
    /// </returns>
    public static Result<QueryingModel> ParseAll(
        this QueryingParameters parameters,
        IReadOnlySet<string>? allowedFilterFields = null,
        IReadOnlySet<string>? allowedSearchFields = null,
        IReadOnlySet<string>? allowedSortFields = null,
        PageBounds? pageBounds = null)
    {
        List<Error> errors = [];

        // Parse: Filter
        Result<FilterModel> filterResult = FilterModelExtensions.FromString(
            parameters.Filter, allowedFilterFields);

        if (filterResult.IsFailure)
            errors.AddRange(filterResult.Errors);

        // Parse: Search
        Result<SearchModel> searchResult;
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            searchResult = SearchModelExtensions.FromQueryString(
                parameters.Search,
                parameters.SearchFields is { Length: > 0 } ? string.Join(",", parameters.SearchFields) : null,
                parameters.SearchMode,
                null,
                allowedSearchFields);
        }
        else
        {
            searchResult = SearchModel.Empty;
        }

        if (searchResult.IsFailure)
            errors.AddRange(searchResult.Errors);

        // Parse: Sort
        Result<SortModel> sortResult = SortModelExtensions.FromQueryString(
            parameters.Sort, allowedSortFields);

        if (sortResult.IsFailure)
            errors.AddRange(sortResult.Errors);

        // Parse: Page
        PageModel pageModel = PageModelExtensions.FromValues(
            parameters.PageNumber, parameters.PageSize, pageBounds).Value;

        // Aggregate: Return all errors if any, otherwise the full QueryingModel.
        return errors.Count > 0
            ? Result<QueryingModel>.Validation(errors: errors)
            : (Result<QueryingModel>)new QueryingModel(
            filterResult.Value,
            searchResult.Value,
            sortResult.Value,
            pageModel);
    }
}
