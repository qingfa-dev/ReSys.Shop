namespace Shared.Security.Cors.Options;

public sealed class CorsSetting
{
    public const string SectionName = "Cors";

    public string[] Origins { get; init; } = [];

    public bool AllowCredentials { get; init; } = true;
}