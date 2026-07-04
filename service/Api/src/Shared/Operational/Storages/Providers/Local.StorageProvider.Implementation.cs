using Microsoft.Extensions.Options;

using Shared.Operational.Storages.Models;
using Shared.Operational.Storages.Providers.Options;

namespace Shared.Operational.Storages.Providers;

internal sealed partial class LocalStorageProvider(
    IOptions<LocalStorageProviderSetting> options,
    ILogger<LocalStorageProvider> logger)
    : IStorageProvider
{
    public string Name => "local";

    public async Task<Result<UploadResult>> UploadAsync(UploadRequest request, CancellationToken ct = default)
    {
        // Guard: Honour caller cancellation before initiating any I/O
        ct.ThrowIfCancellationRequested();

        // Guard: Resolve and validate key — rejects path traversal
        var pathResult = ResolvePath(request.Key);
        if (!pathResult.IsSuccess)
            return pathResult.Errors;
        var fullPath = pathResult.Value;

        // Create: Ensure target directory exists before opening stream
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        try
        {
            var bufferSize = options.Value.BufferSize;
            await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, useAsync: true);
            await request.Content.CopyToAsync(fileStream, bufferSize, ct);
            await fileStream.FlushAsync(ct);

            var fileInfo = new FileInfo(fullPath);
            var size = fileInfo.Length;
            Loggers.LogUploadSuccess(logger, request.Key, size);

            // Log: Upload size — provider has no public URI, so Uri is null
            return Result<UploadResult>.Ok(new UploadResult(
                request.Key, Name, null, size, DateTimeOffset.UtcNow));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Catch: File I/O error during write — surface as provider error
            Loggers.LogUploadFailed(logger, request.Key, ex);
            return StorageResult.Failure.ProviderError(ex.Message);
        }
    }

    public Task<Result<DownloadResult>> DownloadAsync(string key, CancellationToken ct = default)
    {
        // Guard: Honour caller cancellation before initiating any I/O
        ct.ThrowIfCancellationRequested();

        // Guard: Resolve and validate key — rejects path traversal
        var pathResult = ResolvePath(key);
        if (!pathResult.IsSuccess)
            return Task.FromResult<Result<DownloadResult>>(pathResult.Errors);
        var fullPath = pathResult.Value;

        // Check: File exists before opening stream
        if (!File.Exists(fullPath))
            return Task.FromResult<Result<DownloadResult>>(StorageResult.Failure.NotFound(key));

        try
        {
            // Acquire: Open shared-read stream — caller owns disposal
            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, options.Value.BufferSize, useAsync: true);
            var info = new FileInfo(fullPath);
            // Create: StoredObjectInfo with ContentType=null — local FS lacks MIME metadata
            var meta = new StoredObjectInfo(key, Name, info.Length, info.LastWriteTimeUtc, ContentType: null);
            return Task.FromResult(Result<DownloadResult>.Ok(new DownloadResult(stream, meta)));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Catch: File I/O error during read — surface as provider error
            Loggers.LogDownloadFailed(logger, key, ex);
            return Task.FromResult<Result<DownloadResult>>(StorageResult.Failure.ProviderError(ex.Message));
        }
    }

    public Task<Result> DeleteAsync(string key, CancellationToken ct = default)
    {
        // Guard: Honour caller cancellation before initiating any I/O
        ct.ThrowIfCancellationRequested();

        // Guard: Resolve and validate key — rejects path traversal
        var pathResult = ResolvePath(key);
        if (!pathResult.IsSuccess)
            return Task.FromResult<Result>(pathResult.Errors);
        var fullPath = pathResult.Value;

        // Check: File exists before attempting delete
        if (!File.Exists(fullPath))
            return Task.FromResult<Result>(StorageResult.Failure.NotFound(key));

        try
        {
            File.Delete(fullPath);
            Loggers.LogDeleteSuccess(logger, key);
            return Task.FromResult(Result.Ok());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Catch: File I/O error during delete — surface as provider error
            Loggers.LogDeleteFailed(logger, key, ex);
            return Task.FromResult<Result>(StorageResult.Failure.ProviderError(ex.Message));
        }
    }

    public Task<Result<StoredObjectInfo>> StatAsync(string key, CancellationToken ct = default)
    {
        // Guard: Honour caller cancellation before initiating any I/O
        ct.ThrowIfCancellationRequested();

        // Guard: Resolve and validate key — rejects path traversal
        var pathResult = ResolvePath(key);
        if (!pathResult.IsSuccess)
            return Task.FromResult<Result<StoredObjectInfo>>(pathResult.Errors);
        var fullPath = pathResult.Value;

        // Check: File exists before querying metadata
        if (!File.Exists(fullPath))
            return Task.FromResult<Result<StoredObjectInfo>>(StorageResult.Failure.NotFound(key));

        try
        {
            var info = new FileInfo(fullPath);
            return Task.FromResult(Result<StoredObjectInfo>.Ok(
                new StoredObjectInfo(key, Name, info.Length, info.LastWriteTimeUtc, ContentType: null)));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Catch: File I/O error during stat — surface as provider error
            Loggers.LogStatFailed(logger, key, ex);
            return Task.FromResult<Result<StoredObjectInfo>>(StorageResult.Failure.ProviderError(ex.Message));
        }
    }

    public Task<Result<IReadOnlyList<StoredObjectInfo>>> ListAsync(string? prefix, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var root = options.Value.LocalPath;

        // Check: Storage root does not exist — return empty list, not an error
        if (!Directory.Exists(root))
            return Task.FromResult(Result<IReadOnlyList<StoredObjectInfo>>.Ok([]));

        try
        {
            // Batch: Enumerate all files recursively — filter by prefix after projection
            var files = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                .Select(f =>
                {
                    var relativeKey = Path.GetRelativePath(root, f).Replace('\\', '/');
                    var fi = new FileInfo(f);
                    return new StoredObjectInfo(relativeKey, Name, fi.Length, fi.LastWriteTimeUtc, ContentType: null);
                })
                .Where(o => prefix is null || o.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Task.FromResult(Result<IReadOnlyList<StoredObjectInfo>>.Ok(files.AsReadOnly()));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Catch: File I/O error during directory enumeration — surface as provider error
            Loggers.LogListFailed(logger, ex);
            return Task.FromResult<Result<IReadOnlyList<StoredObjectInfo>>>(StorageResult.Failure.ProviderError(ex.Message));
        }
    }

    public Result<string> ResolvePath(string key)
    {
        try
        {
            // Guard: Normalise root path and combine with request key
            var root = Path.GetFullPath(options.Value.LocalPath);
            var combined = Path.GetFullPath(Path.Combine(root, key.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

            // Guard: Prevent path traversal — combined path must stay within root
            if (!combined.StartsWith(root, StringComparison.Ordinal))
                return StorageResult.Failure.PathTraversalDetected(key);

            return Result<string>.Ok(combined);
        }
        catch (Exception ex)
        {
            // Catch: Path resolution failed (invalid chars, etc.) — surface as provider error
            return StorageResult.Failure.ProviderError(ex.Message);
        }
    }
}
