namespace Shared.Operational.Persistence.Specifications.Filtering.Parsing;

/// <summary>
/// Shared DSL lexer used by the model-tree parser (<see cref="FilterDslParser"/>)
/// and consequently by <see cref="FilterDslParser"/> (model-tree path).
/// </summary>
/// <remarks>
/// Stateful: one instance per parse call. Tracks <c>_pos</c> through the <c>_input</c> string.
/// Methods return tokens as they are consumed; callers compose them into parse trees.
/// </remarks>
internal sealed class FilterDslLexer
{
    private readonly string _input;
    private int _pos;

    internal FilterDslLexer(string input)
    {
        _input = input;
        _pos = 0;
    }

    #region Character Navigation

    internal char Peek() => _pos < _input.Length ? _input[_pos] : FilterDslLexerConstant.Delimiters.EndOfInput;

    internal void Consume(char expected)
    {
        if (_pos < _input.Length && _input[_pos] == expected)
            _pos++;
    }

    internal void SkipWhitespace()
    {
        while (_pos < _input.Length && char.IsWhiteSpace(_input[_pos]))
            _pos++;
    }

    internal bool IsAtEnd
    {
        get
        {
            SkipWhitespace();
            return _pos >= _input.Length;
        }
    }

    #endregion Character Navigation

    #region Token Matching

    /// <summary>
    /// Attempts to match <paramref name="token"/> at the current position.
    /// Advances <c>_pos</c> on success.
    /// </summary>
    internal bool Match(string token)
    {
        if (_pos + token.Length > _input.Length) return false;
        if (_input.Substring(_pos, token.Length) != token) return false;
        _pos += token.Length;
        return true;
    }

    #endregion Token Matching

    #region Field Name

    /// <summary>
    /// Reads a field name: letters, digits, underscores, dots, hyphens.
    /// Advances <c>_pos</c> past the read characters.
    /// </summary>
    internal string ReadFieldName()
    {
        int start = _pos;
        while (_pos < _input.Length)
        {
            char ch = _input[_pos];
            if (FilterDslLexerConstant.Constraints.IsValidFieldNameChar(ch)) _pos++;
            else break;
        }
        return _input[start.._pos];
    }

    #endregion Field Name

    #region Operator

    /// <summary>
    /// Reads a DSL operator token. Multi-character operators are matched before
    /// their single-character prefixes to avoid partial matches (e.g. "*~" before "*").
    /// </summary>
    /// <returns>
    /// A tuple of (raw operator string, caseSensitive flag).
    /// Returns (<see langword="null"/>, false) when no operator is found.
    /// </returns>
    internal (string? op, bool caseSensitive) ReadOperator()
    {
        // Case-sensitive variants (longer tokens first).
        if (Match(FilterOperatorConstant.Tokens.ContainsCaseSensitive)) return (FilterOperatorConstant.Tokens.Contains, caseSensitive: true);
        if (Match(FilterOperatorConstant.Tokens.StartsWithCaseSensitive)) return (FilterOperatorConstant.Tokens.StartsWith, caseSensitive: true);
        if (Match(FilterOperatorConstant.Tokens.EndsWithCaseSensitive)) return (FilterOperatorConstant.Tokens.EndsWith, caseSensitive: true);

        // Negation variants.
        if (Match(FilterOperatorConstant.Tokens.NotContains)) return (FilterOperatorConstant.Tokens.NotContains, caseSensitive: false);
        if (Match(FilterOperatorConstant.Tokens.NotStartsWith)) return (FilterOperatorConstant.Tokens.NotStartsWith, caseSensitive: false);
        if (Match(FilterOperatorConstant.Tokens.NotEndsWith)) return (FilterOperatorConstant.Tokens.NotEndsWith, caseSensitive: false);
        if (Match(FilterOperatorConstant.Tokens.NotEqual)) return (FilterOperatorConstant.Tokens.NotEqual, caseSensitive: false);

        // Two-character comparison operators.
        if (Match(FilterOperatorConstant.Tokens.GreaterThanOrEqual)) return (FilterOperatorConstant.Tokens.GreaterThanOrEqual, caseSensitive: false);
        if (Match(FilterOperatorConstant.Tokens.LessThanOrEqual)) return (FilterOperatorConstant.Tokens.LessThanOrEqual, caseSensitive: false);

        // Case-sensitive equality shorthand.
        if (Match(FilterOperatorConstant.Tokens.EqualCaseSensitive)) return (FilterOperatorConstant.Tokens.Equal, caseSensitive: true);

        // Single-character operators.
        if (Match(FilterOperatorConstant.Tokens.GreaterThan)) return (FilterOperatorConstant.Tokens.GreaterThan, caseSensitive: false);
        if (Match(FilterOperatorConstant.Tokens.LessThan)) return (FilterOperatorConstant.Tokens.LessThan, caseSensitive: false);
        if (Match(FilterOperatorConstant.Tokens.Contains)) return (FilterOperatorConstant.Tokens.Contains, caseSensitive: false);
        if (Match(FilterOperatorConstant.Tokens.StartsWith)) return (FilterOperatorConstant.Tokens.StartsWith, caseSensitive: false);
        if (Match(FilterOperatorConstant.Tokens.EndsWith)) return (FilterOperatorConstant.Tokens.EndsWith, caseSensitive: false);
        if (Match(FilterOperatorConstant.Tokens.Equal)) return (FilterOperatorConstant.Tokens.Equal, caseSensitive: false);

        return (null, false);
    }

    #endregion Operator

    #region Value

    /// <summary>
    /// Reads a filter value. Supports double-quoted strings that may contain
    /// commas, pipes, and parentheses without being misinterpreted as DSL tokens.
    /// Plain (unquoted) values terminate at <c>,</c>, <c>|</c>, or <c>)</c>.
    /// </summary>
    internal string ReadValue()
    {
        // Handle quoted strings — e.g. "Smith, John" or "pipe|value".
        if (_pos < _input.Length && _input[_pos] == FilterDslLexerConstant.Delimiters.Quote)
        {
            _pos++; // consume opening quote
            int start = _pos;

            while (_pos < _input.Length && _input[_pos] != FilterDslLexerConstant.Delimiters.Quote)
                _pos++;

            string quoted = _input[start.._pos];

            if (_pos < _input.Length)
                _pos++; // consume closing quote

            return quoted;
        }

        // Plain value: read until a DSL delimiter.
        int plainStart = _pos;

        while (_pos < _input.Length)
        {
            char ch = _input[_pos];
            if (FilterDslLexerConstant.ValueTerminators.Chars.Contains(ch))
                break;
            _pos++;
        }

        return _input[plainStart.._pos].Trim();
    }

    #endregion Value

    #region Wildcard Shorthand

    /// <summary>
    /// Expands the <c>= *text*</c> wildcard shorthand into the correct operator and
    /// stripped value.
    /// </summary>
    /// <param name="value">The raw value that may contain leading/trailing <c>*</c>.</param>
    /// <returns>
    /// <c>("*", value)</c> for contains,
    /// <c>("$", value)</c> for ends-with,
    /// <c>("^", value)</c> for starts-with,
    /// <c>("=", value)</c> for plain equality.
    /// </returns>
    internal static (string op, string value) ApplyWildcardShorthand(string value)
    {
        if (value.StartsWith(FilterDslLexerConstant.Delimiters.Wildcard) && value.EndsWith(FilterDslLexerConstant.Delimiters.Wildcard) && value.Length > FilterDslLexerConstant.Constraints.WildcardBothMinLength)
            return (FilterOperatorConstant.Tokens.Contains, value[1..^1]);

        if (value.StartsWith(FilterDslLexerConstant.Delimiters.Wildcard) && value.Length > FilterDslLexerConstant.Constraints.WildcardSingleMinLength)
            return (FilterOperatorConstant.Tokens.EndsWith, value[1..]);

        if (value.EndsWith(FilterDslLexerConstant.Delimiters.Wildcard) && value.Length > FilterDslLexerConstant.Constraints.WildcardSingleMinLength)
            return (FilterOperatorConstant.Tokens.StartsWith, value[..^1]);

        return (FilterOperatorConstant.Tokens.Equal, value);
    }

    #endregion Wildcard Shorthand
}
