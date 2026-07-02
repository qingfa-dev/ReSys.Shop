namespace Shared.Operational.Storages.Models;

/// <summary>Encapsulates the data required to upload an object to a storage provider.</summary>
/// <param name="Key">Logical path / filename for the object (e.g. <c>avatars/user-42.png</c>).</param>
/// <param name="Content">The raw bytes to persist.</param>
/// <param name="ContentType">MIME type of the content (e.g. <c>image/png</c>).</param>
/// <param name="Metadata">Optional key-value metadata to attach to the object.</param>
public sealed record UploadRequest(
    string Key,
    Stream Content,
    string ContentType,
    IReadOnlyDictionary<string, string>? Metadata = null,
    UploadOptions? Options = null);


public sealed record UploadOptions
{
    public bool Overwrite { get; init; }
    public bool GenerateHash { get; init; } = true;
    public bool Encrypt { get; init; }

    // Malware
    public bool ScanForMalware { get; init; }
    public int MalwareScanTimeoutSeconds { get; init; } = 30;
    public InfectionAction OnMalwareDetected { get; init; }

    // Image resizing
    public int? ResizeWidth { get; init; }
    public int? ResizeHeight { get; init; }
    public ResizeMode ResizeMode { get; init; }
    public bool MaintainAspectRatio { get; init; } = true;
    public string? OutputFormat { get; init; }
}