using Microsoft.Extensions.Options;

using Shared.Operational.Storages.Models;
using Shared.Operational.Storages.Security.Options;

namespace Shared.Operational.Storages.Security;

internal sealed partial class StorageSecurityEnforcer(
    IOptions<StorageSecuritySetting> options,
    ILogger<StorageSecurityEnforcer> logger)
    : IStorageSecurityEnforcer
{
    public async Task<Result> EnforceAsync(UploadRequest request, CancellationToken ct = default)
    {
        string extension = Path.GetExtension(request.Key);

        Result blockedResult = CheckBlockedExtension(extension);
        if (!blockedResult.IsSuccess)
        {
            Loggers.LogBlockedExtension(logger, request.Key, extension);
            return blockedResult;
        }

        Result allowedResult = CheckAllowedExtension(extension);
        if (!allowedResult.IsSuccess)
        {
            Loggers.LogForbiddenExtension(logger, request.Key, extension);
            return allowedResult;
        }

        Result sizeResult = CheckFileSize(request.Content);
        if (!sizeResult.IsSuccess)
        {
            Loggers.LogFileSizeExceeded(logger, request.Key, options.Value.MaxFileSizeBytes);
            return sizeResult;
        }

        if (options.Value.ValidateMagicBytes)
        {
            Result magicResult = await CheckMagicBytesAsync(request.Content, extension, ct);
            if (!magicResult.IsSuccess)
            {
                Loggers.LogMagicBytesMismatch(logger, request.Key, extension);
                return magicResult;
            }
        }

        Loggers.LogSecurityCheckPassed(logger, request.Key);
        return Result.Ok();
    }

    private Result CheckBlockedExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            return StorageSecurityEnforcerResult.Failure.BlockedExtension("(none)");

        if (options.Value.BlockedExtensions.Contains(extension))
            return StorageSecurityEnforcerResult.Failure.BlockedExtension(extension);

        return Result.Ok();
    }

    private Result CheckAllowedExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            return StorageResult.Failure.ForbiddenExtension("(none)");

        if (options.Value.AllowedExtensions.Count == 0)
            return StorageResult.Failure.ForbiddenExtension(extension);

        if (!options.Value.AllowedExtensions.Contains(extension))
            return StorageResult.Failure.ForbiddenExtension(extension);

        return Result.Ok();
    }

    private Result CheckFileSize(Stream content)
    {
        long length;
        try
        {
            length = content.Length;
        }
        catch (NotSupportedException)
        {
            return StorageSecurityEnforcerResult.Failure.FileSizeUnknown();
        }

        if (length > options.Value.MaxFileSizeBytes)
            return StorageResult.Failure.FileTooLarge(options.Value.MaxFileSizeBytes);

        return Result.Ok();
    }

    private static async Task<Result> CheckMagicBytesAsync(Stream content, string extension, CancellationToken ct)
    {
        if (!KnownMagicSignatures.TryGetValue(extension, out byte[][]? signatures))
            return Result.Ok();

        int maxSignatureLength = signatures.Max(s => s.Length);
        byte[] header = await ReadExactAsync(content, maxSignatureLength, ct);

        bool matched = false;
        foreach (byte[] signature in signatures)
        {
            if (header.AsSpan(0, signature.Length).SequenceEqual(signature))
            {
                matched = true;
                break;
            }
        }

        content.Seek(0, SeekOrigin.Begin);

        if (!matched)
            return StorageSecurityEnforcerResult.Failure.MagicBytesMismatch(extension);

        return Result.Ok();
    }

    private static async Task<byte[]> ReadExactAsync(Stream stream, int count, CancellationToken ct)
    {
        byte[] buffer = new byte[count];
        int offset = 0;
        while (offset < count)
        {
            int read = await stream.ReadAsync(buffer.AsMemory(offset, count - offset), ct);
            if (read == 0)
                break;
            offset += read;
        }
        return buffer;
    }
}
