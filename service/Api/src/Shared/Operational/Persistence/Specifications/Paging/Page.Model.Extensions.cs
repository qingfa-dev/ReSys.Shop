using Shared.Operational.Persistence.Specifications.Paging.Parsing;

namespace Shared.Operational.Persistence.Specifications.Paging;

/// <summary>
/// Factory methods for constructing <see cref="PageModel"/> instances from various input surfaces.
/// </summary>
/// <remarks>
/// All factory methods normalize raw values through <see cref="PageBounds"/> (clamping
/// out-of-range values silently) and return <see cref="Result{PageModel}"/>.
/// Error results are produced only for structurally malformed input — never for
/// out-of-range values.
/// </remarks>
public static class PageModelExtensions
{
    /// <summary>
    /// Constructs a <see cref="PageModel"/> from already-typed integer values.
    /// Always succeeds — normalization is applied via <see cref="PageBounds"/>.
    /// </summary>
    /// <param name="page">The raw page number as an integer, or <see langword="null"/>.</param>
    /// <param name="pageSize">The raw page size as an integer, or <see langword="null"/>.</param>
    /// <param name="bounds">The bounds to apply during normalization. Uses <see cref="PageBounds.Default"/> when <see langword="null"/>.</param>
    /// <returns>A <see cref="Result{PageModel}"/> — always <see cref="Result{PageModel}.IsSuccess"/>.</returns>
    public static Result<PageModel> FromValues(
        int? page,
        int? pageSize,
        PageBounds? bounds = null)
    {
        PageBounds effectiveBounds = bounds ?? PageBounds.Default;

        PageModel model = new(
            page:     effectiveBounds.NormalizePage(page),
            pageSize: effectiveBounds.NormalizePageSize(pageSize),
            bounds:   effectiveBounds,
            isEmpty:  page is null && pageSize is null);

        return Result<PageModel>.Ok(model);
    }

    /// <summary>
    /// Constructs a <see cref="PageModel"/> from raw query-string parameter strings.
    /// </summary>
    /// <param name="pageStr">The raw page query-string value, or <see langword="null"/>.</param>
    /// <param name="pageSizeStr">The raw page-size query-string value, or <see langword="null"/>.</param>
    /// <param name="bounds">The bounds to apply during normalization. Uses <see cref="PageBounds.Default"/> when <see langword="null"/>.</param>
    /// <returns>
    /// A <see cref="Result{PageModel}"/> — <see cref="PageModelResult.Failure.InvalidNumber"/> on non-integer input,
    /// otherwise success with normalized values.
    /// </returns>
    public static Result<PageModel> FromQueryString(
        string? pageStr,
        string? pageSizeStr,
        PageBounds? bounds = null)
    {
        PageBounds effectiveBounds = bounds ?? PageBounds.Default;

        return PageQueryStringParser.Parse(pageStr, pageSizeStr, effectiveBounds);
    }

    /// <summary>
    /// Constructs a <see cref="PageModel"/> from a raw JSON string.
    /// </summary>
    /// <param name="json">A JSON object with optional <c>page</c> and <c>pageSize</c> properties, or <see langword="null"/>.</param>
    /// <param name="bounds">The bounds to apply during normalization. Uses <see cref="PageBounds.Default"/> when <see langword="null"/>.</param>
    /// <returns>
    /// A <see cref="Result{PageModel}"/> — <see cref="PageModelResult.Failure.InvalidJson"/> on malformed JSON,
    /// <see cref="PageModelResult.Failure.InvalidNumber"/> on non-integer values,
    /// otherwise success with normalized values.
    /// </returns>
    public static Result<PageModel> FromJson(
        string? json,
        PageBounds? bounds = null)
    {
        PageBounds effectiveBounds = bounds ?? PageBounds.Default;

        return PageJsonParser.Parse(json, effectiveBounds);
    }
}
