using System.Text.Json;

using Shared.Operational.Persistence.Specifications.Searching.Parsing;

namespace Shared.Operational.Persistence.Specifications.Searching;

/// <summary>
/// Public factory methods for creating <see cref="SearchModel"/> instances
/// from all three supported input surfaces: plain text, JSON, and query-string.
/// </summary>
public static class SearchModelExtensions
{
    /// <summary>
    /// Creates a <see cref="SearchModel"/> from a raw text string.
    /// Whitespace-only input produces <see cref="SearchModel.Empty"/>.
    /// </summary>
    public static SearchModel FromText(string? text)
        => SearchTextParser.Parse(text);

    /// <summary>
    /// Parses a JSON search request into a <see cref="SearchModel"/>.
    /// Expects an object with optional <c>term</c>, <c>fields</c>, <c>mode</c>, <c>caseSensitive</c>.
    /// </summary>
    public static Result<SearchModel> FromJson(
        string? json,
        IReadOnlySet<string>? allowedFields = null)
    {
        if (string.IsNullOrWhiteSpace(json))
            return SearchModel.Empty;

        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            Result<SearchModel> result = SearchJsonParser.Parse(doc.RootElement);

            if (result.IsFailure) return result.Errors;

            SearchModel model = result.Value;

            if (allowedFields is not null)
            {
                model = new SearchModel(
                    model.Term,
                    model.Fields,
                    model.Mode,
                    allowedFields,
                    rawInput: json);
            }

            return model;
        }
        catch (JsonException ex)
        {
            return SearchingModelResult.Failure.InvalidJson(ex.Message);
        }
    }

    /// <summary>
    /// Parses a collection of query-string search parameters into a <see cref="SearchModel"/>.
    /// </summary>
    public static Result<SearchModel> FromQueryString(
        string? search,
        string? searchFields = null,
        string? searchingMode = null,
        string? caseSensitive = null,
        IReadOnlySet<string>? allowedFields = null)
    {
        Result<SearchModel> result = SearchQueryStringParser.Parse(
            search, searchFields, searchingMode, caseSensitive);

        if (result.IsFailure) return result.Errors;

        if (allowedFields is not null && result.Value is { IsEmpty: false } model)
        {
            model = new SearchModel(
                model.Term,
                model.Fields,
                model.Mode,
                allowedFields,
                rawInput: search);
            return model;
        }

        return result;
    }
}
