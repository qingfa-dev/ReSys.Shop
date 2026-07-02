using System.Text;

namespace Shared.Operational.Persistence.Specifications.Filtering;

public sealed partial record FilterGroup
{
    #region Computed Properties

    /// <summary>
    /// Gets a value indicating whether this group contains no conditions and no sub-groups.
    /// </summary>
    public bool IsEmpty => Conditions.Count == 0 && Groups.Count == 0;

    /// <summary>
    /// Gets the total number of leaf <see cref="FilterCondition"/> entries reachable from
    /// this group, counting recursively through all sub-groups.
    /// </summary>
    public int TotalConditionCount =>
        Conditions.Count + Groups.Sum(g => g.TotalConditionCount);

    #endregion Computed Properties

    #region Traversal

    /// <summary>
    /// Enumerates all leaf <see cref="FilterCondition"/> entries reachable from this group,
    /// depth-first, regardless of nesting level.
    /// </summary>
    /// <returns>A flat sequence of every condition in the tree.</returns>
    public IEnumerable<FilterCondition> FlattenConditions()
    {
        foreach (FilterCondition condition in Conditions)
            yield return condition;

        foreach (FilterCondition nested in Groups.SelectMany(g => g.FlattenConditions()))
            yield return nested;
    }

    #endregion Traversal

    #region Static Factories — with Result

    // No Result<T>-returning factories exist at the group level; groups are
    // produced exclusively by parsers and never constructed by callers directly.

    #endregion Static Factories — with Result

    #region Static Factories — without Result

    /// <summary>
    /// Creates an AND group from a flat list of conditions with no sub-groups.
    /// Suitable for single-level filters where no compound OR branching is needed.
    /// </summary>
    /// <param name="conditions">The conditions to include.</param>
    public static FilterGroup FlatAnd(IReadOnlyList<FilterCondition> conditions)
        => new(FilterLogic.And, conditions, []);

    /// <summary>
    /// Creates an OR group from a flat list of conditions with no sub-groups.
    /// </summary>
    /// <param name="conditions">The conditions to include.</param>
    public static FilterGroup FlatOr(IReadOnlyList<FilterCondition> conditions)
        => new(FilterLogic.Or, conditions, []);

    /// <summary>
    /// Returns a canonical empty AND group with no conditions and no sub-groups.
    /// </summary>
    public static FilterGroup Empty { get; } = new(FilterLogic.And, [], []);

    #endregion Static Factories — without Result

    #region Display

    /// <summary>
    /// Builds a deterministic cache key that captures the full tree structure.
    /// Two <see cref="FilterGroup"/> trees with different nesting produce different keys,
    /// even when the flat condition lists are identical.
    /// </summary>
    /// <remarks>
    /// Format: <c>{LogicInt}|{conditionCount}|{groupCount}|{C1}|...|{G1}|...</c> where
    /// each condition is <c>{Field}\x00{OpInt}\x00{Value}</c> and each group is recursive.
    /// The null character (<c>\0</c>) is safe because filter values are user-supplied strings
    /// that never contain embedded nulls.
    /// </remarks>
    public string ToStructuralKey()
    {
        StringBuilder sb = new();
        AppendStructuralKey(sb);
        return sb.ToString();
    }

    private void AppendStructuralKey(StringBuilder sb)
    {
        sb.Append((int)Logic);
        sb.Append('|');
        sb.Append(Conditions.Count);
        sb.Append('|');
        sb.Append(Groups.Count);
        sb.Append('|');

        foreach (FilterCondition c in Conditions)
        {
            sb.Append(c.Field);
            sb.Append('\0');
            sb.Append((int)c.Operator);
            sb.Append('\0');
            sb.Append(c.Value);
            sb.Append('|');
        }

        foreach (FilterGroup g in Groups)
        {
            AppendStructuralKey(sb);
        }
    }

    /// <summary>
    /// Returns a compact diagnostic string showing logic, condition count, and sub-group count.
    /// </summary>
    public override string ToString() =>
        $"{Logic}[conditions={Conditions.Count}, groups={Groups.Count}]";

    #endregion Display
}