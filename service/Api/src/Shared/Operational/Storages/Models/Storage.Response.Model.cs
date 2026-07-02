namespace Shared.Operational.Storages.Models;

/// <summary>Metadata returned after a successful upload.</summary>
/// <param name="Key">The key under which the object was stored.</param>
/// <param name="Provider">Name of the provider that accepted the upload.</param>
/// <param name="Uri">Public or pre-signed URI to the stored object, if applicable.</param>
/// <param name="SizeBytes">Persisted byte size as reported by the provider.</param>
/// <param name="StoredAtUtc">UTC timestamp of when the object was committed.</param>
public sealed record UploadResult(
    string Key,
    string Provider,
    Uri? Uri,
    long SizeBytes,
    DateTimeOffset StoredAtUtc);
 
/// <summary>Metadata for a stored object returned by a listing or stat operation.</summary>
/// <param name="Key">Object key.</param>
/// <param name="Provider">Provider that holds the object.</param>
/// <param name="SizeBytes">Byte size of the object.</param>
/// <param name="LastModifiedUtc">UTC timestamp of the last write.</param>
/// <param name="ContentType">MIME type, if known.</param>
public sealed record StoredObjectInfo(
    string Key,
    string Provider,
    long SizeBytes,
    DateTimeOffset LastModifiedUtc,
    string? ContentType);
 
/// <summary>Carries a downloaded object's byte stream and its metadata.</summary>
/// <param name="Content">Readable stream of the object's bytes.</param>
/// <param name="Info">Metadata associated with the downloaded object.</param>
public sealed record DownloadResult(Stream Content, StoredObjectInfo Info);
 