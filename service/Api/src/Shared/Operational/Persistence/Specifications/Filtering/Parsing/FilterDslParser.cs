namespace Shared.Operational.Persistence.Specifications.Filtering.Parsing;

// AgentHint: Called only from FilterModelExtensions.FromString.
//            Produces a FilterGroup tree from a DSL string; no Expression trees are built here.
//            Do not add public members — internal is the maximum visibility.

/// <summary>
/// Parses a DSL filter string into a <see cref="FilterGroup"/> tree.
/// Produces model objects (<see cref="FilterGroup"/> trees) instead of Expression nodes.
/// </summary>
internal static class FilterDslParser
{
    /// <summary>
    /// Parses the full DSL <paramref name="input"/> string into a root
    /// <see cref="FilterGroup"/>. Throws on unrecoverable syntax errors so that the
    /// caller's try/catch can surface a typed <see cref="FilterModelResult.Failure.InvalidSyntax"/>
    /// failure.
    /// </summary>
    internal static FilterGroup Parse(string input)
        => new Tokenizer(input).ParseOr();

    // ── Tokenizer ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Stateful recursive-descent tokenizer. One instance per parse call.
    /// Wraps <see cref="FilterDslLexer"/> for character-level operations.
    /// </summary>
    private sealed class Tokenizer
    {
        private readonly FilterDslLexer _lexer;

        internal Tokenizer(string input)
        {
            _lexer = new FilterDslLexer(input);
        }

        #region Recursive Descent

        // Entry point — OR has the lowest precedence.
        internal FilterGroup ParseOr()
        {
            var arms = new List<FilterGroup> { ParseAnd() };

            while (!_lexer.IsAtEnd && _lexer.Peek() == FilterDslLexerConstant.Delimiters.Or)
            {
                _lexer.Consume(FilterDslLexerConstant.Delimiters.Or);
                arms.Add(ParseAnd());
            }

            if (arms.Count == 1) return arms[0];

            // Merge: Promote flat AND arms to direct conditions; keep compound arms as sub-groups.
            var conditions = new List<FilterCondition>();
            var subGroups = new List<FilterGroup>();

            foreach (FilterGroup arm in arms)
            {
                if (arm.Groups.Count == 0)
                    conditions.AddRange(arm.Conditions);
                else
                    subGroups.Add(arm);
            }

            return new FilterGroup { Logic = FilterLogic.Or, Conditions = conditions.AsReadOnly(), Groups = subGroups.AsReadOnly() };
        }

        private FilterGroup ParseAnd()
        {
            var conditions = new List<FilterCondition>();
            var subGroups = new List<FilterGroup>();

            AppendFactor(conditions, subGroups);

            while (!_lexer.IsAtEnd && _lexer.Peek() == FilterDslLexerConstant.Delimiters.And)
            {
                _lexer.Consume(FilterDslLexerConstant.Delimiters.And);
                AppendFactor(conditions, subGroups);
            }

            return new FilterGroup { Logic = FilterLogic.And, Conditions = conditions.AsReadOnly(), Groups = subGroups.AsReadOnly() };
        }

        private void AppendFactor(List<FilterCondition> conditions, List<FilterGroup> groups)
        {
            _lexer.SkipWhitespace();

            // Handle: Parenthesized sub-expression becomes a nested group.
            if (_lexer.Peek() == FilterDslLexerConstant.Delimiters.OpenParen)
            {
                _lexer.Consume(FilterDslLexerConstant.Delimiters.OpenParen);
                FilterGroup nested = ParseOr();
                _lexer.SkipWhitespace();
                if (_lexer.Peek() == FilterDslLexerConstant.Delimiters.CloseParen) _lexer.Consume(FilterDslLexerConstant.Delimiters.CloseParen);
                groups.Add(nested);
                return;
            }

            // Parse: Leaf condition.
            FilterCondition? condition = ParseCondition();
            if (condition is not null) conditions.Add(condition);
        }

        private FilterCondition? ParseCondition()
        {
            _lexer.SkipWhitespace();

            string field = _lexer.ReadFieldName();
            if (string.IsNullOrEmpty(field)) return null;

            _lexer.SkipWhitespace();

            (string? rawOp, bool caseSensitive) = _lexer.ReadOperator();
            if (rawOp is null) return null;

            _lexer.SkipWhitespace();

            string value = _lexer.ReadValue();

            // Transform: Expand '*' wildcard shorthand before operator mapping.
            if (rawOp == FilterOperatorConstant.Tokens.Equal && !caseSensitive && value.Length > 0)
                (rawOp, value) = FilterDslLexer.ApplyWildcardShorthand(value);

            // Resolve: Map the raw DSL token + case-sensitivity flag to a FilterOperator.
            string lookupToken = (rawOp, caseSensitive) switch
            {
                (FilterOperatorConstant.Tokens.Equal, true) => FilterOperatorConstant.Tokens.EqualCaseSensitiveLookup,
                (FilterOperatorConstant.Tokens.Contains, true) => FilterOperatorConstant.Tokens.ContainsCaseSensitiveLookup,
                (FilterOperatorConstant.Tokens.StartsWith, true) => FilterOperatorConstant.Tokens.StartsWithCaseSensitiveLookup,
                (FilterOperatorConstant.Tokens.EndsWith, true) => FilterOperatorConstant.Tokens.EndsWithCaseSensitiveLookup,
                (var t, _) => t,
            };

            return FilterOperatorMap.TryParse(lookupToken, out FilterOperator op)
                ? new FilterCondition { Field = field, Operator = op, Value = value }
                : null;
        }

        #endregion Recursive Descent
    }
}
