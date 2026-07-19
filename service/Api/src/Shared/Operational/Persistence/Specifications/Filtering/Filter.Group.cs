namespace Shared.Operational.Persistence.Specifications.Filtering;

/// <summary>
/// Represents a logical grouping of filter conditions and nested sub-groups.
/// </summary>
/// <remarks>
/// A group with no conditions and no sub-groups is empty and contributes nothing to the
/// resulting predicate. The root group produced by <see cref="FilterModel"/> always uses
/// <see cref="FilterLogic.And"/>, reflecting the DSL's default comma-separated AND semantics.
/// Parenthesized OR sub-expressions become nested groups with <see cref="FilterLogic.Or"/>.
/// </remarks>
public sealed partial record FilterGroup
{
    /// <summary>
    /// The logical connective applied between all members of <see cref="Conditions"/>
    /// and <see cref="Groups"/>.
    /// </summary>
    public FilterLogic Logic { get; init; }

    /// <summary>
    /// The leaf-level <see cref="FilterCondition"/> entries belonging directly to this group.
    /// </summary>
    public IReadOnlyList<FilterCondition> Conditions { get; init; } = default!;

    /// <summary>
    /// Nested <see cref="FilterGroup"/> sub-expressions, each evaluated as a unit before
    /// the outer <see cref="Logic"/> combines it with its siblings.
    /// </summary>
    public IReadOnlyList<FilterGroup> Groups { get; init; } = default!;
}