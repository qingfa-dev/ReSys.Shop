namespace Shared.Operational.Persistence.Specifications.Filtering.Expression;

/// <summary>
/// Constants used by <see cref="FilterExpressionBuilder"/> during value parsing
/// and expression construction.
/// </summary>
internal static class FilterExpressionBuilderConstant
{
    /// <summary>
    /// The string literal that represents a typed null in filter input.
    /// Compared case-insensitively at parse time.
    /// </summary>
    public static class NullSentinel
    {
        /// <summary>Case-insensitive string that resolves to a typed null constant.</summary>
        public const string Value = "null";
    }

    /// <summary>
    /// String aliases that resolve to boolean <see langword="true"/> or
    /// <see langword="false"/> during value parsing. Defined as individual
    /// <see langword="const"/> fields so they can be used in <c>switch</c>
    /// pattern matching.
    /// </summary>
    public static class BooleanAliases
    {
        public const string True1 = "1";
        public const string TrueYes = "yes";
        public const string TrueY = "y";

        public const string False0 = "0";
        public const string FalseNo = "no";
        public const string FalseN = "n";
    }

    /// <summary>
    /// Syntax separators used when constructing property-path expressions.
    /// </summary>
    public static class Navigation
    {
        /// <summary>Separator between segments of a dot-notation property path (e.g. "Order.Customer.Name").</summary>
        public const char Separator = '.';
    }
}
