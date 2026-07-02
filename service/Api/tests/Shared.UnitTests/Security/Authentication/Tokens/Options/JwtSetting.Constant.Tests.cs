using Shared.Security.Authentication.Tokens.Options;

namespace Shared.UnitTests.Security.Authentication.Tokens.Options;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "JwtSettings")]
public sealed class JwtSettingsConstantTests
{
    [Fact(DisplayName = "Defaults.Algorithm should be HS256")]
    public void Defaults_Algorithm_ShouldBeHS256()
    {
        JwtSettingsConstant.Defaults.Algorithm.Should().Be("HS256");
    }

    [Fact(DisplayName = "Defaults.AccessTokenExpirationInMinutes should be 15")]
    public void Defaults_AccessTokenExpirationInMinutes_ShouldBe15()
    {
        JwtSettingsConstant.Defaults.AccessTokenExpirationInMinutes.Should().Be(15);
    }

    [Fact(DisplayName = "Defaults.RefreshTokenExpirationInDays should be 7")]
    public void Defaults_RefreshTokenExpirationInDays_ShouldBe7()
    {
        JwtSettingsConstant.Defaults.RefreshTokenExpirationInDays.Should().Be(7);
    }

    [Fact(DisplayName = "Constraints.Secret.MinLength should be 32")]
    public void Constraints_Secret_MinLength_ShouldBe32()
    {
        JwtSettingsConstant.Constraints.Secret.MinLength.Should().Be(32);
    }

    [Fact(DisplayName = "Constraints.AccessTokenExpiration.MaxMinutes should be 1440")]
    public void Constraints_AccessTokenExpiration_MaxMinutes_ShouldBe1440()
    {
        JwtSettingsConstant.Constraints.AccessTokenExpiration.MaxMinutes.Should().Be(1440);
    }

    [Fact(DisplayName = "Constraints.RefreshTokenExpiration.MaxDays should be 365")]
    public void Constraints_RefreshTokenExpiration_MaxDays_ShouldBe365()
    {
        JwtSettingsConstant.Constraints.RefreshTokenExpiration.MaxDays.Should().Be(365);
    }

    [Fact(DisplayName = "Constraints.TokenSecurity.DefaultMaxTokenAgeDays should be 30")]
    public void Constraints_TokenSecurity_DefaultMaxTokenAgeDays_ShouldBe30()
    {
        JwtSettingsConstant.Constraints.TokenSecurity.DefaultMaxTokenAgeDays.Should().Be(30);
    }

    [Fact(DisplayName = "Allowed.Algorithms should contain HS256, RS256, ES256")]
    public void Allowed_Algorithms_ShouldContainExpectedValues()
    {
        JwtSettingsConstant.Allowed.Algorithms.Should().BeEquivalentTo(["HS256", "RS256", "ES256"]);
    }

    [Fact(DisplayName = "Allowed.WeakSecrets should contain 7 known weak secrets")]
    public void Allowed_WeakSecrets_ShouldContainExpectedValues()
    {
        JwtSettingsConstant.Allowed.WeakSecrets.Should().Contain(
        [
            "SuperSecretKeyForTestingPurposesOnly123!",
            "secret",
            "123456",
            "password",
            "admin",
            "test",
            "default"
        ]);
        JwtSettingsConstant.Allowed.WeakSecrets.Should().HaveCount(7);
    }
}
