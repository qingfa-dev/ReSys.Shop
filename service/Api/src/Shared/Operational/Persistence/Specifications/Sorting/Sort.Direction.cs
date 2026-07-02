namespace Shared.Operational.Persistence.Specifications.Sorting;

/// <summary>
/// Specifies the direction of a sort clause applied to a single field.
/// </summary>
/// <remarks>
/// Recognized string aliases (case-insensitive) across all input surfaces:
/// <list type="table">
///   <listheader><term>Member</term><description>DSL token / JSON value / query-string value</description></listheader>
///   <item>
///     <term><see cref="Ascending"/></term>
///     <description><c>asc</c>, <c>ascending</c>, <c>+</c> (DSL prefix)</description>
///   </item>
///   <item>
///     <term><see cref="Descending"/></term>
///     <description><c>desc</c>, <c>descending</c>, <c>-</c> (DSL prefix)</description>
///   </item>
/// </list>
/// When no direction is specified, <see cref="Ascending"/> is the default.
/// </remarks>
public enum SortDirection
{
    /// <summary>Sort from lowest to highest (A → Z, 0 → 9, oldest → newest).</summary>
    Ascending,

    /// <summary>Sort from highest to lowest (Z → A, 9 → 0, newest → oldest).</summary>
    Descending
}