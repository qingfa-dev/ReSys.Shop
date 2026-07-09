namespace Shared.UnitTests.Application.Models.Results;

[Trait("Category", "Unit")]
[Trait("Module", "Results")]
[Trait("Feature", "EdgeCases")]
public sealed class ValueResultEdgeCaseTests
{
    #region Value Access Guard

    [Fact(DisplayName = "Value getter returns default on failure")]
    public void ValueGetter_OnFailure_ShouldReturnDefault()
    {
        var result = Result<int>.NotFound("missing");

        result.Value.Should().Be(default);
    }

    [Fact(DisplayName = "Value getter returns null on failure with errors")]
    public void ValueGetter_OnFailureWithErrors_ShouldReturnNull()
    {
        var result = Result<string>.BadRequest("bad", [Error.BadRequest("V.E", "e")]);

        result.Value.Should().BeNull();
    }

    [Fact(DisplayName = "Value getter succeeds on success")]
    public void ValueGetter_OnSuccess_ShouldReturnValue()
    {
        var result = Result<int>.Ok(42);

        result.Value.Should().Be(42);
    }

    #endregion

    #region Default Values

    [Fact(DisplayName = "Default(T) on success with no value should be default")]
    public void DefaultValue_OnSuccessWithoutValue_ShouldBeDefault()
    {
        var result = Result<int>.Create(value: 0);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact(DisplayName = "Reference type null on success should be null")]
    public void NullReferenceValue_OnSuccess_ShouldBeNull()
    {
        string? nullString = null;
        var result = Result<string?>.Ok(nullString);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    #endregion

    #region Implicit Operators

    [Fact(DisplayName = "Implicit: int value -> Result<int>")]
    public void Implicit_IntToResult_ShouldReturnOk()
    {
        Result<int> result = 99;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    [Fact(DisplayName = "Implicit: string value -> Result<string>")]
    public void Implicit_StringToResult_ShouldReturnOk()
    {
        Result<string> result = "auto";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("auto");
    }

    [Fact(DisplayName = "Implicit: null string -> Result<string?> should be Ok")]
    public void Implicit_NullStringToResult_ShouldReturnOk()
    {
        string? nullValue = null;
        Result<string?> result = nullValue;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact(DisplayName = "Implicit: Error -> Result<T> should return failure with default value")]
    public void Implicit_ErrorToResult_ShouldReturnFailureWithDefault()
    {
        Result<int> result = Error.NotFound("R.Missing", "missing");

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.NotFound);
    }

    #endregion

    #region Metadata

    [Fact(DisplayName = "Empty metadata should be null")]
    public void EmptyMetadata_ShouldBeNull()
    {
        var result = Result<int>.Ok(1, metadata: []);

        result.Metadata.Should().BeNull();
    }

    [Fact(DisplayName = "Metadata when not provided should be null")]
    public void Metadata_WhenNotProvided_ShouldBeNull()
    {
        var result = Result<int>.Ok(1);

        result.Metadata.Should().BeNull();
    }

    #endregion


}
