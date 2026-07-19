using Shared.Operational.Persistence.Specifications.Filtering;
using Shared.Operational.Persistence.Specifications.Paging;
using Shared.Operational.Persistence.Specifications.Searching;
using Shared.Operational.Persistence.Specifications.Sorting;

namespace Shared.Operational.Persistence.Specifications.Querying;

/// <summary>
/// A fully parsed, validated record carrying all four querying concern models.
/// Produced by <see cref="QueryingParametersExtensions.ParseAll"/>.
/// </summary>
public sealed record QueryingModel
{
    /// <summary>The parsed filter model, or <see cref="FilterModel.Empty"/>.</summary>
    public FilterModel Filter { get; init; } = default!;

    /// <summary>The parsed search model, or <see cref="SearchModel.Empty"/>.</summary>
    public SearchModel Search { get; init; } = default!;

    /// <summary>The parsed sort model, or <see cref="SortModel.Empty"/>.</summary>
    public SortModel Sort { get; init; } = default!;

    /// <summary>The parsed page model, or <see cref="PageModel.Empty"/>.</summary>
    public PageModel Page { get; init; } = default!;

    /// <summary>
    /// Returns a <see cref="QueryingModel"/> with all four concern models set to their
    /// respective empty sentinels — equivalent to a request with no query-string parameters.
    /// </summary>
    public static QueryingModel Empty { get; } = new()
    {
        Filter = FilterModel.Empty,
        Search = SearchModel.Empty,
        Sort = SortModel.Empty,
        Page = PageModel.Empty
    };
}