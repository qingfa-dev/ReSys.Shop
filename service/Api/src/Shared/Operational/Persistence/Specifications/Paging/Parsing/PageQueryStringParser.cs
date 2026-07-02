using System.Globalization;

namespace Shared.Operational.Persistence.Specifications.Paging.Parsing;

/// <summary>
/// Parses raw query-string parameter strings into a validated <see cref="PageModel"/>.
/// </summary>
internal static class PageQueryStringParser
{
    /// <summary>
    /// Parses raw page and pageSize strings into a validated <see cref="PageModel"/>,
    /// applying <paramref name="bounds"/> normalization.
    /// </summary>
    /// <param name="pageStr">The raw <c>page</c> query-string value, or <see langword="null"/>.</param>
    /// <param name="pageSizeStr">The raw <c>pageSize</c> query-string value, or <see langword="null"/>.</param>
    /// <param name="bounds">The bounds to apply during normalization.</param>
    /// <returns>
    /// A <see cref="Result{PageModel}"/> — <see cref="PageModelResult.Failure.InvalidNumber"/> on non-integer input,
    /// otherwise success with normalized values. Returns <see cref="PageModel.Empty"/> when both parameters are missing.
    /// </returns>
    public static Result<PageModel> Parse(string? pageStr, string? pageSizeStr, PageBounds bounds)
    {
        int? page = null;
        int? pageSize = null;
        List<Error> errors = [];

        if (!string.IsNullOrEmpty(pageStr))
        {
            if (int.TryParse(pageStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedPage))
            {
                page = parsedPage;
            }
            else
            {
                errors.Add(PageModelResult.Failure.InvalidNumber("page", pageStr));
            }
        }

        if (!string.IsNullOrEmpty(pageSizeStr))
        {
            if (int.TryParse(pageSizeStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedSize))
            {
                pageSize = parsedSize;
            }
            else
            {
                errors.Add(PageModelResult.Failure.InvalidNumber("pageSize", pageSizeStr));
            }
        }

        if (errors is { Count: > 0 })
            return errors;

        PageModel model = new(
            page:     bounds.NormalizePage(page),
            pageSize: bounds.NormalizePageSize(pageSize),
            bounds:   bounds,
            isEmpty:  page is null && pageSize is null);

        return Result<PageModel>.Ok(model);
    }
}
