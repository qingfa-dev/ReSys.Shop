using FluentValidation.TestHelper;

using Microsoft.Extensions.Hosting;

using Shared.Security.Authentication.Tokens.Options;

namespace Module.UnitTests.Identity;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
public class JwtSettingsValidatorTests
{
    private const string DevSecret = "dev-jwt-secret-min-32-chars-for-hs256-algorithm!";

    private static JwtSettingsValidator CreateValidator(string environmentName = "Production")
    {
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns(environmentName);
        return new JwtSettingsValidator(environment.Object);
    }

    private static JwtSettings CreateValidSettings()
    {
        return new JwtSettings
        {
            Secret = "real-32-character-or-longer-secret-here",
            Issuer = "ReSys.Shop",
            Audience = "ReSys.Shop",
            AccessTokenExpirationInMinutes = 15,
            RefreshTokenExpirationInDays = 7,
            Algorithm = "HS256"
        };
    }

    [Fact(DisplayName = "Validator: rejects dev secret literal in Production")]
    public void Production_DevSecret_Fails()
    {
        var settings = CreateValidSettings();
        settings.Secret = DevSecret;

        var validator = CreateValidator("Production");
        var result = validator.TestValidate(settings);

        result.ShouldHaveValidationErrorFor(s => s.Secret);
    }

    [Fact(DisplayName = "Validator: accepts dev secret literal in Development")]
    public void Development_DevSecret_Allowed()
    {
        var settings = CreateValidSettings();
        settings.Secret = DevSecret;

        var validator = CreateValidator("Development");
        var result = validator.TestValidate(settings);

        result.ShouldNotHaveValidationErrorFor(s => s.Secret);
    }

    [Fact(DisplayName = "Validator: rejects empty secret")]
    public void Empty_Fails()
    {
        var settings = CreateValidSettings();
        settings.Secret = string.Empty;

        var validator = CreateValidator("Production");
        var result = validator.TestValidate(settings);

        result.ShouldHaveValidationErrorFor(s => s.Secret);
    }
}
