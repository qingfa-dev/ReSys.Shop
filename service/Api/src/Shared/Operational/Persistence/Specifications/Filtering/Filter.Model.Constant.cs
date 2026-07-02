using Shared.Operational.Persistence.Specifications.Filtering.Extensions;
using Shared.Operational.Persistence.Specifications.Helpers;

namespace Shared.Operational.Persistence.Specifications.Filtering;

/// <summary>
/// Constants governing <see cref="FilterModel"/> parsing behaviour and structural limits.
/// </summary>
public static class FilterModelConstant
{
    /// <summary>
    /// Default values applied when optional parse parameters are omitted.
    /// </summary>
    public static class Defaults
    {
        /// <summary>
        /// The connective used for the root group and for any JSON group object that
        /// omits the <c>"logic"</c> property. Mirrors the DSL's comma-separated AND semantics.
        /// </summary>
        public const FilterLogic RootLogic = FilterLogic.And;

        /// <summary>
        /// The separator character used between triplet segments in query-string format
        /// (e.g. <c>Name:contains:John</c>).
        /// </summary>
        public const char QueryStringSeparator = ':';

        /// <summary>
        /// Maximum number of parts produced when splitting a query-string triplet.
        /// Capped at 3 so that values containing colons (e.g. ISO timestamps) are preserved.
        /// </summary>
        public const int QueryStringSplitCount = 3;
    }

    /// <summary>
    /// JSON property names expected in structured filter payloads.
    /// </summary>
    public static class JsonKeys
    {
        /// <summary>The property that identifies a group's logical connective: <c>"logic"</c>.</summary>
        public const string Logic = "logic";

        /// <summary>The property that holds a group's child conditions array: <c>"conditions"</c>.</summary>
        public const string Conditions = "conditions";

        /// <summary>The property that names the target field in a condition object: <c>"field"</c>.</summary>
        public const string Field = "field";

        /// <summary>The property that names the operator in a condition object: <c>"op"</c>.</summary>
        public const string Op = "op";

        /// <summary>The property that holds the comparison value in a condition object: <c>"value"</c>.</summary>
        public const string Value = "value";

        /// <summary>
        /// The JSON string that selects <see cref="FilterLogic.Or"/> when found
        /// under the <see cref="Logic"/> key (comparison is case-insensitive).
        /// </summary>
        public const string OrValue = "or";
    }

    /// <summary>
    /// Cache key constants used with <see cref="QueryHelper"/>.
    /// </summary>
    public static class Cache
    {
        /// <summary>
        /// The operation-type prefix used when caching compiled filter expressions
        /// via <c>QueryHelper.GetCachedExpression&lt;T&gt;</c> from the legacy DSL path
        /// (<see cref="FilterExtensions"/>).
        /// </summary>
        public const string Prefix = "Filter";

        /// <summary>
        /// The operation-type prefix used when caching expressions compiled from
        /// <see cref="FilterModel"/> trees (the model-tree path via
        /// <see cref="FilterModelEfCoreExtensions"/>).
        /// <para>Separate from <see cref="Prefix"/> to avoid cross-path cache collisions.</para>
        /// </summary>
        public const string ModelPrefix = "FilterModel";
    }

    /// <summary>
    /// Expression-tree construction defaults shared across all expression-building paths.
    /// </summary>
    public static class Expression
    {
        /// <summary>
        /// The canonical lambda parameter name used when building
        /// <c>x => ...</c> predicates for both DSL and model-tree paths.
        /// </summary>
        public const string ParameterName = "x";
    }

    /// <summary>
    /// DSL string serialization constants.
    /// </summary>
    public static class Dsl
    {
        /// <summary>
        /// The separator used between condition strings in <c>FilterModel.ToDslString()</c>
        /// output. Consistent with the DSL's comma-separated AND convention.
        /// </summary>
        public const string JoinSeparator = ", ";
    }
}