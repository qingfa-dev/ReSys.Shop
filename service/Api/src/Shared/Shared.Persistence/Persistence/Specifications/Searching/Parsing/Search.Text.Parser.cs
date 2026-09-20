namespace Shared.Operational.Persistence.Specifications.Searching.Parsing;

// Boundary: Parses plain-text search input into a SearchingModel.
internal static class SearchTextParser
{
    // Contract: Returns SearchingModel.Empty for null/whitespace input.
    public static SearchModel Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return SearchModel.Empty;
        }

        return new SearchModel(
            new SearchTerm { Value = input.Trim() },
            [],
            SearchMode.Any,
            rawInput: input);
    }
}