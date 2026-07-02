namespace Shared.Operational.Persistence.Specifications.Searching.Parsing;

// Boundary: Parses query-string search parameters into a validated SearchingModel.
internal static class SearchQueryStringParser
{
    // Contract: Returns Result<SearchingModel> - success with model or failure with error messages.
    public static Result<SearchModel> Parse(
        string? search,
        string? searchFields,
        string? searchingMode,
        string? caseSensitive)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return SearchModel.Empty;
            }

            List<string> fields = [];
            if (!string.IsNullOrWhiteSpace(searchFields))
            {
                fields.AddRange(
                    searchFields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            }

            SearchMode mode = SearchMode.Any;
            if (!string.IsNullOrWhiteSpace(searchingMode) &&
                searchingMode.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                mode = SearchMode.All;
            }

            bool caseSensitiveValue = false;
            if (!string.IsNullOrWhiteSpace(caseSensitive) &&
                bool.TryParse(caseSensitive, out bool parsed))
            {
                caseSensitiveValue = parsed;
            }

            SearchModel model = new SearchModel(
                new SearchTerm(search.Trim(), caseSensitiveValue),
                fields,
                mode,
                rawInput: search);

            return model;
        }
        catch (Exception ex)
        {
            return SearchingModelResult.Failure.InvalidQueryString(ex.Message);
        }
    }
}