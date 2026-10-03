using Shared.Governance.Conventions;

namespace Shared.UnitTests.Governance.Conventions;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Base64")]
public class Base64ConverterTests
{
    [Fact(DisplayName = "FromBase64Url: round-trips correctly with underscore in input")]
    public void FromBase64Url_RoundTrip_WithComplexInput()
    {
        var original = "test-data_with/special+chars";
        var encoded = original.ToBase64Url();
        var decoded = encoded.FromBase64Url();

        decoded.Should().Be(original);
    }

    [Fact(DisplayName = "ToBase64Url: produces URL-safe output without +/=")]
    public void ToBase64Url_ProducesNoSpecialChars()
    {
        var result = "hello world".ToBase64Url();
        result.Should().NotContain("+");
        result.Should().NotContain("/");
        result.Should().NotContain("=");
    }

    [Fact(DisplayName = "TryFromBase64Url: returns true for valid base64url input")]
    public void TryFromBase64Url_ValidInput_ReturnsTrue()
    {
        var encoded = "test-data".ToBase64Url();
        var success = encoded.TryFromBase64Url(out var decoded);

        success.Should().BeTrue();
        decoded.Should().Be("test-data");
    }

    [Fact(DisplayName = "TryFromBase64Url: returns false for empty input")]
    public void TryFromBase64Url_EmptyInput_ReturnsFalse()
    {
        var success = "".TryFromBase64Url(out var decoded);

        success.Should().BeFalse();
        decoded.Should().BeEmpty();
    }

    [Fact(DisplayName = "TryFromBase64Url: returns false for invalid base64")]
    public void TryFromBase64Url_InvalidInput_ReturnsFalse()
    {
        var success = "!!!not-base64!!!".TryFromBase64Url(out var decoded);

        success.Should().BeFalse();
        decoded.Should().BeEmpty();
    }

    [Fact(DisplayName = "ToBase64Url: null input throws ArgumentNullException")]
    public void ToBase64Url_NullInput_Throws()
    {
        string? input = null;
        var act = () => input!.ToBase64Url();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "FromBase64Url: null input throws ArgumentNullException")]
    public void FromBase64Url_NullInput_Throws()
    {
        string? input = null;
        var act = () => input!.FromBase64Url();
        act.Should().Throw<ArgumentNullException>();
    }
}
