namespace Shared.Security.Authentication.Tokens.Options;

using FluentValidation;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Validator for JWT settings configuration.
/// </summary>
public sealed class JwtSettingsValidator : AbstractValidator<JwtSettings>
{
    /// <summary>
    /// The dev secret literal hardcoded in <c>appsettings.Development.json</c>. Refused in any
    /// non-Development environment to prevent booting a host with a known signing key.
    /// </summary>
    public const string DevSecretLiteral = "dev-jwt-secret-min-32-chars-for-hs256-algorithm!";

    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtSettingsValidator"/> class with a stub
    /// <see cref="IHostEnvironment"/> reporting <c>Production</c>. Use only in tests that do not
    /// exercise the dev-secret-literal rule. Production code should inject the host's
    /// <see cref="IHostEnvironment"/> via the other constructor.
    /// </summary>
    public JwtSettingsValidator() : this(new EmptyHostEnvironment())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtSettingsValidator"/> class bound to the
    /// hosting environment. Use this constructor from DI so the validator can inspect the
    /// environment name when rejecting the dev secret literal.
    /// </summary>
    public JwtSettingsValidator(IHostEnvironment environment)
    {
        _environment = environment;

        RuleFor(x => x.Secret)
            .NotEmpty()
            .WithErrorCode(JwtSettingsResult.Failure.SecretRequired.Code)
            .WithMessage(JwtSettingsResult.Failure.SecretRequired.Message)
            .MinimumLength(JwtSettingsConstant.Constraints.Secret.MinLength)
            .WithErrorCode(JwtSettingsResult.Failure.SecretTooShort.Code)
            .WithMessage(JwtSettingsResult.Failure.SecretTooShort.Message)
            .Must((settings, secret) => !IsDevSecretInNonDevelopment(secret))
            .WithErrorCode(JwtSettingsResult.Failure.DevSecretNotAllowed.Code)
            .WithMessage(JwtSettingsResult.Failure.DevSecretNotAllowed.Message);

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

    private bool IsDevSecretInNonDevelopment(string? secret) =>
        secret == DevSecretLiteral && !_environment.IsDevelopment();
}

/// <summary>
/// Minimal <see cref="IHostEnvironment"/> stub used when the validator is constructed without a
/// hosting environment (e.g., unit tests calling the parameterless constructor). Defaults to
/// <c>Production</c> so that the dev secret literal is rejected by default — the safe behaviour.
/// </summary>
internal sealed class EmptyHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = Environments.Production;
    public string ApplicationName { get; set; } = string.Empty;
    public string ContentRootPath { get; set; } = string.Empty;
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
}

