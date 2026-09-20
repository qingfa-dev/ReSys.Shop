using System.Text.Json;

namespace Shared.Operational.Persistence.Specifications.Searching.Parsing;

// Boundary: Parses JSON search requests into a validated SearchingModel.
internal static class SearchJsonParser
{
    // Contract: Returns Result<SearchingModel> - success with model or failure with error messages.
    public static Result<SearchModel> Parse(JsonElement element)
    {
        try
        {
            string? term = null;
            List<string> fields = [];
            SearchMode mode = SearchMode.Any;
            bool caseSensitive = false;

            if (element.TryGetProperty("term", out JsonElement termElement) && termElement.ValueKind == JsonValueKind.String)
            {
                term = termElement.GetString();
            }

            if (string.IsNullOrWhiteSpace(term))
            {
                return SearchingModelResult.Failure.TermRequired;
            }

            if (element.TryGetProperty("fields", out JsonElement fieldsElement) && fieldsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement fieldElement in fieldsElement.EnumerateArray())
                {
                    if (fieldElement.ValueKind == JsonValueKind.String)
                    {
                        string? field = fieldElement.GetString();
                        if (!string.IsNullOrWhiteSpace(field))
                        {
                            fields.Add(field!);
                        }
                    }
                }
            }

            if (element.TryGetProperty("mode", out JsonElement modeElement) && modeElement.ValueKind == JsonValueKind.String)
            {
                string? modeStr = modeElement.GetString();
                if (modeStr?.Equals("all", StringComparison.OrdinalIgnoreCase) == true)
                {
                    mode = SearchMode.All;
                }
            }

            if (element.TryGetProperty("caseSensitive", out JsonElement caseElement))
            {
                caseSensitive = caseElement.ValueKind switch
                {
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => false
                };
            }

            SearchModel model = new(
                new SearchTerm { Value = term!, CaseSensitive = caseSensitive },
                fields,
                mode,
                rawInput: element.GetRawText());

            return model;
        }
        catch (Exception ex)
        {
            return Result<SearchModel>.Unexpected(
                exception: ex,
                errors: [SearchingModelResult.Failure.InvalidJson(ex.Message)]);
        }
    }
}