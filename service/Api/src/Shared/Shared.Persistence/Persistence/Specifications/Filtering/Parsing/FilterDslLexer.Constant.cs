namespace Shared.Operational.Persistence.Specifications.Filtering.Parsing;

/// <summary>
/// Constants governing the DSL lexer behaviour: delimiter characters, field-name
/// rules, wildcard shorthand constraints, and value-reading terminators.
/// </summary>
internal static class FilterDslLexerConstant
{
    /// <summary>
    /// DSL syntax delimiter characters.
    /// </summary>
    public static class Delimiters
    {
        /// <summary>OR connective between filter expressions.</summary>
        public const char Or = '|';

        /// <summary>AND connective (list separator) between filter expressions.</summary>
        public const char And = ',';

        /// <summary>Opens a grouped sub-expression.</summary>
        public const char OpenParen = '(';

        /// <summary>Closes a grouped sub-expression.</summary>
        public const char CloseParen = ')';

        /// <summary>Quotes a literal value containing DSL delimiters.</summary>
        public const char Quote = '"';

        /// <summary>Wildcard character for contains/starts/ends shorthand in values.</summary>
        public const char Wildcard = '*';

        /// <summary>Sentinel returned by Peek() at end of input.</summary>
        public const char EndOfInput = '\0';
    }

    /// <summary>
    /// Characters that terminate a plain (unquoted) value token during lexing.
    /// Quoted values are terminated only by the closing quote.
    /// </summary>
    public static class ValueTerminators
    {
        /// <summary>Characters that end an unquoted value.</summary>
        public static readonly char[] Chars = [',', '|', ')'];
    }

    /// <summary>
    /// Constraints governing wildcard-shorthand detection and field-name validation.
    /// </summary>
    public static class Constraints
    {
        /// <summary>
        /// Minimum total length for a <c>*text*</c> wildcard value to be recognized
        /// as a contains pattern (must be at least <c>*a*</c>).
        /// </summary>
        public const int WildcardBothMinLength = 2;

        /// <summary>
        /// Minimum length for a <c>*text</c> or <c>text*</c> value to be recognized
        /// as a single-sided wildcard.
        /// </summary>
        public const int WildcardSingleMinLength = 1;

        /// <summary>
        /// Valid characters for field names: letters, digits, underscores, dots, hyphens.
        /// </summary>
        public static bool IsValidFieldNameChar(char ch) =>
            char.IsLetterOrDigit(ch) || ch == '_' || ch == '.' || ch == '-';
    }
}
