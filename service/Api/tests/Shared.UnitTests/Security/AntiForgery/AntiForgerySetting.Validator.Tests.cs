using FluentValidation.TestHelper;

using Shared.Security.AntiForgery.Options;

namespace Shared.UnitTests.Security.AntiForgery;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Storage")]
public sealed class AntiForgerySettingValidatorTests
{
    private readonly AntiForgerySettingValidator _sut = new();

    public static TheoryData<
        Action<AntiForgerySetting>,
        string> InvalidCases =>
        new()
        {
            {
                x => x.HeaderName = string.Empty,
                AntiForgerySettingResult.Failure.HeaderNameRequired.Code
            },
            {
                x => x.HeaderName = new string('x', 257),
                AntiForgerySettingResult.Failure.HeaderNameInvalid.Code
            }
        };

    [Fact(DisplayName = "Validate should pass for default options")]
    public void Validate_WithDefaultOptions_ShouldPass()
    {
        AntiForgerySetting options = new();

        TestValidationResult<AntiForgerySetting> result = _sut.TestValidate(options);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void Validate_WithInvalidValue_ShouldFail(
        Action<AntiForgerySetting> setup,
        string expectedErrorCode)
    {
        AntiForgerySetting options = new();
        setup(options);

        TestValidationResult<AntiForgerySetting> result = _sut.TestValidate(options);

        result.Errors.Should().Contain(e => e.ErrorCode == expectedErrorCode);
    }
}
