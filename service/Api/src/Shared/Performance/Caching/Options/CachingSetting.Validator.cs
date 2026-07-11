using FluentValidation;

using Microsoft.Extensions.Configuration;

using Shared.Performance.Caching.Options.Distributed;
using Shared.Performance.Caching.Options.Hybrid;
using Shared.Performance.Caching.Options.InMemory;

namespace Shared.Performance.Caching.Options;

/// <summary>
/// Validates the <see cref="CachingSetting"/> configuration section.
/// Includes validation for sub‑options and connection string requirements.
/// </summary>
public sealed class CachingSettingValidator : AbstractValidator<CachingSetting>
{
    private readonly IConfiguration _configuration;

    public CachingSettingValidator(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        // Validate Memory options
        RuleFor(x => x.Memory)
            .NotNull()
            .WithErrorCode(CachingSettingResult.Failure.MemoryRequired.Code)
            .WithMessage(CachingSettingResult.Failure.MemoryRequired.Message)
            .SetValidator(new MemoryCacheSettingValidator());

        // Validate Distributed options
        RuleFor(x => x.Distributed)
            .NotNull()
            .WithErrorCode(CachingSettingResult.Failure.DistributedRequired.Code)
            .WithMessage(CachingSettingResult.Failure.DistributedRequired.Message)
            .SetValidator(new DistributedCacheSettingValidator());

        // Validate Hybrid options
        RuleFor(x => x.Hybrid)
            .NotNull()
            .WithErrorCode(CachingSettingResult.Failure.HybridRequired.Code)
            .WithMessage(CachingSettingResult.Failure.HybridRequired.Message)
            .SetValidator(new HybridCacheSettingValidator());

        // Custom validation for connection string when distributed cache is required
        RuleFor(x => x)
            .Custom((Action<CachingSetting, ValidationContext<CachingSetting>>)((options, context) =>
            {
                if (options.Distributed is null || !options.Distributed.Required)
                    return;

                (string? name, string? connectionString) = ResolveConnectionString();

                if (string.IsNullOrEmpty(connectionString))
                {
                    Error error = CachingSettingResult.Failure.ConnectionStringMissing(name);
                    context.AddFailure(error.Code, (string)error.Message);
                }
            }));
    }

    private (string Name, string? Value) ResolveConnectionString()
    {
        // Prefer Aspire-managed connection string
        var aspire = _configuration.GetConnectionString(CachingSettingConstant.Aspire);
        if (!string.IsNullOrEmpty(aspire))
            return (CachingSettingConstant.Aspire, aspire);

        // Fall back to standalone connection string
        var @default = _configuration.GetConnectionString(CachingSettingConstant.Default);
        if (!string.IsNullOrEmpty(@default))
            return (CachingSettingConstant.Default, @default);

        // No connection string found – return the preferred key name for error reporting
        return (CachingSettingConstant.Aspire, null);
    }
}