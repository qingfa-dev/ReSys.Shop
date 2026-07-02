using System.Text.Json;

namespace Shared.Operational.Persistence.Specifications.Filtering.Parsing;

/// <summary>
/// Parses a JSON array of condition / group objects into a <see cref="FilterGroup"/> tree.
/// </summary>
internal static class FilterJsonParser
{
    /// <summary>
    /// Entry point. Parses <paramref name="json"/> and returns a root
    /// <see cref="FilterGroup"/> or a typed failure.
    /// </summary>
    internal static Result<FilterGroup> Parse(string json)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            // Guard: Top-level element must be a JSON array.
            if (root.ValueKind != JsonValueKind.Array)
                return FilterModelResult.Failure.InvalidJson("Root element must be a JSON array.");

            return ParseArray(root);
        }
        catch (JsonException ex)
        {
            return FilterModelResult.Failure.InvalidJson(ex.Message);
        }
        catch
        {
            return FilterModelResult.Failure.InvalidJson("An unexpected error occurred while parsing the filter JSON.");
        }
    }

    // ── Array / Group traversal ───────────────────────────────────────────────

    private static Result<FilterGroup> ParseArray(JsonElement array)
    {
        var conditions = new List<FilterCondition>();
        var subGroups = new List<FilterGroup>();

        foreach (JsonElement element in array.EnumerateArray())
        {
            // Determine: Group object (has "logic" or "conditions") vs. leaf condition.
            if (element.TryGetProperty(FilterModelConstant.JsonKeys.Logic, out _) ||
                element.TryGetProperty(FilterModelConstant.JsonKeys.Conditions, out _))
            {
                Result<FilterGroup> nested = ParseGroup(element);
                if (nested.IsFailure) return nested.Errors;
                subGroups.Add(nested.Value);
            }
            else
            {
                Result<FilterCondition> condition = ParseCondition(element);
                if (condition.IsFailure) return condition.Errors;
                conditions.Add(condition.Value);
            }
        }

        return new FilterGroup(
            FilterModelConstant.Defaults.RootLogic,
            conditions.AsReadOnly(),
            subGroups.AsReadOnly());
    }

    private static Result<FilterGroup> ParseGroup(JsonElement element)
    {
        // Resolve: Logic connective — default to AND when the property is absent.
        FilterLogic logic = FilterLogic.And;
        if (element.TryGetProperty(FilterModelConstant.JsonKeys.Logic, out JsonElement logicEl))
        {
            logic = string.Equals(
                logicEl.GetString(),
                FilterModelConstant.JsonKeys.OrValue,
                StringComparison.OrdinalIgnoreCase)
                    ? FilterLogic.Or
                    : FilterLogic.And;
        }

        // Guard: "conditions" array is required on group objects.
        if (!element.TryGetProperty(FilterModelConstant.JsonKeys.Conditions, out JsonElement condEl) ||
            condEl.ValueKind != JsonValueKind.Array)
            return FilterModelResult.Failure.InvalidJson(
                $"A group object must include a '{FilterModelConstant.JsonKeys.Conditions}' array.");

        var conditions = new List<FilterCondition>();
        var subGroups = new List<FilterGroup>();

        foreach (JsonElement child in condEl.EnumerateArray())
        {
            if (child.TryGetProperty(FilterModelConstant.JsonKeys.Logic, out _) ||
                child.TryGetProperty(FilterModelConstant.JsonKeys.Conditions, out _))
            {
                Result<FilterGroup> nested = ParseGroup(child);
                if (nested.IsFailure) return nested.Errors;
                subGroups.Add(nested.Value);
            }
            else
            {
                Result<FilterCondition> condition = ParseCondition(child);
                if (condition.IsFailure) return condition.Errors;
                conditions.Add(condition.Value);
            }
        }

        return new FilterGroup(logic, conditions.AsReadOnly(), subGroups.AsReadOnly());
    }

    // ── Leaf condition ────────────────────────────────────────────────────────

    private static Result<FilterCondition> ParseCondition(JsonElement element)
    {
        // Guard: "field" is required and must be non-empty.
        if (!element.TryGetProperty(FilterModelConstant.JsonKeys.Field, out JsonElement fieldEl))
            return FilterModelResult.Failure.MissingField;

        string? field = fieldEl.GetString();
        if (string.IsNullOrWhiteSpace(field))
            return FilterModelResult.Failure.MissingField;

        // Guard: "op" is required and must map to a known operator.
        if (!element.TryGetProperty(FilterModelConstant.JsonKeys.Op, out JsonElement opEl))
            return FilterModelResult.Failure.MissingOperator;

        string? opToken = opEl.GetString();
        if (!FilterOperatorMap.TryParse(opToken, out FilterOperator op))
            return FilterModelResult.Failure.UnknownOperator(opToken ?? "(null)");

        // Parse: "value" defaults to empty string when absent.
        string value = element.TryGetProperty(FilterModelConstant.JsonKeys.Value, out JsonElement valueEl)
            ? (valueEl.GetString() ?? string.Empty)
            : string.Empty;

        return new FilterCondition(field, op, value);
    }
}
