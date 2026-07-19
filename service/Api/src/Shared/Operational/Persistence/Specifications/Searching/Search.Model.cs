using System.Collections.ObjectModel;

using Shared.Operational.Persistence.Specifications.Filtering;

namespace Shared.Operational.Persistence.Specifications.Searching;

/// <summary>
/// The unified, parsed representation of a search expression.
/// </summary>
/// <remarks>
/// <c>SearchingModel</c> is the root output of all three input surfaces — plain string,
/// JSON, and query-string — via the factory methods in <see cref="SearchModelExtensions"/>.
/// <para>
/// A search always consists of a single <see cref="Term"/> applied as a substring
/// (<em>contains</em>) match across one or more <see cref="Fields"/>. The <see cref="Mode"/>
/// property controls whether the term must appear in <em>any</em> field (OR) or
/// <em>all</em> fields (AND).
/// </para>
/// <para>
/// For operator-specific matching (e.g. starts-with, equality, range), use the filter
/// layer (<see cref="FilterModel"/>) instead.
/// </para>
/// <para>
/// Construction enforces an optional <see cref="AllowedFields"/> whitelist. When supplied,
/// any target field not in the whitelist causes <see cref="IsValid"/> to be
/// <see langword="false"/> and populates <see cref="Violations"/>.
/// </para>
/// </remarks>
public sealed partial class SearchModel
{
    #region Properties

    /// <summary>
    /// Gets the normalized search term to match against target fields.
    /// </summary>
    public SearchTerm Term { get; }

    /// <summary>
    /// Gets the ordered list of property names to search across.
    /// May be empty when the caller delegates field selection entirely to the expression builder
    /// (which then falls back to a per-entity <c>SearchableFields</c> constant).
    /// </summary>
    public IReadOnlyList<string> Fields { get; }

    /// <summary>
    /// Gets the match mode controlling how the term is combined across multiple fields.
    /// Defaults to <see cref="SearchMode.Any"/> (OR semantics).
    /// </summary>
    public SearchMode Mode { get; }

    /// <summary>
    /// Gets the set of field names that are allowed as search targets.
    /// <see langword="null"/> means no whitelist is enforced.
    /// </summary>
    public IReadOnlySet<string>? AllowedFields { get; }

    /// <summary>
    /// Gets the original raw input string that produced this model, if available.
    /// Used for diagnostics and cache key correlation.
    /// </summary>
    public string? RawInput { get; }

    /// <summary>
    /// Gets a value indicating whether the model passed allowed-field validation.
    /// Always <see langword="true"/> when no <see cref="AllowedFields"/> whitelist was supplied.
    /// </summary>
    public bool IsValid => Violations.Count == 0;

    /// <summary>
    /// Gets the field names that were specified as targets but are not present in
    /// <see cref="AllowedFields"/>. Empty when <see cref="IsValid"/> is <see langword="true"/>.
    /// </summary>
    public IReadOnlyList<string> Violations { get; }

    /// <summary>
    /// Gets a value indicating whether the model carries no actionable search term.
    /// The expression builder treats an empty model as a no-op and returns the query unchanged.
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(Term.Value);

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new <see cref="SearchModel"/>.
    /// </summary>
    /// <param name="term">The normalized search term.</param>
    /// <param name="fields">The ordered list of target field names. Pass an empty list to use entity defaults.</param>
    /// <param name="mode">The match mode. Defaults to <see cref="SearchMode.Any"/>.</param>
    /// <param name="allowedFields">Optional whitelist enforced at construction time.</param>
    /// <param name="rawInput">The original input string, for diagnostics.</param>
    internal SearchModel(
        SearchTerm term,
        IReadOnlyList<string> fields,
        SearchMode mode = SearchMode.Any,
        IReadOnlySet<string>? allowedFields = null,
        string? rawInput = null)
    {
        Term = term;
        Fields = fields;
        Mode = mode;
        AllowedFields = allowedFields;
        RawInput = rawInput;

        // Validate: Check every specified target field against the whitelist.
        Violations = ComputeViolations(allowedFields, fields);
    }

    #endregion Constructor

    #region Derived Views

    /// <summary>
    /// Returns <see langword="true"/> if the given field name is included in <see cref="Fields"/>.
    /// Comparison is case-insensitive.
    /// </summary>
    public bool HasField(string field)
        => Fields.Any(f => f.Equals(field, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Returns the effective fields to search: <see cref="Fields"/> when non-empty,
    /// otherwise falls back to the supplied <paramref name="defaultFields"/>.
    /// </summary>
    /// <param name="defaultFields">
    /// The entity-level default searchable fields, typically from
    /// <c>[Entity]Constant.Query.SearchableFields</c>.
    /// </param>
    public IReadOnlyList<string> ResolveFields(IReadOnlyList<string> defaultFields)
        => Fields.Count > 0 ? Fields : defaultFields;

    #endregion Derived Views

    #region Static Factories

    /// <summary>
    /// Returns an empty <see cref="SearchModel"/> with no term, no fields, and no violations.
    /// </summary>
    public static SearchModel Empty { get; } =
        new(new SearchTerm { Value = string.Empty }, [], SearchMode.Any);

    #endregion Static Factories

    #region Validation

    // Validate: Check field names case-insensitively against the allowed-fields whitelist.
    private static ReadOnlyCollection<string> ComputeViolations(
        IReadOnlySet<string>? allowedFields,
        IReadOnlyList<string> fields)
    {
        if (allowedFields is null || fields.Count == 0)
        {
            return [];
        }

        return fields
            .Where(f => !allowedFields.Contains(f, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();
    }

    #endregion Validation

    #region Defaults

    public static class Default
    {
        public static readonly IReadOnlyList<string> Fields = [];
        public const SearchMode Mode = SearchMode.Any;
    }

    #endregion Defaults
}
