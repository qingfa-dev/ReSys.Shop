namespace Shared.Security.Authentication.Tokens.Options;

using FluentValidation;

/// <summary>
/// Validator for JWT settings configuration.
/// </summary>
public sealed class JwtSettingsValidator : AbstractValidator<JwtSettings>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtSettingsValidator"/> class.
    /// </summary>
    public JwtSettingsValidator()
    {
        RuleFor(x => x.Secret)
            .NotEmpty()
            .WithErrorCode(JwtSettingsResult.Failure.SecretRequired.Code)
            .WithMessage(JwtSettingsResult.Failure.SecretRequired.Message)
            .MinimumLength(JwtSettingsConstant.Constraints.Secret.MinLength)
            .WithErrorCode(JwtSettingsResult.Failure.SecretTooShort.Code)
            .WithMessage(JwtSettingsResult.Failure.SecretTooShort.Message);

        RuleFor(x => x.Issuer)
            .NotEmpty()
            .WithErrorCode(JwtSettingsResult.Failure.IssuerRequired.Code)
            .WithMessage(JwtSettingsResult.Failure.IssuerRequired.Message);

        RuleFor(x => x.Audience)
            .NotEmpty()
            .WithErrorCode(JwtSettingsResult.Failure.AudienceRequired.Code)
            .WithMessage(JwtSettingsResult.Failure.AudienceRequired.Message);

        RuleFor(x => x.Algorithm)
            .NotEmpty()
            .WithErrorCode(JwtSettingsResult.Failure.AlgorithmRequired.Code)
            .WithMessage(JwtSettingsResult.Failure.AlgorithmRequired.Message)
            .Must(alg => !string.Equals(alg, "none", StringComparison.OrdinalIgnoreCase))
            .WithErrorCode(JwtSettingsResult.Failure.AlgorithmNoneNotAllowed.Code)
            .WithMessage(JwtSettingsResult.Failure.AlgorithmNoneNotAllowed.Message)
            .Must(alg => JwtSettingsConstant.Allowed.Algorithms.Contains(alg, StringComparer.OrdinalIgnoreCase))
            .WithErrorCode(JwtSettingsResult.Failure.AlgorithmNotAllowed.Code)
            .WithMessage(JwtSettingsResult.Failure.AlgorithmNotAllowed.Message);

        RuleFor(x => x.AccessTokenExpirationInMinutes)
            .GreaterThan(0)
            .WithErrorCode(JwtSettingsResult.Failure.AccessTokenExpirationInvalid.Code)
            .WithMessage(JwtSettingsResult.Failure.AccessTokenExpirationInvalid.Message)
            .LessThanOrEqualTo(JwtSettingsConstant.Constraints.AccessTokenExpiration.MaxMinutes)
            .WithErrorCode(JwtSettingsResult.Failure.AccessTokenExpirationExceeded.Code)
            .WithMessage(JwtSettingsResult.Failure.AccessTokenExpirationExceeded.Message);

        RuleFor(x => x.RefreshTokenExpirationInDays)
            .GreaterThan(0)
            .WithErrorCode(JwtSettingsResult.Failure.RefreshTokenExpirationInvalid.Code)
            .WithMessage(JwtSettingsResult.Failure.RefreshTokenExpirationInvalid.Message)
            .LessThanOrEqualTo(JwtSettingsConstant.Constraints.RefreshTokenExpiration.MaxDays)
            .WithErrorCode(JwtSettingsResult.Failure.RefreshTokenExpirationExceeded.Code)
            .WithMessage(JwtSettingsResult.Failure.RefreshTokenExpirationExceeded.Message);

        RuleFor(x => x.TokenSecurity.MaxTokenAgeDays)
            .GreaterThan(0)
            .When(x => x.TokenSecurity.ReuseDetectionEnabled)
            .WithErrorCode(JwtSettingsResult.Failure.MaxTokenAgeRequired.Code)
            .WithMessage(JwtSettingsResult.Failure.MaxTokenAgeRequired.Message);
    }
}

