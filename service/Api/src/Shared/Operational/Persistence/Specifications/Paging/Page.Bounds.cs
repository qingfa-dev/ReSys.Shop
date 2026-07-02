namespace Shared.Operational.Persistence.Specifications.Paging;

/// <summary>
/// Defines the boundary constraints applied when normalizing a <see cref="PageModel"/>.
/// </summary>
/// <param name="DefaultPage">
/// The page number used when the caller supplies no page, or supplies a value ≤ 0.
/// Must be ≥ 1. Defaults to <c>1</c>.
/// </param>
/// <param name="DefaultPageSize">
/// The page size used when the caller supplies no size, or supplies a value ≤ 0.
/// Must be ≥ 1. Defaults to <c>10</c>.
/// </param>
/// <param name="MaxPageSize">
/// The hard ceiling on page size. Any caller-supplied size above this value is clamped
/// to this value. Must be ≥ 1. Defaults to <c>100</c>.
/// </param>
/// <remarks>
/// <c>PageBounds</c> replaces the static <c>PaginationConstants</c> class, allowing
/// different endpoints to enforce different limits without a global singleton:
/// <code>
/// // Standard endpoint — 10 items default, 100 max.
/// PageBounds.Default
///
/// // Admin export endpoint — 500 items default, 1000 max.
/// new PageBounds(DefaultPageSize: 500, MaxPageSize: 1000)
/// </code>
/// Pass a <c>PageBounds</c> instance into any <c>PageModelExtensions</c> factory method.
/// </remarks>
public sealed partial record PageBounds(
    int DefaultPage    = PageBounds.DefaultPageValue,
    int DefaultPageSize = PageBounds.DefaultPageSizeValue,
    int MaxPageSize    = PageBounds.DefaultMaxPageSizeValue)
{
    #region Constants

    public const int DefaultPageValue = 1;
    public const int DefaultPageSizeValue = 10;
    public const int DefaultMaxPageSizeValue = 100;

    public static readonly PageBounds Default = new();

    #endregion Constants

    #region Normalization

    /// <summary>
    /// Clamps the raw page number to a value ≥ <see cref="DefaultPage"/>.
    /// Null or ≤ 0 values fall back to <c>DefaultPage</c>.
    /// </summary>
    public int NormalizePage(int? page)
        => page is > 0 ? page.Value : DefaultPage;

    /// <summary>
    /// Clamps the raw page size to the range [1, <see cref="MaxPageSize"/>].
    /// Null or ≤ 0 values fall back to <c>DefaultPageSize</c>.
    /// </summary>
    public int NormalizePageSize(int? pageSize)
    {
        if (pageSize is not > 0) return DefaultPageSize;
        return pageSize.Value > MaxPageSize ? MaxPageSize : pageSize.Value;
    }

    #endregion Normalization
}
