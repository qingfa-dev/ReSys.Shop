using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Operational.Security.Encryption;

namespace Shared.Persistence.Converters;

public sealed class EncryptedDictionaryConverter : ValueConverter<Dictionary<string, string>, string>
{
    private static Func<IEncryptionService>? _encryptionServiceFactory;

    public static void Configure(Func<IEncryptionService> factory)
    {
        _encryptionServiceFactory = factory;
    }

    public EncryptedDictionaryConverter()
        : base(
            convertToProviderExpression: dict => EncryptDictionary(dict),
            convertFromProviderExpression: encrypted => DecryptDictionary(encrypted))
    {
    }

    private static string EncryptDictionary(Dictionary<string, string> dict)
    {
        var json = JsonSerializer.Serialize(dict);
        return GetService().Encrypt(json);
    }

    private static Dictionary<string, string> DecryptDictionary(string encrypted)
    {
        if (string.IsNullOrEmpty(encrypted))
            return [];

        var json = GetService().Decrypt(encrypted);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
    }

    private static IEncryptionService GetService()
    {
        return _encryptionServiceFactory?.Invoke()
            ?? throw new InvalidOperationException(
                "EncryptedDictionaryConverter.Configure() must be called at startup.");
    }
}
