using Shared.Operational.Persistence.Specifications.Filtering.Extensions;

namespace Shared.Operational.Persistence.Specifications.Filtering;

public sealed partial class FilterModel
{
    #region Derived Views

    /// <summary>
    /// Returns all conditions in the flat list that target <paramref name="field"/>.
    /// Comparison is case-insensitive.
    /// </summary>
    /// <param name="field">The property name to look up.</param>
    public IEnumerable<FilterCondition> ConditionsFor(string field)
        => Conditions.Where(c => c.Field.Equals(field, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Returns <see langword="true"/> when at least one condition targets <paramref name="field"/>.
    /// Comparison is case-insensitive.
    /// </summary>
    /// <param name="field">The property name to check.</param>
    public bool HasField(string field)
        => Conditions.Any(c => c.Field.Equals(field, StringComparison.OrdinalIgnoreCase));

    #endregion Derived Views

    #region Serialization

    /// <summary>
    /// Reconstructs a canonical DSL string from the flat <see cref="Conditions"/> list.
    /// The output is always valid input for <see cref="FilterModelExtensions.FromString"/>
    /// apply-filter extension methods such as
    /// <see cref="FilterModelEfCoreExtensions.ApplyFilter{T}(System.Linq.IQueryable{T}, FilterModel?)"/>.
    /// </summary>
    /// <remarks>
    /// This is a flat reconstruction — nested OR groups from the original parse are not
    /// preserved in the output. Used internally by EF Core bridge extensions to feed the
    /// expression-cache key without rebuilding Expression trees from the model.
    /// </remarks>
    public string ToDslString()
        => string.Join(FilterModelConstant.Dsl.JoinSeparator, Conditions.Select(c => c.ToString()));

    #endregion Serialization

    #region Static Sentinels

    /// <summary>
    /// A pre-built empty <see cref="FilterModel"/> with no conditions, no violations,
    /// and no allowed-fields whitelist. Used as the zero-allocation result for empty input.
    /// </summary>
    public static FilterModel Empty { get; } = new(FilterGroup.Empty);

    #endregion Static Sentinels
}