using System.Diagnostics;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Shared.Operational.Storages.Helpers;
using Shared.Operational.Storages.Models;
using Shared.Operational.Storages.Processing;
using Shared.Operational.Storages.Providers;
using Shared.Operational.Storages.Security;
using Shared.Operational.Storages.Security.Guard;
using Shared.Operational.Storages.Security.Options;
using Shared.Operational.Storages.Security.Scanners;

namespace Shared.Operational.Storages.Services;

/// <summary>
/// Routes storage operations to the registered <see cref="IStorageProvider"/> by name,
/// applying security enforcement, anti-forgery protection, and an optional pre-processing
/// pipeline (hash → malware scan → image processing → encryption) before each upload.
/// </summary>
internal sealed partial class StorageService(
    IReadOnlyDictionary<string, IStorageProvider> providers,
    string defaultProviderName,
    IStorageSecurityEnforcer enforcer,
    IStorageAntiForgeryGuard antiforgeryGuard,
    IHttpContextAccessor httpContextAccessor,
    ILogger<StorageService> logger,
    IStorageMalwareScanner? malwareScanner = null,
    IImageProcessor? imageProcessor = null,
    IOptions<StorageSecuritySetting>? storageSecurityOptions = null)
    : IStorageService
{
    /// <inheritdoc />
    public async Task<Result<UploadResult>> UploadAsync(
        UploadRequest request,
        string? providerName = null,
        UploadOptions? options = null,
        CancellationToken ct = default)
    {
        if (!TryResolve(providerName, out IStorageProvider provider))
            return StorageResult.Failure.ProviderNotFound(providerName ?? defaultProviderName);

        // Check: Validate anti-forgery CSRF token and rate-limit consecutive failures.
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            string identityKey = ExtractIdentityKey(httpContext);
            Result guardResult = await antiforgeryGuard.ValidateRequestAsync(identityKey, httpContext, ct);
            if (!guardResult.IsSuccess)
            {
                Loggers.LogUploadBlocked(logger, request.Key, FirstErrorCode(guardResult.Errors) ?? "antiforgery failure");
                return guardResult.Errors;
            }
        }

        // Validate: Run security rules — extension allowlist, size cap, magic bytes.
        Result securityResult = await enforcer.EnforceAsync(request, ct);
        if (!securityResult.IsSuccess)
        {
            Loggers.LogSecurityCheckFailed(logger, request.Key, FirstErrorCode(securityResult.Errors));
            return securityResult.Errors;
        }

        // Merge options: method-level overrides request-level.
        UploadOptions effectiveOptions = options ?? request.Options ?? new();

        // Track metadata additions from pipeline steps.
        Dictionary<string, string>? pipelineMetadata = null;

        // Pipeline: Hash (on original content, before any transforms).
        Stream content = request.Content;
        if (effectiveOptions.GenerateHash)
        {
            if (content.CanSeek)
                content.Position = 0;

            try
            {
                string hash = await HashHelper.ComputeHashAsync(content, ct);
                pipelineMetadata ??= [];
                pipelineMetadata["content-hash"] = hash;
                Loggers.LogHashComputed(logger, request.Key, hash);
            }
            catch (Exception ex)
            {
                Loggers.LogHashFailed(logger, request.Key, ex.Message);
                return StorageResult.Failure.HashFailed(ex.Message);
            }
        }

        // Pipeline: Malware scan.
        if (effectiveOptions.ScanForMalware && malwareScanner is not null)
        {
            if (content.CanSeek)
                content.Position = 0;

            Result<MalwareScanResult> scanResult = await malwareScanner.ScanAsync(content, request.Key, effectiveOptions, ct);
            if (!scanResult.IsSuccess)
            {
                Loggers.LogMalwareScanFailed(logger, request.Key, FirstErrorCode(scanResult.Errors));
                return scanResult.Errors;
            }

            if (!scanResult.Value.IsClean)
            {
                string threat = scanResult.Value.ThreatName ?? "unknown";

                if (effectiveOptions.OnMalwareDetected == InfectionAction.Reject)
                {
                    Loggers.LogMalwareRejected(logger, request.Key, threat);
                    return StorageResult.Failure.MalwareRejected(threat);
                }

                if (effectiveOptions.OnMalwareDetected == InfectionAction.Quarantine)
                {
                    Loggers.LogMalwareQuarantined(logger, request.Key, threat);
                    pipelineMetadata ??= [];
                    pipelineMetadata["quarantine-threat"] = threat;
                    pipelineMetadata["quarantine-timestamp"] = DateTimeOffset.UtcNow.ToString("O");
                }

                if (effectiveOptions.OnMalwareDetected == InfectionAction.AllowWithWarning)
                {
                    Loggers.LogMalwareWarning(logger, request.Key, threat);
                    pipelineMetadata ??= [];
                    pipelineMetadata["malware-threat"] = threat;
                }
            }
        }

        // Pipeline: Image processing.
        if (imageProcessor is not null && (effectiveOptions.ResizeWidth is not null || effectiveOptions.ResizeHeight is not null || effectiveOptions.OutputFormat is not null))
        {
            if (content.CanSeek)
                content.Position = 0;

            Result<Stream> processResult = await imageProcessor.ProcessAsync(content, effectiveOptions, ct);
            if (!processResult.IsSuccess)
            {
                Loggers.LogImageProcessingFailed(logger, request.Key, FirstErrorCode(processResult.Errors));
                return processResult.Errors;
            }

            content = processResult.Value;
            Loggers.LogImageProcessingCompleted(logger, request.Key);
        }

        // Pipeline: Encrypt.
        if (effectiveOptions.Encrypt)
        {
            string? encryptionKey = storageSecurityOptions?.Value?.EncryptionKey;
            if (string.IsNullOrEmpty(encryptionKey))
            {
                Loggers.LogEncryptionSkipped(logger, request.Key);
            }
            else
            {
                if (content.CanSeek)
                    content.Position = 0;

                try
                {
                    byte[] key = Encoding.UTF8.GetBytes(encryptionKey);
                    content = await EncryptionHelper.EncryptAsync(content, key, ct);
                    pipelineMetadata ??= [];
                    pipelineMetadata["encrypted"] = "true";
                    Loggers.LogEncryptionApplied(logger, request.Key);
                }
                catch (Exception ex)
                {
                    Loggers.LogEncryptionFailed(logger, request.Key, ex.Message);
                    return StorageResult.Failure.EncryptionFailed(ex.Message);
                }
            }
        }

        // Overwrite flag: pass via metadata.
        if (effectiveOptions.Overwrite)
        {
            pipelineMetadata ??= [];
            pipelineMetadata["overwrite-existing"] = "true";
        }

        // Merge pipeline metadata into request metadata.
        IReadOnlyDictionary<string, string>? mergedMetadata = request.Metadata;
        if (pipelineMetadata is not null)
        {
            var combined = new Dictionary<string, string>(request.Metadata ?? new Dictionary<string, string>());
            foreach (var kvp in pipelineMetadata)
                combined[kvp.Key] = kvp.Value;
            mergedMetadata = combined;
        }

        // Rebuild request with processed content and merged metadata.
        UploadRequest uploadRequest = request with { Content = content, Metadata = mergedMetadata };

        // Upload: Delegate to the resolved provider and record timing.
        Stopwatch sw = Stopwatch.StartNew();
        Result<UploadResult> result = await provider.UploadAsync(uploadRequest, ct);
        sw.Stop();

        if (result.IsSuccess)
        {
            Loggers.LogUploadSuccess(logger, provider.Name, request.Key, sw.ElapsedMilliseconds);
        }
        else
        {
            Loggers.LogUploadFailure(logger, provider.Name, request.Key, sw.ElapsedMilliseconds, FirstErrorCode(result.Errors));
        }

        return result;
    }

    /// <inheritdoc />
    public Task<Result<DownloadResult>> DownloadAsync(
        string key,
        string? providerName = null,
        CancellationToken ct = default)
        => TryResolve(providerName, out IStorageProvider provider)
            ? RunTimed(provider.Name, nameof(DownloadAsync), key, () => provider.DownloadAsync(key, ct))
            : Task.FromResult<Result<DownloadResult>>(StorageResult.Failure.ProviderNotFound(providerName ?? defaultProviderName));

    /// <inheritdoc />
    public Task<Result> DeleteAsync(
        string key,
        string? providerName = null,
        CancellationToken ct = default)
        => TryResolve(providerName, out IStorageProvider provider)
            ? RunTimed(provider.Name, nameof(DeleteAsync), key, () => provider.DeleteAsync(key, ct))
            : Task.FromResult<Result>(StorageResult.Failure.ProviderNotFound(providerName ?? defaultProviderName));

    /// <inheritdoc />
    public Task<Result<StoredObjectInfo>> StatAsync(
        string key,
        string? providerName = null,
        CancellationToken ct = default)
        => TryResolve(providerName, out IStorageProvider provider)
            ? RunTimed(provider.Name, nameof(StatAsync), key, () => provider.StatAsync(key, ct))
            : Task.FromResult<Result<StoredObjectInfo>>(StorageResult.Failure.ProviderNotFound(providerName ?? defaultProviderName));

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<StoredObjectInfo>>> ListAsync(
        string? prefix = null,
        string? providerName = null,
        CancellationToken ct = default)
        => TryResolve(providerName, out IStorageProvider provider)
            ? RunTimed(provider.Name, nameof(ListAsync), prefix ?? "(all)", () => provider.ListAsync(prefix, ct))
            : Task.FromResult<Result<IReadOnlyList<StoredObjectInfo>>>(StorageResult.Failure.ProviderNotFound(providerName ?? defaultProviderName));

    // ── Helpers ─────────────────────────────────────────────────────────────

    private bool TryResolve(string? name, out IStorageProvider provider)
    {
        string key = string.IsNullOrWhiteSpace(name) ? defaultProviderName : name;
        if (providers.TryGetValue(key, out provider!))
            return true;
        Loggers.LogProviderNotFound(logger, key);
        return false;
    }

    private static string? FirstErrorCode(List<Error> errors)
        => errors.Count > 0 ? errors[0].Code : null;

    private static string ExtractIdentityKey(HttpContext httpContext)
        => httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? httpContext.Connection.RemoteIpAddress?.ToString()
           ?? "anonymous";

    private async Task<Result<T>> RunTimed<T>(
        string providerName, string operation, string key, Func<Task<Result<T>>> action)
    {
        Stopwatch sw = Stopwatch.StartNew();
        Result<T> result = await action();
        sw.Stop();
        if (result.IsSuccess)
        {
            Loggers.LogOperationSuccess(logger, providerName, operation, key, sw.ElapsedMilliseconds);
        }
        else
        {
            Loggers.LogOperationFailure(logger, providerName, operation, key, sw.ElapsedMilliseconds, FirstErrorCode(result.Errors));
        }
        return result;
    }

    private async Task<Result> RunTimed(
        string providerName, string operation, string key, Func<Task<Result>> action)
    {
        Stopwatch sw = Stopwatch.StartNew();
        Result result = await action();
        sw.Stop();
        if (result.IsSuccess)
        {
            Loggers.LogOperationSuccess(logger, providerName, operation, key, sw.ElapsedMilliseconds);
        }
        else
        {
            Loggers.LogOperationFailure(logger, providerName, operation, key, sw.ElapsedMilliseconds, FirstErrorCode(result.Errors));
        }
        return result;
    }
}
