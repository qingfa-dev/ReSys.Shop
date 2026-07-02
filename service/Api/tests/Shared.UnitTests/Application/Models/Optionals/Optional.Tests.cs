using Shared.Application.Models.Optionals;

namespace Shared.UnitTests.Application.Models.Optionals;

public sealed class OptionalTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    #region HasValue / IsNone
    [Fact(DisplayName = "HasValue: should be true for Some")]
    public void HasValue_ShouldBeTrue_ForSome()
    {
        Optional<int> opt = Optional<int>.Some(42);

        _output.WriteLine("Some(42).HasValue = {0}", opt.HasValue);

        opt.HasValue.Should().BeTrue();
    }

    [Fact(DisplayName = "HasValue: should be false for None")]
    public void HasValue_ShouldBeFalse_ForNone()
    {
        Optional<int> opt = Optional<int>.None;

        opt.HasValue.Should().BeFalse();
    }

    [Fact(DisplayName = "IsNone: should be false for Some")]
    public void IsNone_ShouldBeFalse_ForSome()
    {
        Optional<int> opt = Optional<int>.Some(42);

        opt.IsNone.Should().BeFalse();
    }

    [Fact(DisplayName = "IsNone: should be true for None")]
    public void IsNone_ShouldBeTrue_ForNone()
    {
        Optional<int> opt = Optional<int>.None;

        opt.IsNone.Should().BeTrue();
    }
    #endregion

    #region Value
    [Fact(DisplayName = "Value: should return contained value for Some")]
    public void Value_ShouldReturnContainedValue_ForSome()
    {
        Optional<int> opt = Optional<int>.Some(42);

        int value = opt.Value;

        value.Should().Be(42);
    }

    [Fact(DisplayName = "Value: should throw InvalidOperationException for None")]
    public void Value_ShouldThrowInvalidOperationException_ForNone()
    {
        Optional<int> opt = Optional<int>.None;

        Action act = () => _ = opt.Value;

        act.Should().Throw<InvalidOperationException>().WithMessage("Optional has no value.");
    }
    #endregion

    #region Equality
    [Fact(DisplayName = "Equals: two Some with equal values should be equal")]
    public void Equals_TwoSomeWithEqualValues_ShouldBeEqual()
    {
        Optional<int> a = Optional<int>.Some(42);
        Optional<int> b = Optional<int>.Some(42);

        bool result = a.Equals(b);

        _output.WriteLine("Some(42) == Some(42): {0}", result);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "Equals: two Some with different values should not be equal")]
    public void Equals_TwoSomeWithDifferentValues_ShouldNotBeEqual()
    {
        Optional<int> a = Optional<int>.Some(42);
        Optional<int> b = Optional<int>.Some(99);

        a.Equals(b).Should().BeFalse();
    }

    [Fact(DisplayName = "Equals: two None should be equal")]
    public void Equals_TwoNone_ShouldBeEqual()
    {
        Optional<int> a = Optional<int>.None;
        Optional<int> b = Optional<int>.None;

        a.Equals(b).Should().BeTrue();
    }

    [Fact(DisplayName = "Equals: Some and None should not be equal")]
    public void Equals_SomeAndNone_ShouldNotBeEqual()
    {
        Optional<int> a = Optional<int>.Some(42);
        Optional<int> b = Optional<int>.None;

        a.Equals(b).Should().BeFalse();
    }
    #endregion

    #region operator == / !=
    [Fact(DisplayName = "operator ==: should return true for equal optionals")]
    public void OperatorEquals_ShouldReturnTrue_ForEqualOptionals()
    {
        Optional<int> a = Optional<int>.Some(42);
        Optional<int> b = Optional<int>.Some(42);

        (a == b).Should().BeTrue();
    }

    [Fact(DisplayName = "operator ==: should return false for different optionals")]
    public void OperatorEquals_ShouldReturnFalse_ForDifferentOptionals()
    {
        Optional<int> a = Optional<int>.Some(42);
        Optional<int> b = Optional<int>.None;

        (a == b).Should().BeFalse();
    }

    [Fact(DisplayName = "operator !=: should return false for equal optionals")]
    public void OperatorNotEquals_ShouldReturnFalse_ForEqualOptionals()
    {
        Optional<int> a = Optional<int>.Some(42);
        Optional<int> b = Optional<int>.Some(42);

        (a != b).Should().BeFalse();
    }

    [Fact(DisplayName = "operator !=: should return true for different optionals")]
    public void OperatorNotEquals_ShouldReturnTrue_ForDifferentOptionals()
    {
        Optional<int> a = Optional<int>.Some(42);
        Optional<int> b = Optional<int>.None;

        (a != b).Should().BeTrue();
    }
    #endregion

    #region GetHashCode
    [Fact(DisplayName = "GetHashCode: equal optionals should have equal hash codes")]
    public void GetHashCode_EqualOptionals_ShouldHaveEqualHashCodes()
    {
        Optional<int> a = Optional<int>.Some(42);
        Optional<int> b = Optional<int>.Some(42);

        int hashA = a.GetHashCode();
        int hashB = b.GetHashCode();

        _output.WriteLine("Hash(Some(42)): {0}, {1}", hashA, hashB);

        hashA.Should().Be(hashB);
    }

    [Fact(DisplayName = "GetHashCode: None instances should have equal hash codes")]
    public void GetHashCode_NoneInstances_ShouldHaveEqualHashCodes()
    {
        Optional<int> a = Optional<int>.None;
        Optional<int> b = Optional<int>.None;

        a.GetHashCode().Should().Be(b.GetHashCode());
    }
    #endregion

    #region ToString
    [Fact(DisplayName = "ToString: Some should return Some(value)")]
    public void ToString_Some_ShouldReturnSomeValue()
    {
        Optional<int> opt = Optional<int>.Some(42);

        string text = opt.ToString();

        _output.WriteLine("ToString: {0}", text);

        text.Should().Be("Some(42)");
    }

    [Fact(DisplayName = "ToString: None should return None")]
    public void ToString_None_ShouldReturnNone()
    {
        Optional<int> opt = Optional<int>.None;

        string text = opt.ToString();

        text.Should().Be("None");
    }
    #endregion
}
