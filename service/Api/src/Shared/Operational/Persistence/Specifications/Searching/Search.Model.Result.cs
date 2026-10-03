namespace Shared.Operational.Persistence.Specifications.Searching;

// Contract: Immutable validation result for search model field whitelist checks.
public sealed record SearchValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Violations { get; init; } = default!;
    public IReadOnlyList<string>? AllowedFields { get; init; }
}

public partial class SearchModel
{
    // AgentHint: Produces a structured validation result from the model's current state.
    public SearchValidationResult ToValidationResult() =>
        new() { IsValid = IsValid, Violations = Violations, AllowedFields = AllowedFields?.ToList() };
}

// Boundary: Typed result messages and error definitions for SearchingModel operations.
public static class SearchingModelResult
{
    #region Success

    public static class SuccessMsg
    {
        public static string Parsed => "Search parsed successfully.";
        public static string Empty => "No search input provided; empty model returned.";
    }

    #endregion Success

    #region Error

    public static class Failure
    {
        public static Error TermRequired =>
            Error.Validation(
                "Search.Parsing.TermRequired",
                "Search term is required and must not be empty.");

        public static Error InvalidJson(string detail) =>
            Error.Validation(
                "Search.Parsing.InvalidJson",
                $"Failed to parse search JSON: {detail}");

        public static Error InvalidQueryString(string detail) =>
            Error.Validation(
                "Search.Parsing.InvalidQueryString",
                $"Failed to parse search query: {detail}");
    }

    #endregion Error
}