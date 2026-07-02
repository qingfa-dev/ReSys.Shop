using System.Globalization;

using Shared.Application.Models.Optionals;

namespace Shared.UnitTests.Application.Models.Optionals;

public sealed class OptionalMethodTests(ITestOutputHelper output)
{
    #region Some
    [Fact(DisplayName = "Some: should create optional with HasValue true")]
    public void Some_ShouldCreateOptionalWithHasValueTrue()
    {
        Optional<int> opt = Optional<int>.Some(42);

        output.WriteLine("Some(42).HasValue = {0}", opt.HasValue);

        opt.HasValue.Should().BeTrue();
    }

    [Fact(DisplayName = "Some: should throw ArgumentNullException when value is null")]
    public void Some_ShouldThrowArgumentNullException_WhenValueIsNull()
    {
        Action act = () => Optional<string>.Some(null!);

        act.Should().Throw<ArgumentNullException>();
    }
    #endregion

    #region Implicit / Explicit operators
    [Fact(DisplayName = "Implicit: value should implicitly convert to Some(value)")]
    public void Implicit_ShouldConvertValueToSome()
    {
        Optional<int> opt = 42;

        opt.HasValue.Should().BeTrue();
    }

    [Fact(DisplayName = "Explicit: Some should explicitly convert to T")]
    public void Explicit_ShouldConvertSomeToT()
    {
        Optional<int> opt = Optional<int>.Some(42);

        int value = (int)opt;

        value.Should().Be(42);
    }

    [Fact(DisplayName = "Explicit: None should throw when converting to T")]
    public void Explicit_NoneShouldThrow_WhenConvertingToT()
    {
        Optional<int> opt = Optional<int>.None;

        Action act = () => _ = (int)opt;

        act.Should().Throw<InvalidOperationException>();
    }
    #endregion

    #region Map
    [Fact(DisplayName = "Map: should transform contained value")]
    public void Map_ShouldTransformContainedValue()
    {
        Optional<int> opt = Optional<int>.Some(21);
        Optional<string> mapped = opt.Map(x => (x * 2).ToString(CultureInfo.InvariantCulture));

        output.WriteLine("Map result: {0}", mapped);

        mapped.HasValue.Should().BeTrue();
        mapped.Value.Should().Be("42");
    }

    [Fact(DisplayName = "Map: on None should return None")]
    public void Map_OnNone_ShouldReturnNone()
    {
        Optional<int> opt = Optional<int>.None;
        Optional<string> mapped = opt.Map(x => x.ToString(CultureInfo.InvariantCulture));

        mapped.HasValue.Should().BeFalse();
    }

    [Fact(DisplayName = "Map: should throw ArgumentNullException when selector is null")]
    public void Map_ShouldThrowArgumentNullException_WhenSelectorIsNull()
    {
        Optional<int> opt = Optional<int>.Some(42);

        Action act = () => opt.Map<int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }
    #endregion

    #region Bind
    [Fact(DisplayName = "Bind: should flat-map contained value")]
    public void Bind_ShouldFlatMapContainedValue()
    {
        Optional<int> opt = Optional<int>.Some(3);
        Optional<int> bound = opt.Bind(x => Optional<int>.Some(x * 10));

        output.WriteLine("Bind result: {0}", bound);

        bound.HasValue.Should().BeTrue();
        bound.Value.Should().Be(30);
    }

    [Fact(DisplayName = "Bind: on None should return None")]
    public void Bind_OnNone_ShouldReturnNone()
    {
        Optional<int> opt = Optional<int>.None;
        Optional<int> bound = opt.Bind(x => Optional<int>.Some(x));

        bound.HasValue.Should().BeFalse();
    }

    [Fact(DisplayName = "Bind: should throw ArgumentNullException when binder is null")]
    public void Bind_ShouldThrowArgumentNullException_WhenBinderIsNull()
    {
        Optional<int> opt = Optional<int>.Some(42);

        Action act = () => opt.Bind<int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }
    #endregion

    #region Filter
    [Fact(DisplayName = "Filter: should return same optional when predicate passes")]
    public void Filter_ShouldReturnSameOptional_WhenPredicatePasses()
    {
        Optional<int> opt = Optional<int>.Some(42);
        Optional<int> filtered = opt.Filter(x => x > 10);

        output.WriteLine("Filter(>10) on Some(42): {0}", filtered);

        filtered.HasValue.Should().BeTrue();
        filtered.Value.Should().Be(42);
    }

    [Fact(DisplayName = "Filter: should return None when predicate fails")]
    public void Filter_ShouldReturnNone_WhenPredicateFails()
    {
        Optional<int> opt = Optional<int>.Some(5);
        Optional<int> filtered = opt.Filter(x => x > 10);

        filtered.HasValue.Should().BeFalse();
    }

    [Fact(DisplayName = "Filter: on None should return None")]
    public void Filter_OnNone_ShouldReturnNone()
    {
        Optional<int> opt = Optional<int>.None;
        Optional<int> filtered = opt.Filter(x => true);

        filtered.HasValue.Should().BeFalse();
    }

    [Fact(DisplayName = "Filter: should throw ArgumentNullException when predicate is null")]
    public void Filter_ShouldThrowArgumentNullException_WhenPredicateIsNull()
    {
        Optional<int> opt = Optional<int>.Some(42);

        Action act = () => opt.Filter(null!);

        act.Should().Throw<ArgumentNullException>();
    }
    #endregion

    #region OrElse
    [Fact(DisplayName = "OrElse: should return value when present")]
    public void OrElse_ShouldReturnValue_WhenPresent()
    {
        Optional<int> opt = Optional<int>.Some(42);

        int result = opt.OrElse(-1);

        result.Should().Be(42);
    }

    [Fact(DisplayName = "OrElse: should return fallback when None")]
    public void OrElse_ShouldReturnFallback_WhenNone()
    {
        Optional<int> opt = Optional<int>.None;

        int result = opt.OrElse(-1);

        result.Should().Be(-1);
    }
    #endregion

    #region OrElseGet
    [Fact(DisplayName = "OrElseGet: should return value when present")]
    public void OrElseGet_ShouldReturnValue_WhenPresent()
    {
        Optional<int> opt = Optional<int>.Some(42);

        int result = opt.OrElseGet(() => -1);

        result.Should().Be(42);
    }

    [Fact(DisplayName = "OrElseGet: should return fallback when None")]
    public void OrElseGet_ShouldReturnFallback_WhenNone()
    {
        Optional<int> opt = Optional<int>.None;

        int result = opt.OrElseGet(() => -1);

        result.Should().Be(-1);
    }

    [Fact(DisplayName = "OrElseGet: should throw ArgumentNullException when fallback is null")]
    public void OrElseGet_ShouldThrowArgumentNullException_WhenFallbackIsNull()
    {
        Optional<int> opt = Optional<int>.Some(42);

        Action act = () => opt.OrElseGet(null!);

        act.Should().Throw<ArgumentNullException>();
    }
    #endregion

    #region OrElseThrow
    [Fact(DisplayName = "OrElseThrow: should return value when present")]
    public void OrElseThrow_ShouldReturnValue_WhenPresent()
    {
        Optional<int> opt = Optional<int>.Some(42);

        int result = opt.OrElseThrow();

        result.Should().Be(42);
    }

    [Fact(DisplayName = "OrElseThrow: should throw default exception when None")]
    public void OrElseThrow_ShouldThrowDefaultException_WhenNone()
    {
        Optional<int> opt = Optional<int>.None;

        Action act = () => opt.OrElseThrow();

        act.Should().Throw<InvalidOperationException>().WithMessage("Optional is empty.");
    }

    [Fact(DisplayName = "OrElseThrow: should throw custom exception when None")]
    public void OrElseThrow_ShouldThrowCustomException_WhenNone()
    {
        Optional<int> opt = Optional<int>.None;

        Action act = () => opt.OrElseThrow(() => new InvalidOperationException("Custom"));

        act.Should().Throw<InvalidOperationException>().WithMessage("Custom");
    }
    #endregion

    #region IfPresent
    [Fact(DisplayName = "IfPresent: should invoke action when value present")]
    public void IfPresent_ShouldInvokeAction_WhenValuePresent()
    {
        Optional<int> opt = Optional<int>.Some(42);
        int invoked = 0;

        opt.IfPresent(x => invoked = x);

        output.WriteLine("Invoked with: {0}", invoked);

        invoked.Should().Be(42);
    }

    [Fact(DisplayName = "IfPresent: should not invoke action when None")]
    public void IfPresent_ShouldNotInvokeAction_WhenNone()
    {
        Optional<int> opt = Optional<int>.None;
        bool invoked = false;

        opt.IfPresent(_ => invoked = true);

        invoked.Should().BeFalse();
    }

    [Fact(DisplayName = "IfPresent: should throw ArgumentNullException when action is null")]
    public void IfPresent_ShouldThrowArgumentNullException_WhenActionIsNull()
    {
        Optional<int> opt = Optional<int>.Some(42);

        Action act = () => opt.IfPresent(null!);

        act.Should().Throw<ArgumentNullException>();
    }
    #endregion

    #region LINQ GetEnumerator
    [Fact(DisplayName = "GetEnumerator: should yield value when present")]
    public void GetEnumerator_ShouldYieldValue_WhenPresent()
    {
        Optional<int> opt = Optional<int>.Some(42);

        List<int> items = [.. opt];

        items.Should().HaveCount(1);
        items[0].Should().Be(42);
    }

    [Fact(DisplayName = "GetEnumerator: should yield nothing when None")]
    public void GetEnumerator_ShouldYieldNothing_WhenNone()
    {
        Optional<int> opt = Optional<int>.None;

        List<int> items = [.. opt];

        items.Should().BeEmpty();
    }
    #endregion
}
