using Shared.Operational.Persistence.Specifications.Filtering.Parsing;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering.Parsing;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterDslLexerTests
{
    #region Peek / Consume / SkipWhitespace / IsAtEnd

    [Fact(DisplayName = "Peek: Returns first char at start")]
    public void Peek_ShouldReturnFirstChar()
    {
        new FilterDslLexer("Name=Apple").Peek().Should().Be('N');
    }

    [Fact(DisplayName = "Peek: Returns EndOfInput at empty input")]
    public void Peek_ShouldReturnEndOfInput_ForEmpty()
    {
        new FilterDslLexer("").Peek().Should().Be(FilterDslLexerConstant.Delimiters.EndOfInput);
    }

    [Fact(DisplayName = "Consume: Advances on match, no-op on mismatch, no-op at EOF")]
    public void Consume_ShouldBehaveCorrectly()
    {
        FilterDslLexer lexer = new("Abc");
        lexer.Consume('A');
        lexer.Peek().Should().Be('b');

        lexer.Consume('X');
        lexer.Peek().Should().Be('b');

        FilterDslLexer emptyLexer = new("");
        Action act = () => emptyLexer.Consume('A');
        act.Should().NotThrow();
    }

    [Fact(DisplayName = "SkipWhitespace: Skips leading spaces and tabs")]
    public void SkipWhitespace_ShouldSkipSpaces()
    {
        FilterDslLexer lexer = new("   Name");
        lexer.SkipWhitespace();
        lexer.Peek().Should().Be('N');
    }

    [Theory(DisplayName = "IsAtEnd: True for empty or whitespace-only input")]
    [InlineData("", true)]
    [InlineData("   \t  ", true)]
    [InlineData("Name", false)]
    public void IsAtEnd_ShouldDetectEnd(string input, bool expected)
    {
        new FilterDslLexer(input).IsAtEnd.Should().Be(expected);
    }

    #endregion

    #region Match

    [Fact(DisplayName = "Match: Returns true and advances on match")]
    public void Match_ShouldReturnTrue_AndAdvance()
    {
        FilterDslLexer lexer = new("!=30");
        lexer.Match("!=").Should().BeTrue();
        lexer.Peek().Should().Be('3');
    }

    [Fact(DisplayName = "Match: Returns false on mismatch without advancing")]
    public void Match_ShouldReturnFalse_OnMismatch()
    {
        FilterDslLexer lexer = new("=30");
        lexer.Match("!=").Should().BeFalse();
        lexer.Peek().Should().Be('=');
    }

    [Fact(DisplayName = "Match: Returns false when token longer than remaining input")]
    public void Match_ShouldReturnFalse_WhenTokenTooLong()
    {
        FilterDslLexer lexer = new("=30");
        lexer.Match("===").Should().BeFalse();
    }

    [Fact(DisplayName = "Match: Multi-char matched before single-char (*~ before *)")]
    public void Match_MultiCharBeforeSingleChar()
    {
        FilterDslLexer lexer = new("*~");
        lexer.Match("*~").Should().BeTrue();
    }

    #endregion

    #region ReadFieldName

    [Theory(DisplayName = "ReadFieldName: Reads field names with various characters")]
    [InlineData("Name123=value", "Name123")]
    [InlineData("Order.Customer_Name=John", "Order.Customer_Name")]
    [InlineData("order-item=value", "order-item")]
    [InlineData("Age>=18", "Age")]
    [InlineData("Name=value", "Name")]
    public void ReadFieldName_ShouldReadCorrectly(string input, string expected)
    {
        new FilterDslLexer(input).ReadFieldName().Should().Be(expected);
    }

    [Fact(DisplayName = "ReadFieldName: Returns empty when starting at delimiter")]
    public void ReadFieldName_ShouldReturnEmpty_AtDelimiter()
    {
        new FilterDslLexer("=value").ReadFieldName().Should().Be("");
    }

    #endregion

    #region ReadOperator (parameterized)

    [Theory(DisplayName = "ReadOperator: All token variants with correct caseSensitive flag")]
    // Case-sensitive variants (longer tokens first)
    [InlineData("*~", "*", true)]
    [InlineData("^~", "^", true)]
    [InlineData("$~", "$", true)]
    // Negation variants
    [InlineData("!*", "!*", false)]
    [InlineData("!^", "!^", false)]
    [InlineData("!$", "!$", false)]
    [InlineData("!=", "!=", false)]
    // Two-character comparison
    [InlineData(">=", ">=", false)]
    [InlineData("<=", "<=", false)]
    // Case-sensitive equality shorthand
    [InlineData("==", "=", true)]
    // Single-character
    [InlineData(">", ">", false)]
    [InlineData("<", "<", false)]
    [InlineData("*", "*", false)]
    [InlineData("^", "^", false)]
    [InlineData("$", "$", false)]
    [InlineData("=", "=", false)]
    public void ReadOperator_ShouldReturnCorrectToken(string input, string expectedOp, bool expectedCs)
    {
        FilterDslLexer lexer = new(input);
        (string? op, bool cs) = lexer.ReadOperator();

        op.Should().Be(expectedOp);
        cs.Should().Be(expectedCs);
    }

    [Fact(DisplayName = "ReadOperator: Returns null when no operator found")]
    public void ReadOperator_ShouldReturnNull_WhenNoOperator()
    {
        FilterDslLexer lexer = new("abc");
        (string? op, _) = lexer.ReadOperator();
        op.Should().BeNull();
    }

    [Fact(DisplayName = "ReadOperator: Precedence — != matches before =")]
    public void ReadOperator_Precedence_BangEqualBeforeEqual()
    {
        FilterDslLexer lexer = new("!=30");
        (string? op, _) = lexer.ReadOperator();
        op.Should().Be("!=");
    }

    [Fact(DisplayName = "ReadOperator: Precedence — >= matches before >")]
    public void ReadOperator_Precedence_GteBeforeGt()
    {
        FilterDslLexer lexer = new(">=30");
        (string? op, _) = lexer.ReadOperator();
        op.Should().Be(">=");
    }

    #endregion

    #region ReadValue

    [Fact(DisplayName = "ReadValue: Reads plain value and trims whitespace")]
    public void ReadValue_ShouldTrimWhitespace()
    {
        new FilterDslLexer(" apple | other").ReadValue().Should().Be("apple");
    }

    [Fact(DisplayName = "ReadValue: Stops at delimiter")]
    public void ReadValue_ShouldStopAtDelimiter()
    {
        new FilterDslLexer("apple,Name=Banana").ReadValue().Should().Be("apple");
    }

    [Fact(DisplayName = "ReadValue: Quoted value preserves commas, pipes, parens")]
    public void ReadValue_Quoted_ShouldPreserveDelimiters()
    {
        new FilterDslLexer("\"Smith, John\"").ReadValue().Should().Be("Smith, John");
        new FilterDslLexer("\"A|B\"").ReadValue().Should().Be("A|B");
        new FilterDslLexer("\"foo(bar)\"").ReadValue().Should().Be("foo(bar)");
    }

    [Fact(DisplayName = "ReadValue: Empty quoted value returns empty string")]
    public void ReadValue_EmptyQuoted_ShouldReturnEmpty()
    {
        new FilterDslLexer("\"\"").ReadValue().Should().Be("");
    }

    [Fact(DisplayName = "ReadValue: Quoted then delimiter — only reads quoted part")]
    public void ReadValue_QuotedThenDelimiter_ShouldReadOnlyQuoted()
    {
        new FilterDslLexer("\"hello\",next").ReadValue().Should().Be("hello");
    }

    #endregion

    #region ApplyWildcardShorthand (parameterized)

    [Theory(DisplayName = "ApplyWildcardShorthand: Wildcard patterns resolve to correct op")]
    [InlineData("*ap*", "*", "ap")]
    [InlineData("*hello*", "*", "hello")]
    [InlineData("*ple", "$", "ple")]
    [InlineData("*suffix", "$", "suffix")]
    [InlineData("App*", "^", "App")]
    [InlineData("prefix*", "^", "prefix")]
    [InlineData("plain", "=", "plain")]
    [InlineData("*", "=", "*")]
    [InlineData("**", "$", "*")]
    [InlineData("a", "=", "a")]
    public void ApplyWildcardShorthand_ShouldResolveCorrectly(string input, string expectedOp, string expectedValue)
    {
        (string op, string value) = FilterDslLexer.ApplyWildcardShorthand(input);
        op.Should().Be(expectedOp);
        value.Should().Be(expectedValue);
    }

    #endregion

    #region Integration: Full Tokenization

    [Fact(DisplayName = "Integration: Full tokenization of a condition")]
    public void Integration_FullTokenization()
    {
        FilterDslLexer lexer = new("Name=*ap*");

        lexer.ReadFieldName().Should().Be("Name");
        (string? op, bool cs) = lexer.ReadOperator();
        op.Should().Be("=");
        cs.Should().BeFalse();
        lexer.ReadValue().Should().Be("*ap*");
    }

    [Fact(DisplayName = "Integration: Tokenization with quoted value")]
    public void Integration_TokenizationWithQuotedValue()
    {
        FilterDslLexer lexer = new("Tag=\"Smith, John\"");

        lexer.ReadFieldName().Should().Be("Tag");
        (string? op, _) = lexer.ReadOperator();
        op.Should().Be("=");
        lexer.ReadValue().Should().Be("Smith, John");
    }

    [Fact(DisplayName = "Integration: Tokenization with whitespace")]
    public void Integration_TokenizationWithWhitespace()
    {
        FilterDslLexer lexer = new(" Age >= 18 ");

        lexer.SkipWhitespace();
        lexer.ReadFieldName().Should().Be("Age");
        lexer.SkipWhitespace();
        (string? op, _) = lexer.ReadOperator();
        op.Should().Be(">=");
        lexer.SkipWhitespace();
        lexer.ReadValue().Should().Be("18");
    }

    #endregion
}
