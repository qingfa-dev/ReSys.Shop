using System.Text.RegularExpressions;

namespace Shared.UnitTests.Application.Models.Errors;

public sealed class ErrorConstantTests(ITestOutputHelper output)
{
    [Fact(DisplayName = "Constraints: should have expected limits")]
    public void Constraints_ShouldHaveExpectedLimits()
    {
        ErrorConstant.Constraints.MaxCodeLength.Should().Be(256);
        ErrorConstant.Constraints.MaxMessageLength.Should().Be(2048);

        output.WriteLine("MaxCodeLength={0}, MaxMessageLength={1}",
            ErrorConstant.Constraints.MaxCodeLength,
            ErrorConstant.Constraints.MaxMessageLength);
    }

    [Fact(DisplayName = "DefaultValues: should have expected defaults")]
    public void DefaultValues_ShouldHaveExpectedDefaults()
    {
        ErrorConstant.DefaultValues.Code.Should().Be("General.Unexpected");
        ErrorConstant.DefaultValues.Message.Should().Be("An unexpected error occurred.");
        ErrorConstant.DefaultValues.Type.Should().Be(ErrorType.Unexpected);
        ErrorConstant.DefaultValues.Type.Should().Be(ResultConstant.StatusCodes.InternalServerError);

        output.WriteLine("Default Type={0}", ErrorConstant.DefaultValues.Type);
    }

    [Fact(DisplayName = "Patterns: valid codes should match regex")]
    public void Patterns_Code_ShouldMatchValidCodes()
    {
        var regex = new Regex(ErrorConstant.Patterns.Code);

        regex.IsMatch("Auth.InvalidToken").Should().BeTrue();
        regex.IsMatch("A.B").Should().BeTrue();
        regex.IsMatch("Validation.Field.Required").Should().BeTrue();
    }

    [Fact(DisplayName = "Patterns: invalid codes should not match regex")]
    public void Patterns_Code_ShouldNotMatchInvalidCodes()
    {
        var regex = new Regex(ErrorConstant.Patterns.Code);

        regex.IsMatch("invalid").Should().BeFalse();
        regex.IsMatch("no-dots").Should().BeFalse();
        regex.IsMatch(".StartsWithDot").Should().BeFalse();
        regex.IsMatch("EndsWithDot.").Should().BeFalse();
        regex.IsMatch("").Should().BeFalse();
    }

    [Fact(DisplayName = "Metadata: should have expected keys")]
    public void Metadata_ShouldHaveExpectedKeys()
    {
        ErrorConstant.Metadata.Field.Should().Be("Field");
        ErrorConstant.Metadata.Resource.Should().Be("Resource");
        ErrorConstant.Metadata.ResourceId.Should().Be("ResourceId");
        ErrorConstant.Metadata.AttemptedValue.Should().Be("AttemptedValue");
    }
}
