using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Shared.Operational.Storages.Security.Guard.Options;
using Shared.Performance.Caching.Wrappers;

namespace Shared.Operational.Storages.Security.Guard;

internal sealed partial class StorageAntiForgeryGuard(
    ICacheService cacheService,
    IAntiforgery antiforgery,
    IOptions<AntiForgeryOptions> options,
    ILogger<StorageAntiForgeryGuard> logger)
    : IStorageAntiForgeryGuard
{
    private static string GetCacheKey(string identityKey)
        => $"antiforgery:failures:{identityKey}";

    private static CachingEntryOption CreateEntryOptions(TimeSpan blockDuration)
        => new()
        {
            Expiration = blockDuration,
            LocalCacheExpiration = blockDuration,
        };

    public async Task<Result> ValidateRequestAsync(string identityKey, HttpContext httpContext, CancellationToken ct = default)
    {
        if (await IsBlockedAsync(identityKey, ct))
            return StorageAntiForgeryGuardResult.Failure.TooManyAttempts();

        if (!await antiforgery.IsRequestValidAsync(httpContext))
        {
            Loggers.LogAntiforgeryTokenInvalid(logger, identityKey);
            return await RecordFailureAsync(identityKey, ct);
        }

        await ResetAsync(identityKey, ct);
        return Result.Ok();
    }

    public async Task<Result> RecordFailureAsync(string identityKey, CancellationToken ct = default)
    {
        AntiForgeryOptions opts = options.Value;
        string cacheKey = GetCacheKey(identityKey);

        int failures = await cacheService.GetOrCreateAsync<int>(
            cacheKey,
            static _ => ValueTask.FromResult(0),
            CreateEntryOptions(opts.BlockDuration),
            cancellationToken: ct);

        failures++;

        await cacheService.SetAsync(
            cacheKey,
            failures,
            CreateEntryOptions(opts.BlockDuration),
            cancellationToken: ct);

        if (failures >= opts.MaxConsecutiveFailures)
        {
            Loggers.LogIdentityBlocked(logger, identityKey, failures);
            return StorageAntiForgeryGuardResult.Failure.TooManyAttempts();
        }

        Loggers.LogFailureRecorded(logger, identityKey, failures);
        return Result.Ok();
    }

    public async Task ResetAsync(string identityKey, CancellationToken ct = default)
    {
        await cacheService.RemoveAsync(GetCacheKey(identityKey), ct);
        Loggers.LogIdentityReset(logger, identityKey);
    }

    public async Task<bool> IsBlockedAsync(string identityKey, CancellationToken ct = default)
    {
        int failures = await cacheService.GetOrCreateAsync<int>(
            GetCacheKey(identityKey),
            static _ => ValueTask.FromResult(0),
            cancellationToken: ct);

        return failures >= options.Value.MaxConsecutiveFailures;
    }
}
