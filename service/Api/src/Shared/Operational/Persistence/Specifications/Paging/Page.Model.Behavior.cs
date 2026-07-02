namespace Shared.Operational.Persistence.Specifications.Paging;

/// <summary>
/// Computed properties and diagnostics for <see cref="PageModel"/>.
/// </summary>
public sealed partial class PageModel
{
    #region Derived

    /// <summary>
    /// Calculates the zero-based item offset for use with <c>Skip()</c>.
    /// </summary>
    public int Skip => (Page - 1) * PageSize;

    /// <summary>
    /// Returns the total number of pages for a given <paramref name="totalCount"/>.
    /// </summary>
    /// <param name="totalCount">The total number of matching items in the data source.</param>
    public int TotalPages(long totalCount)
        => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)totalCount / PageSize);

    /// <summary>
    /// Returns <see langword="true"/> if the given <paramref name="totalCount"/> has a next page
    /// beyond the current <see cref="Page"/>.
    /// </summary>
    public bool HasNextPage(long totalCount)
        => Page < TotalPages(totalCount);

    /// <summary>
    /// Returns <see langword="true"/> if the current <see cref="Page"/> is not the first page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    #endregion Derived

    /// <summary>Returns a diagnostic string showing page, size, and bounds.</summary>
    public override string ToString() =>
        IsEmpty
            ? $"PageModel(empty, bounds={Bounds.DefaultPage}/{Bounds.DefaultPageSize}/{Bounds.MaxPageSize})"
            : $"PageModel(page={Page}, size={PageSize}, bounds=.../{Bounds.DefaultPageSize}/{Bounds.MaxPageSize})";
}
