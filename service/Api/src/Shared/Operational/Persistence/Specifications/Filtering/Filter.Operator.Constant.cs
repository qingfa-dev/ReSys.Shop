using Shared.Operational.Persistence.Specifications.Filtering.Expression;

namespace Shared.Operational.Persistence.Specifications.Filtering;

/// <summary>
/// Constants governing <see cref="FilterOperator"/> tokenization and parsing.
/// </summary>
internal static class FilterOperatorConstant
{
    /// <summary>
    /// Canonical DSL and JSON/query-string operator token strings.
    /// Every token used by <see cref="FilterOperatorMap"/> dictionaries,
    /// <see cref="FilterExpressionBuilder"/> switch statements, and the DSL
    /// lexer/parser is defined here.
    /// </summary>
    public static class Tokens
    {
        #region DSL symbols

        public const string Equal = "=";
        public const string EqualCaseSensitive = "==";
        public const string NotEqual = "!=";
        public const string GreaterThan = ">";
        public const string GreaterThanOrEqual = ">=";
        public const string LessThan = "<";
        public const string LessThanOrEqual = "<=";

        public const string Contains = "*";
        public const string ContainsCaseSensitive = "*~";
        public const string NotContains = "!*";

        public const string StartsWith = "^";
        public const string StartsWithCaseSensitive = "^~";
        public const string NotStartsWith = "!^";

        public const string EndsWith = "$";
        public const string EndsWithCaseSensitive = "$~";
        public const string NotEndsWith = "!$";

        #endregion DSL symbols

        #region JSON / query-string aliases

        public const string EqualAlias = "eq";
        public const string EqualCaseSensitiveAlias = "eq~";
        public const string NotEqualAlias = "neq";

        public const string GreaterThanAlias = "gt";
        public const string GreaterThanOrEqualAlias = "gte";
        public const string LessThanAlias = "lt";
        public const string LessThanOrEqualAlias = "lte";

        public const string ContainsAlias = "contains";
        public const string ContainsCaseSensitiveAlias = "contains~";
        public const string NotContainsAlias = "ncontains";

        public const string StartsWithAlias = "starts";
        public const string StartsWithCaseSensitiveAlias = "starts~";
        public const string NotStartsWithAlias = "nstarts";

        public const string EndsWithAlias = "ends";
        public const string EndsWithCaseSensitiveAlias = "ends~";
        public const string NotEndsWithAlias = "nends";

        #endregion JSON / query-string aliases

        #region Lookup tokens (used only by FilterDslParser for resolving case-sensitive DSL operators)

        /// <summary>
        /// The parse-table key used when the DSL tokenizer reads <c>==</c>
        /// (case-sensitive equality).
        /// </summary>
        public const string EqualCaseSensitiveLookup = "eq~";

        /// <summary>
        /// The parse-table key used when the DSL tokenizer reads <c>*~</c>
        /// (case-sensitive contains).
        /// </summary>
        public const string ContainsCaseSensitiveLookup = "contains~";

        /// <summary>
        /// The parse-table key used when the DSL tokenizer reads <c>^~</c>
        /// (case-sensitive starts-with).
        /// </summary>
        public const string StartsWithCaseSensitiveLookup = "starts~";

        /// <summary>
        /// The parse-table key used when the DSL tokenizer reads <c>$~</c>
        /// (case-sensitive ends-with).
        /// </summary>
        public const string EndsWithCaseSensitiveLookup = "ends~";

        #endregion Lookup tokens
    }
}
