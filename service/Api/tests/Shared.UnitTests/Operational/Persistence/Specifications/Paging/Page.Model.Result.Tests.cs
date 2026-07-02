using Shared.Operational.Persistence.Specifications.Paging;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Paging;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class PageModelResultTests
{
    [Theory]
    [InlineData(nameof(PageModelResult.Success.Parsed), "Page parsed successfully.")]
    [InlineData(nameof(PageModelResult.Success.Empty), "No page input provided; empty model returned.")]
    public void SuccessMsg_ShouldHaveExpectedMessage(string constantName, string expected)
    {
        string actual = constantName switch
        {
            nameof(PageModelResult.Success.Parsed) => PageModelResult.Success.Parsed,
            nameof(PageModelResult.Success.Empty) => PageModelResult.Success.Empty,
            _ => string.Empty
        };

        actual.Should().Be(expected);
    }

    [Fact]
    public void Failure_InvalidJson_ShouldHaveCorrectCode()
    {
        Error error = PageModelResult.Failure.InvalidJson("bad input");

        error.Code.Should().Be("Paging.InvalidJson");
        error.Message.Should().Contain("bad input");
    }

    [Fact]
    public void Failure_InvalidNumber_ShouldContainPropertyAndValue()
    {
        Error error = PageModelResult.Failure.InvalidNumber("page", "abc");

        error.Code.Should().Be("Paging.InvalidNumber");
        error.Message.Should().Contain("page");
        error.Message.Should().Contain("abc");
    }
}
