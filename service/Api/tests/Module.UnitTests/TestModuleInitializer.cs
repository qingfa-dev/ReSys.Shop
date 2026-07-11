using System.Runtime.CompilerServices;
using Shared.Operational.Security.Encryption;
using Shared.Operational.Persistence.Configurations.Dictionaries;

namespace Module.UnitTests;

internal static class TestModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        EncryptedDictionaryConverter.Configure(() => new TestEncryptionService());
    }
}

internal sealed class TestEncryptionService : IEncryptionService
{
    public string Encrypt(string plaintext) => $"enc:{plaintext}";
    public string Decrypt(string ciphertext) => ciphertext.StartsWith("enc:") ? ciphertext[4..] : ciphertext;
}
