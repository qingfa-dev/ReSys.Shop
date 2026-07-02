using Shared.Application.Models.Optionals;

namespace Shared.UnitTests.Application.Models.Optionals;

public sealed class OptionalConstantTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    #region None
    [Fact(DisplayName = "None: should have HasValue false")]
    public void None_ShouldHaveHasValueFalse()
    {
        Optional<int> none = Optional<int>.None;

        _output.WriteLine("None.HasValue = {0}", none.HasValue);

        none.HasValue.Should().BeFalse();
    }

    [Fact(DisplayName = "None: should have IsNone true")]
    public void None_ShouldHaveIsNoneTrue()
    {
        Optional<int> none = Optional<int>.None;

        none.IsNone.Should().BeTrue();
    }

    [Fact(DisplayName = "None: Value getter should throw InvalidOperationException")]
    public void None_ValueGetter_ShouldThrowInvalidOperationException()
    {
        Optional<int> none = Optional<int>.None;

        Action act = () => _ = none.Value;

        act.Should().Throw<InvalidOperationException>().WithMessage("Optional has no value.");
    }

    [Fact(DisplayName = "None: should be default for reference types")]
    public void None_ShouldBeDefaultForReferenceTypes()
    {
        Optional<string> none = Optional<string>.None;

        none.HasValue.Should().BeFalse();
        none.IsNone.Should().BeTrue();
    }
    #endregion
}
