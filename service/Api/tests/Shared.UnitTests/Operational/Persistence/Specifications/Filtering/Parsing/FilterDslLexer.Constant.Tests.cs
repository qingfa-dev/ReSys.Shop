using Shared.Operational.Persistence.Specifications.Filtering.Parsing;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering.Parsing;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterDslLexerConstantTests
{
    [Fact(DisplayName = "Delimiters: Or is '|'")]
    public void Delimiters_Or_ShouldBePipe()
    {
        FilterDslLexerConstant.Delimiters.Or.Should().Be('|');
    }

    [Fact(DisplayName = "Delimiters: And is ','")]
    public void Delimiters_And_ShouldBeComma()
    {
        FilterDslLexerConstant.Delimiters.And.Should().Be(',');
    }

    [Fact(DisplayName = "Delimiters: OpenParen is '('")]
    public void Delimiters_OpenParen_ShouldBeOpenParen()
    {
        FilterDslLexerConstant.Delimiters.OpenParen.Should().Be('(');
    }

    [Fact(DisplayName = "Delimiters: CloseParen is ')'")]
    public void Delimiters_CloseParen_ShouldBeCloseParen()
    {
        FilterDslLexerConstant.Delimiters.CloseParen.Should().Be(')');
    }

    [Fact(DisplayName = "Delimiters: Quote is '\"'")]
    public void Delimiters_Quote_ShouldBeQuote()
    {
        FilterDslLexerConstant.Delimiters.Quote.Should().Be('"');
    }

    [Fact(DisplayName = "Delimiters: Wildcard is '*'")]
    public void Delimiters_Wildcard_ShouldBeAsterisk()
    {
        FilterDslLexerConstant.Delimiters.Wildcard.Should().Be('*');
    }

    [Fact(DisplayName = "Delimiters: EndOfInput is '\0'")]
    public void Delimiters_EndOfInput_ShouldBeNullChar()
    {
        FilterDslLexerConstant.Delimiters.EndOfInput.Should().Be('\0');
    }

    [Fact(DisplayName = "ValueTerminators: Chars contains comma, pipe, close-paren")]
    public void ValueTerminators_Chars_ShouldContainExpected()
    {
        FilterDslLexerConstant.ValueTerminators.Chars.Should().Contain(',');
        FilterDslLexerConstant.ValueTerminators.Chars.Should().Contain('|');
        FilterDslLexerConstant.ValueTerminators.Chars.Should().Contain(')');
        FilterDslLexerConstant.ValueTerminators.Chars.Should().HaveCount(3);
    }

    [Fact(DisplayName = "Constraints: WildcardBothMinLength is 2")]
    public void Constraints_WildcardBothMinLength_ShouldBeTwo()
    {
        FilterDslLexerConstant.Constraints.WildcardBothMinLength.Should().Be(2);
    }

    [Fact(DisplayName = "Constraints: WildcardSingleMinLength is 1")]
    public void Constraints_WildcardSingleMinLength_ShouldBeOne()
    {
        FilterDslLexerConstant.Constraints.WildcardSingleMinLength.Should().Be(1);
    }
}
