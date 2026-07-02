using FluentValidation.TestHelper;

using Shared.Security.Authorization.Options;

namespace Shared.UnitTests.Security.Authorization.Options;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "AuthorizationOptions")]
public sealed class AuthorizationOptionsValidatorTests
{
    private readonly AuthzSettingValidator _sut = new();

    [Fact(DisplayName = "AuthorizationOptionsValidator: Should fail when PermissionCache is null")]
    public void ShouldFail_WhenPermissionCacheIsNull()
    {
        AuthzSetting setting = new()
        {
            PermissionCache = null!
        };

        TestValidationResult<AuthzSetting> result = _sut.TestValidate(setting);

        result.ShouldHaveValidationErrorFor(x => x.PermissionCache);
    }

    [Fact(DisplayName = "AuthorizationOptionsValidator: Should fail when SlidingExpiration is negative")]
    public void ShouldFail_WhenSlidingExpirationIsNegative()
    {
        AuthzSetting setting = new()
        {
            PermissionCache = new PermissionCacheOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(-1),
                AbsoluteExpiration = TimeSpan.FromMinutes(30),
            }
        };

        TestValidationResult<AuthzSetting> result = _sut.TestValidate(setting);

        result.ShouldHaveValidationErrorFor(x => x.PermissionCache.SlidingExpiration);
    }

    [Fact(DisplayName = "AuthorizationOptionsValidator: Should fail when AbsoluteExpiration is negative")]
    public void ShouldFail_WhenAbsoluteExpirationIsNegative()
    {
        AuthzSetting setting = new()
        {
            PermissionCache = new PermissionCacheOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(5),
                AbsoluteExpiration = TimeSpan.FromMinutes(-1),
            }
        };

        TestValidationResult<AuthzSetting> result = _sut.TestValidate(setting);

        result.ShouldHaveValidationErrorFor(x => x.PermissionCache.AbsoluteExpiration);
    }

    [Fact(DisplayName = "AuthorizationOptionsValidator: Should pass when configuration is valid")]
    public void ShouldPass_WhenConfigurationIsValid()
    {
        AuthzSetting setting = new()
        {
            PermissionCache = new PermissionCacheOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(5),
                AbsoluteExpiration = TimeSpan.FromMinutes(30),
            }
        };

        TestValidationResult<AuthzSetting> result = _sut.TestValidate(setting);

        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "AuthorizationOptionsValidator: Should pass when expirations are zero")]
    public void ShouldPass_WhenExpirationsAreZero()
    {
        AuthzSetting setting = new()
        {
            PermissionCache = new PermissionCacheOptions
            {
                SlidingExpiration = TimeSpan.Zero,
                AbsoluteExpiration = TimeSpan.Zero,
            }
        };

        TestValidationResult<AuthzSetting> result = _sut.TestValidate(setting);

        result.IsValid.Should().BeTrue();
    }
}
