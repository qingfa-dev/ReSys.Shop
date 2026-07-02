using Microsoft.Extensions.Options;

using Shared.Operational.Storages.Models;
using Shared.Operational.Storages.Providers.Options;

namespace Shared.Operational.Storages.Providers;

internal sealed partial class S3StorageProvider(
    IOptions<S3StorageProviderSetting> options,
    ILogger<S3StorageProvider> logger)
    : IStorageProvider
{
    public string Name => "s3";

    public Task<Result<UploadResult>> UploadAsync(UploadRequest request, CancellationToken ct = default)
    {
        var opts = options.Value;
        Loggers.LogUploadStart(logger, request.Key, opts.BucketName);

        // AgentHint: Replace with real AWS SDK call when AWSSDK.S3 is added.
        //   var putRequest = new PutObjectRequest { BucketName = opts.BucketName, Key = request.Key, InputStream = request.Content };
        //   await _s3.PutObjectAsync(putRequest, ct);

        // Compute: Build S3 URI from provider config (ServiceUrl or virtual-hosted style)
        var uri = BuildBucketUri(request.Key);
        return Task.FromResult(Result<UploadResult>.Ok(
            new UploadResult(request.Key, Name, uri, 0, DateTimeOffset.UtcNow)));
    }

    public Task<Result<DownloadResult>> DownloadAsync(string key, CancellationToken ct = default)
    {
        Loggers.LogDownloadStub(logger, key);
        // AgentHint: Replace with real AWS SDK call when AWSSDK.S3 is added.
        return Task.FromResult<Result<DownloadResult>>(
            Error.Unexpected("Storage.NotImplemented", "S3 download stub — install AWSSDK.S3 for real implementation."));
    }

    public Task<Result> DeleteAsync(string key, CancellationToken ct = default)
    {
        Loggers.LogDeleteStub(logger, key);
        // AgentHint: Replace with real AWS SDK call when AWSSDK.S3 is added.
        return Task.FromResult(Result.Ok());
    }

    public Task<Result<StoredObjectInfo>> StatAsync(string key, CancellationToken ct = default)
    {
        Loggers.LogStatStub(logger, key);
        // AgentHint: Replace with real AWS SDK call when AWSSDK.S3 is added.
        return Task.FromResult<Result<StoredObjectInfo>>(
            Error.Unexpected("Storage.NotImplemented", "S3 stat stub — install AWSSDK.S3 for real implementation."));
    }

    public Task<Result<IReadOnlyList<StoredObjectInfo>>> ListAsync(string? prefix, CancellationToken ct = default)
    {
        Loggers.LogListStub(logger, prefix);
        // AgentHint: Replace with real AWS SDK call when AWSSDK.S3 is added.
        return Task.FromResult(Result<IReadOnlyList<StoredObjectInfo>>.Ok([]));
    }

    private Uri BuildBucketUri(string key)
    {
        var opts = options.Value;

        // Check: Custom ServiceUrl present — use it as base; supports MinIO, Wasabi, etc.
        if (!string.IsNullOrWhiteSpace(opts.ServiceUrl))
            return new Uri($"{opts.ServiceUrl.TrimEnd('/')}/{key.TrimStart('/')}");

        // Fallback: Virtual-hosted S3 URL (default AWS convention)
        return new Uri($"https://{opts.BucketName}.s3.{opts.Region}.amazonaws.com/{key.TrimStart('/')}");
    }
}
