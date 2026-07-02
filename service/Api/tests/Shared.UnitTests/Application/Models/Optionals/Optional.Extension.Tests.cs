using System.Globalization;

using Shared.Application.Models.Optionals;

namespace Shared.UnitTests.Application.Models.Optionals;

public sealed class OptionalExtensionTests(ITestOutputHelper output)
{
    #region Apply
    [Fact(DisplayName = "Apply: on Some should invoke setter and return true")]
    public void Apply_OnSome_ShouldInvokeSetterAndReturnTrue()
    {
        Optional<int> opt = Optional<int>.Some(42);
        int captured = 0;

        bool result = opt.Apply(x => captured = x);

        output.WriteLine("Apply result: {0}, captured: {1}", result, captured);

        result.Should().BeTrue();
        captured.Should().Be(42);
    }

    [Fact(DisplayName = "Apply: on None should return false without invoking setter")]
    public void Apply_OnNone_ShouldReturnFalseWithoutInvokingSetter()
    {
        Optional<int> opt = Optional<int>.None;
        bool invoked = false;

        bool result = opt.Apply(_ => invoked = true);

        result.Should().BeFalse();
        invoked.Should().BeFalse();
    }

    [Fact(DisplayName = "Apply: should throw ArgumentNullException when setter is null")]
    public void Apply_ShouldThrowArgumentNullException_WhenSetterIsNull()
    {
        Optional<int> opt = Optional<int>.Some(42);

        Action act = () => opt.Apply(null!);

        act.Should().Throw<ArgumentNullException>();
    }
    #endregion

    #region ApplyIfChanged
    [Fact(DisplayName = "ApplyIfChanged: on Some with same value should return false")]
    public void ApplyIfChanged_OnSomeWithSameValue_ShouldReturnFalse()
    {
        Optional<int> opt = Optional<int>.Some(42);
        int captured = 0;

        bool result = opt.ApplyIfChanged(42, x => captured = x);

        output.WriteLine("ApplyIfChanged(same) result: {0}", result);

        result.Should().BeFalse();
        captured.Should().Be(0);
    }

    [Fact(DisplayName = "ApplyIfChanged: on Some with different value should invoke setter and return true")]
    public void ApplyIfChanged_OnSomeWithDifferentValue_ShouldInvokeSetterAndReturnTrue()
    {
        Optional<int> opt = Optional<int>.Some(42);
        int captured = 0;

        bool result = opt.ApplyIfChanged(99, x => captured = x);

        result.Should().BeTrue();
        captured.Should().Be(42);
    }

    [Fact(DisplayName = "ApplyIfChanged: on None should return false")]
    public void ApplyIfChanged_OnNone_ShouldReturnFalse()
    {
        Optional<int> opt = Optional<int>.None;
        bool invoked = false;

        bool result = opt.ApplyIfChanged(42, _ => invoked = true);

        result.Should().BeFalse();
        invoked.Should().BeFalse();
    }

    [Fact(DisplayName = "ApplyIfChanged: should throw ArgumentNullException when setter is null")]
    public void ApplyIfChanged_ShouldThrowArgumentNullException_WhenSetterIsNull()
    {
        Optional<int> opt = Optional<int>.Some(42);

        Action act = () => opt.ApplyIfChanged(42, null!);

        act.Should().Throw<ArgumentNullException>();
    }
    #endregion

    #region ApplyIf
    [Fact(DisplayName = "ApplyIf: on Some with passing predicate should invoke setter and return true")]
    public void ApplyIf_OnSomeWithPassingPredicate_ShouldInvokeSetterAndReturnTrue()
    {
        Optional<int> opt = Optional<int>.Some(42);
        int captured = 0;

        bool result = opt.ApplyIf(x => x > 10, x => captured = x);

        output.WriteLine("ApplyIf(passing) result: {0}, captured: {1}", result, captured);

        result.Should().BeTrue();
        captured.Should().Be(42);
    }

    [Fact(DisplayName = "ApplyIf: on Some with failing predicate should return false")]
    public void ApplyIf_OnSomeWithFailingPredicate_ShouldReturnFalse()
    {
        Optional<int> opt = Optional<int>.Some(5);
        bool invoked = false;

        bool result = opt.ApplyIf(x => x > 10, _ => invoked = true);

        result.Should().BeFalse();
        invoked.Should().BeFalse();
    }

    [Fact(DisplayName = "ApplyIf: on None should return false")]
    public void ApplyIf_OnNone_ShouldReturnFalse()
    {
        Optional<int> opt = Optional<int>.None;
        bool invoked = false;

        bool result = opt.ApplyIf(x => true, _ => invoked = true);

        result.Should().BeFalse();
        invoked.Should().BeFalse();
    }

    [Fact(DisplayName = "ApplyIf: should throw ArgumentNullException when predicate is null")]
    public void ApplyIf_ShouldThrowArgumentNullException_WhenPredicateIsNull()
    {
        Optional<int> opt = Optional<int>.Some(42);

        Action act = () => opt.ApplyIf(null!, _ => { });

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "ApplyIf: should throw ArgumentNullException when setter is null")]
    public void ApplyIf_ShouldThrowArgumentNullException_WhenSetterIsNull()
    {
        Optional<int> opt = Optional<int>.Some(42);

        Action act = () => opt.ApplyIf(x => true, null!);

        act.Should().Throw<ArgumentNullException>();
    }
    #endregion

    #region ApplyValidated
    [Fact(DisplayName = "ApplyValidated: on Some with passing validator should invoke setter and return Ok")]
    public void ApplyValidated_OnSomeWithPassingValidator_ShouldInvokeSetterAndReturnOk()
    {
        Optional<int> opt = Optional<int>.Some(42);
        int captured = 0;

        Result result = opt.ApplyValidated(
            x => x > 0 ? Result.Ok() : Result.BadRequest(),
            x => captured = x);

        output.WriteLine("ApplyValidated(passing) IsSuccess: {0}", result.IsSuccess);

        result.IsSuccess.Should().BeTrue();
        captured.Should().Be(42);
    }

    [Fact(DisplayName = "ApplyValidated: on Some with failing validator should return failure without invoking setter")]
    public void ApplyValidated_OnSomeWithFailingValidator_ShouldReturnFailureWithoutInvokingSetter()
    {
        Optional<int> opt = Optional<int>.Some(-1);
        bool invoked = false;

        Result result = opt.ApplyValidated(
            x => x > 0 ? Result.Ok() : Result.BadRequest(),
            _ => invoked = true);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.BadRequest);
        invoked.Should().BeFalse();
    }

    [Fact(DisplayName = "ApplyValidated: on None should return Ok without invoking validator or setter")]
    public void ApplyValidated_OnNone_ShouldReturnOkWithoutInvokingValidatorOrSetter()
    {
        Optional<int> opt = Optional<int>.None;
        bool validatorInvoked = false;
        bool setterInvoked = false;

        Result result = opt.ApplyValidated(
            x => { validatorInvoked = true; return Result.Ok(); },
            _ => setterInvoked = true);

        result.IsSuccess.Should().BeTrue();
        validatorInvoked.Should().BeFalse();
        setterInvoked.Should().BeFalse();
    }

    [Fact(DisplayName = "ApplyValidated: should throw ArgumentNullException when validator is null")]
    public void ApplyValidated_ShouldThrowArgumentNullException_WhenValidatorIsNull()
    {
        Optional<int> opt = Optional<int>.Some(42);

        Action act = () => opt.ApplyValidated(null!, _ => { });

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "ApplyValidated: should throw ArgumentNullException when setter is null")]
    public void ApplyValidated_ShouldThrowArgumentNullException_WhenSetterIsNull()
    {
        Optional<int> opt = Optional<int>.Some(42);

        Action act = () => opt.ApplyValidated(x => Result.Ok(), null!);

        act.Should().Throw<ArgumentNullException>();
    }
    #endregion

    #region Implicit bool operator
    [Fact(DisplayName = "Implicit bool: Some should convert to true")]
    public void ImplicitBool_Some_ShouldConvertToTrue()
    {
        Optional<int> opt = Optional<int>.Some(42);

        bool result = opt;

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "Implicit bool: None should convert to false")]
    public void ImplicitBool_None_ShouldConvertToFalse()
    {
        Optional<int> opt = Optional<int>.None;

        bool result = opt;

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Implicit bool: can be used in if statement")]
    public void ImplicitBool_CanBeUsedInIfStatement()
    {
        Optional<int> opt = Optional<int>.Some(42);
        bool entered = false;

        if (opt)
        {
            entered = true;
        }

        entered.Should().BeTrue();
    }

    [Fact(DisplayName = "Implicit bool: None should skip if block")]
    public void ImplicitBool_None_ShouldSkipIfBlock()
    {
        Optional<int> opt = Optional<int>.None;
        bool entered = false;

        if (opt)
        {
            entered = true;
        }

        entered.Should().BeFalse();
    }
    #endregion

    #region ToResult
    [Fact(DisplayName = "ToResult: on Some should return success Result with value")]
    public void ToResult_OnSome_ShouldReturnSuccessResultWithValue()
    {
        Optional<string> opt = Optional<string>.Some("hello");
        Error error = Error.BadRequest("Test.Code", "not used");

        Result<string> result = opt.ToResult(error);

        output.WriteLine("ToResult(Some) IsSuccess: {0}, Value: {1}", result.IsSuccess, result.Value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact(DisplayName = "ToResult: on None should return failure Result with error")]
    public void ToResult_OnNone_ShouldReturnFailureResultWithError()
    {
        Optional<string> opt = Optional<string>.None;
        Error error = Error.BadRequest("Missing.Value", "value was missing");

        Result<string> result = opt.ToResult(error);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.BadRequest);
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("Missing.Value");
    }
    #endregion

    #region ToResult (Func<Error>)
    [Fact(DisplayName = "ToResult with factory: on Some should not invoke factory and return success")]
    public void ToResult_WithFactory_OnSome_ShouldNotInvokeFactoryAndReturnSuccess()
    {
        Optional<int> opt = Optional<int>.Some(42);
        bool factoryInvoked = false;

        Result<int> result = opt.ToResult(() =>
        {
            factoryInvoked = true;
            return Error.BadRequest("E", "e");
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        factoryInvoked.Should().BeFalse();
    }

    [Fact(DisplayName = "ToResult with factory: on None should invoke factory and return failure")]
    public void ToResult_WithFactory_OnNone_ShouldInvokeFactoryAndReturnFailure()
    {
        Optional<int> opt = Optional<int>.None;
        bool factoryInvoked = false;

        Result<int> result = opt.ToResult(() =>
        {
            factoryInvoked = true;
            return Error.BadRequest("E", "e");
        });

        result.IsSuccess.Should().BeFalse();
        factoryInvoked.Should().BeTrue();
    }

    [Fact(DisplayName = "ToResult with factory: should throw ArgumentNullException when factory is null")]
    public void ToResult_WithFactory_ShouldThrowArgumentNullException_WhenFactoryIsNull()
    {
        Optional<int> opt = Optional<int>.Some(42);

        Action act = () => opt.ToResult((Func<Error>)null!);

        act.Should().Throw<ArgumentNullException>();
    }
    #endregion

    #region Match
    [Fact(DisplayName = "Match: on Some should invoke some branch and return result")]
    public void Match_OnSome_ShouldInvokeSomeBranchAndReturnResult()
    {
        Optional<int> opt = Optional<int>.Some(42);

        string result = opt.Match(
            x => x.ToString(CultureInfo.InvariantCulture),
            () => "none");

        output.WriteLine("Match(Some) result: {0}", result);

        result.Should().Be("42");
    }

    [Fact(DisplayName = "Match: on None should invoke none branch and return result")]
    public void Match_OnNone_ShouldInvokeNoneBranchAndReturnResult()
    {
        Optional<int> opt = Optional<int>.None;

        string result = opt.Match(
            x => x.ToString(CultureInfo.InvariantCulture),
            () => "none");

        result.Should().Be("none");
    }

    [Fact(DisplayName = "Match: should throw ArgumentNullException when some is null")]
    public void Match_ShouldThrowArgumentNullException_WhenSomeIsNull()
    {
        Optional<int> opt = Optional<int>.Some(42);

        Action act = () => opt.Match(null!, () => "none");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Match: should throw ArgumentNullException when none is null")]
    public void Match_ShouldThrowArgumentNullException_WhenNoneIsNull()
    {
        Optional<int> opt = Optional<int>.Some(42);

        Action act = () => opt.Match(x => x.ToString(CultureInfo.InvariantCulture), null!);

        act.Should().Throw<ArgumentNullException>();
    }
    #endregion

    #region SelectMany
    [Fact(DisplayName = "SelectMany: with two Some optionals should combine results")]
    public void SelectMany_TwoSome_ShouldCombineResults()
    {
        Optional<int> first = Optional<int>.Some(3);

        Optional<int> result = OptionalExtensions.SelectMany(
            first,
            x => Optional<int>.Some(x * 10),
            (x, y) => x + y);

        output.WriteLine("SelectMany result: {0}", result);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(33);
    }

    [Fact(DisplayName = "SelectMany: with first None should return None")]
    public void SelectMany_FirstNone_ShouldReturnNone()
    {
        Optional<int> first = Optional<int>.None;

        Optional<int> result = OptionalExtensions.SelectMany(
            first,
            x => Optional<int>.Some(x * 10),
            (x, y) => x + y);

        result.HasValue.Should().BeFalse();
    }

    [Fact(DisplayName = "SelectMany: with second None should return None")]
    public void SelectMany_SecondNone_ShouldReturnNone()
    {
        Optional<int> first = Optional<int>.Some(3);

        Optional<int> result = OptionalExtensions.SelectMany(
            first,
            x => Optional<int>.None,
            (x, y) => x + y);

        result.HasValue.Should().BeFalse();
    }

    [Fact(DisplayName = "SelectMany: should throw ArgumentNullException when collectionSelector is null")]
    public void SelectMany_ShouldThrowArgumentNullException_WhenCollectionSelectorIsNull()
    {
        Optional<int> opt = Optional<int>.Some(42);

        Action act = () => OptionalExtensions.SelectMany<int, int, int>(opt, null!, (x, y) => x + y);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "SelectMany: should throw ArgumentNullException when resultSelector is null")]
    public void SelectMany_ShouldThrowArgumentNullException_WhenResultSelectorIsNull()
    {
        Optional<int> opt = Optional<int>.Some(42);

        Action act = () => OptionalExtensions.SelectMany<int, int, int>(opt, x => Optional<int>.Some(x), null!);

        act.Should().Throw<ArgumentNullException>();
    }
    #endregion
}
