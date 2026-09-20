namespace Shared.Operational.Persistence.Specifications.Sorting;

/// <summary>
/// The unified, parsed representation of a sort expression.
/// </summary>
/// <remarks>
/// <c>SortModel</c> is the root output of all three input surfaces — DSL string,
/// JSON, and query-string — via the factory methods in <see cref="SortModelExtensions"/>.
/// <para>
/// A sort is an ordered list of <see cref="SortClause"/> entries. The order of clauses in
/// <see cref="Clauses"/> is significant: index 0 is the primary sort, index 1 is the
/// first tie-breaker, and so on. The expression builder applies them in this order using
/// <c>OrderBy</c> / <c>ThenBy</c> chaining.
/// </para>
/// <para>
/// Construction enforces an optional <see cref="AllowedFields"/> whitelist. When supplied,
/// any clause referencing a field not in the whitelist causes <see cref="IsValid"/> to be
/// <see langword="false"/> and populates <see cref="Violations"/>.
/// </para>
/// </remarks>
public sealed partial class SortModel
{
    #region Properties

    /// <summary>
    /// Gets the ordered list of sort clauses. Index 0 is the primary sort key.
    /// </summary>
    public IReadOnlyList<SortClause> Clauses { get; }

    /// <summary>
    /// Gets the set of field names that clauses are allowed to reference.
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
    /// Gets the field names that were referenced in clauses but are not present in
    /// <see cref="AllowedFields"/>. Empty when <see cref="IsValid"/> is <see langword="true"/>.
    /// </summary>
    public IReadOnlyList<string> Violations { get; }

    /// <summary>
    /// Gets a value indicating whether this model contains no sort clauses.
    /// The expression builder treats an empty model as a no-op and returns the query unchanged.
    /// </summary>
    public bool IsEmpty => Clauses.Count == 0;

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new <see cref="SortModel"/> from an ordered list of clauses.
    /// </summary>
    /// <param name="clauses">The ordered sort clauses. Index 0 is the primary sort key.</param>
    /// <param name="allowedFields">
    /// Optional whitelist of field names. When supplied, clauses referencing unlisted
    /// fields are recorded as <see cref="Violations"/> and <see cref="IsValid"/> returns
    /// <see langword="false"/>.
    /// </param>
    /// <param name="rawInput">The original input string, for diagnostics.</param>
    internal SortModel(
        IReadOnlyList<SortClause> clauses,
        IReadOnlySet<string>? allowedFields = null,
        string? rawInput = null)
    {
        Clauses = clauses;
        AllowedFields = allowedFields;
        RawInput = rawInput;

        // Validate: Check every referenced field against the whitelist.
        Violations = ComputeViolations(allowedFields, clauses);
    }

    #endregion Constructor

    #region Derived Views

    /// <summary>
    /// Gets the primary sort clause (index 0), or <see langword="null"/> when the model is empty.
    /// </summary>
    public SortClause? Primary => Clauses.Count > 0 ? Clauses[0] : null;

    #endregion Derived Views

    #region Static Sentinels

    /// <summary>
    /// Default values for <see cref="SortModel"/> construction parameters.
    /// </summary>
    public static class Default
    {
        public static readonly IReadOnlyList<SortClause> Clauses = [];
    }

    /// <summary>
    /// Returns an empty <see cref="SortModel"/> with no clauses and no violations.
    /// </summary>
    public static SortModel Empty { get; } = new([]);

    #endregion Static Sentinels
}