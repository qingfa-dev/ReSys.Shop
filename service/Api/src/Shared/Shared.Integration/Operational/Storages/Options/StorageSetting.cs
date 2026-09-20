using Shared.Operational.Storages.Security.Options;

namespace Shared.Operational.Storages.Options;

public sealed class StorageSetting
{
    public const string SectionName = "Storage";

    public string DefaultProvider { get; set; } = StorageSettingConstant.Defaults.DefaultProvider;

    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the storage subsystem is active. Default: <c>true</c>.</summary>
    public bool Enabled { get; set; } = true;

    public StorageSecuritySetting Security { get; set; } = new();
}
