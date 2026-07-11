namespace Shared.Governance.Conventions;

public static class DictionaryExtensions
{
    public static IReadOnlyDictionary<string, object?>? ToDictionary(
        this (string Key, object? Value)[] metadata)
    {
        return metadata.Length == 0
            ? null
            : metadata.ToDictionary(x => x.Key, x => x.Value);
    }
}