using Shared.Operational.Persistence.Specifications.Sorting;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Sorting;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SortModelResultTests
{
    [Theory]
    [InlineData(nameof(SortModelResult.Success.Parsed), "Sort parsed successfully.")]
    [InlineData(nameof(SortModelResult.Success.Empty), "No sort input provided; empty model returned.")]
    public void SuccessMsg_ShouldHaveExpectedMessage(string constantName, string expected)
    {
        string actual = constantName switch
        {
            nameof(SortModelResult.Success.Parsed) => SortModelResult.Success.Parsed,
            nameof(SortModelResult.Success.Empty) => SortModelResult.Success.Empty,
            _ => string.Empty
        };

        actual.Should().Be(expected);
    }

    [Fact]
    public void Failure_InvalidSyntax_ShouldHaveCorrectCode()
    {
        Error error = SortModelResult.Failure.InvalidSyntax("bad");

        error.Code.Should().Be("Sorting.Parsing.InvalidSyntax");
        error.Message.Should().Contain("bad");
    }

    [Fact]
    public void Failure_InvalidJson_ShouldHaveCorrectCode()
    {
        Error error = SortModelResult.Failure.InvalidJson("detail");

        error.Code.Should().Be("Sorting.Parsing.InvalidJson");
        error.Message.Should().Contain("detail");
    }

    [Fact]
    public void Failure_DisallowedField_ShouldContainFieldName()
    {
        Error error = SortModelResult.Failure.DisallowedField("Forbidden");

        error.Code.Should().Be("Sorting.Field.Disallowed");
        error.Message.Should().Contain("Forbidden");
    }

    [Fact]
    public void Failure_DisallowedFields_ShouldContainFieldNames()
    {
        Error error = SortModelResult.Failure.DisallowedFields(["A", "B"]);

        error.Code.Should().Be("Sorting.Field.Disallowed");
        error.Message.Should().Contain("A").And.Contain("B");
    }

    [Fact]
    public void Failure_UnknownDirection_ShouldContainValue()
    {
        Error error = SortModelResult.Failure.UnknownDirection("sideways");

        error.Code.Should().Be("Sorting.Direction.Unknown");
        error.Message.Should().Contain("sideways");
    }

    [Fact]
    public void Failure_UnknownNulls_ShouldContainValue()
    {
        Error error = SortModelResult.Failure.UnknownNulls("middle");

        error.Code.Should().Be("Sorting.Nulls.Unknown");
        error.Message.Should().Contain("middle");
    }

    [Fact]
    public void Failure_MissingField_ShouldHaveCorrectCode()
    {
        Error error = SortModelResult.Failure.MissingField;

        error.Code.Should().Be("Sorting.Field.Missing");
    }
}
