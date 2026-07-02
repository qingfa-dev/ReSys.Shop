namespace Shared.Operational.Persistence.Specifications.Searching;

/// <summary>
/// Controls how a search term is applied across multiple target fields.
/// </summary>
/// <remarks>
/// <list type="table">
///   <listheader><term>Member</term><description>Behavior</description></listheader>
///   <item>
///     <term><see cref="Any"/></term>
///     <description>
///       A row matches if the term is found in <em>any</em> of the target fields (OR semantics).
///       This is the default and produces the broadest result set.
///     </description>
///   </item>
///   <item>
///     <term><see cref="All"/></term>
///     <description>
///       A row matches only if the term is found in <em>every</em> target field (AND semantics).
///       Useful for narrowing across composite identifiers.
///     </description>
///   </item>
/// </list>
/// JSON / query-string aliases: <c>"any"</c> (default) and <c>"all"</c>.
/// </remarks>
public enum SearchMode
{
    /// <summary>The term must match at least one target field.</summary>
    Any,

    /// <summary>The term must match all target fields.</summary>
    All
}