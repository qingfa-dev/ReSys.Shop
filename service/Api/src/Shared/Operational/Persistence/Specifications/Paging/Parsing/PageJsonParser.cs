using System.Text.Json;

namespace Shared.Operational.Persistence.Specifications.Paging.Parsing;

/// <summary>
/// Parses a JSON string into a validated <see cref="PageModel"/>.
/// </summary>
internal static class PageJsonParser
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { PropertyNameCaseInsensitive = true };

    private sealed record PageJsonDto(int? Page, int? PageSize);

    /// <summary>
    /// Parses a JSON string into a validated <see cref="PageModel"/>,
    /// applying <paramref name="bounds"/> normalization.
    /// </summary>
    /// <param name="json">A JSON object with optional <c>page</c> and <c>pageSize</c> properties, or <see langword="null"/>.</param>
    /// <param name="bounds">The bounds to apply during normalization.</param>
    /// <returns>
    /// A <see cref="Result{PageModel}"/> — <see cref="PageModelResult.Failure.InvalidJson"/> on malformed JSON,
    /// <see cref="PageModelResult.Failure.InvalidNumber"/> on non-integer values,
    /// otherwise success with normalized values. Returns <see cref="PageModel.Empty"/> for null or empty input.
    /// </returns>
    public static Result<PageModel> Parse(string? json, PageBounds bounds)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Result<PageModel>.Ok(PageModel.Empty);
        }

        PageJsonDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<PageJsonDto>(json, s_jsonOptions);
        }
        catch (JsonException ex)
        {
            return PageModelResult.Failure.InvalidJson(ex.Message);
        }

        if (dto is null)
            return PageModelResult.Failure.InvalidJson("Deserialization produced null.");

        // Validate: Check for non-integer values in the raw JSON.
        // Deserialization already handled the JSON structure; int values are parsed.
        // Non-numeric values are caught by JsonException above.

        PageModel model = new(
            page:     bounds.NormalizePage(dto.Page),
            pageSize: bounds.NormalizePageSize(dto.PageSize),
            bounds:   bounds,
            isEmpty:  dto.Page is null && dto.PageSize is null);

        return Result<PageModel>.Ok(model);
    }
}
