using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Operational.Security.Encryption;

namespace Shared.Operational.Persistence.Configurations.Dictionaries;

public sealed class EncryptedDictionaryConverter : ValueConverter<Dictionary<string, string>, string>
{
    private static IServiceProvider? _serviceProvider;
    private static Func<IServiceProvider, IEncryptionService>? _resolver;

    public static void Configure(Func<IServiceProvider, IEncryptionService> resolver)
    {
        _resolver = resolver;
    }

    public static void ConfigureServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static IEncryptionService GetService()
    {
        if (_serviceProvider is not null && _resolver is not null)
            return _resolver(_serviceProvider);

        throw new InvalidOperationException(
            "EncryptedDictionaryConverter.Configure() and ConfigureServiceProvider() must be called at startup.");
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
}
