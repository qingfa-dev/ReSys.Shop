namespace Shared.Operational.Persistence.Specifications.Filtering;

/// <summary>
/// The unified, parsed representation of a filter expression produced by any of the three
/// input surfaces: DSL string, JSON, or query-string triplets.
/// </summary>
/// <remarks>
/// Exposes both a structured <see cref="Root"/> tree for compound expressions and a flat
/// <see cref="Conditions"/> list for simple single-level use cases. When an
/// <see cref="AllowedFields"/> whitelist is supplied, any condition referencing an unlisted
/// field causes <see cref="IsValid"/> to return <see langword="false"/> and populates
/// <see cref="Violations"/>. Parsing never throws — invalid input surfaces as failures in
/// the <c>Result&lt;FilterModel&gt;</c> returned by the factory methods.
/// </remarks>
public sealed partial class FilterModel
{
    #region Properties

    /// <summary>
    /// Gets the root group of the parsed filter tree.
    /// The top-level connective is always <see cref="FilterLogic.And"/>.
    /// </summary>
    public FilterGroup Root { get; }

    /// <summary>
    /// Gets a flat, ordered list of every leaf <see cref="FilterCondition"/> reachable from
    /// <see cref="Root"/>, regardless of nesting depth.
    /// </summary>
    public IReadOnlyList<FilterCondition> Conditions { get; }

    /// <summary>
    /// Gets the set of field names that filter conditions are permitted to reference.
    /// <see langword="null"/> means no whitelist is enforced.
    /// </summary>
    public IReadOnlySet<string>? AllowedFields { get; }

    /// <summary>
    /// Gets the original raw input string that produced this model, for diagnostics and
    /// cache-key correlation. <see langword="null"/> when the model was built from structured input.
    /// </summary>
    public string? RawInput { get; }

    /// <summary>
    /// Gets a value indicating whether all referenced fields pass the
    /// <see cref="AllowedFields"/> whitelist check.
    /// Always <see langword="true"/> when no whitelist was supplied.
    /// </summary>
    public bool IsValid => Violations.Count == 0;

    /// <summary>
    /// Gets the field names referenced in conditions but absent from <see cref="AllowedFields"/>.
    /// Empty when <see cref="IsValid"/> is <see langword="true"/>.
    /// </summary>
    public IReadOnlyList<string> Violations { get; }

    /// <summary>
    /// Gets a value indicating whether the model contains no conditions at any nesting level.
    /// </summary>
    public bool IsEmpty => Root.IsEmpty;

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a <see cref="FilterModel"/> from a parsed root group.
    /// </summary>
    /// <param name="root">The root <see cref="FilterGroup"/> produced by a parser.</param>
    /// <param name="allowedFields">
    /// Optional whitelist. Conditions referencing unlisted fields populate
    /// <see cref="Violations"/> and cause <see cref="IsValid"/> to return
    /// <see langword="false"/>.
    /// </param>
    /// <param name="rawInput">The original input string; stored for diagnostics only.</param>
    internal FilterModel(
        FilterGroup root,
        IReadOnlySet<string>? allowedFields = null,
        string? rawInput = null)
    {
        Root          = root;
        AllowedFields = allowedFields;
        RawInput      = rawInput;

        // Flatten: Pre-compute the full condition list for O(1) flat access.
        Conditions = root.FlattenConditions().ToList().AsReadOnly();

        // Validate: Record every field that is not in the whitelist.
        Violations = allowedFields is null
            ? []
            : Conditions
                .Select(c => c.Field)
                .Where(f => !allowedFields.Contains(f, StringComparer.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                .AsReadOnly();
    }

    #endregion Constructor
}