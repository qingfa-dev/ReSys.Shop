using System.Text;
using System.Security.Cryptography;
using Shared.Operational.Storages.Helpers;

namespace Shared.Operational.Security.Encryption;

public sealed class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public AesEncryptionService(string encryptionKey)
    {
        if (string.IsNullOrEmpty(encryptionKey))
            throw new InvalidOperationException("Encryption key is not configured.");
        if (Encoding.UTF8.GetByteCount(encryptionKey) < 32)
            throw new InvalidOperationException("Encryption key must be at least 32 bytes.");
        _key = Encoding.UTF8.GetBytes(encryptionKey);
    }

    public string Encrypt(string plaintext)
    {
        using var plainStream = new MemoryStream(Encoding.UTF8.GetBytes(plaintext));
        using var cipherStream = EncryptionHelper.EncryptAsync(plainStream, _key).GetAwaiter().GetResult();
        var bytes = ((MemoryStream)cipherStream).ToArray();
        return Convert.ToBase64String(bytes);
    }

    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            return string.Empty;

        var bytes = Convert.FromBase64String(ciphertext);
        using var cipherStream = new MemoryStream(bytes);
        using var plainStream = EncryptionHelper.DecryptAsync(cipherStream, _key).GetAwaiter().GetResult();
        using var reader = new StreamReader(plainStream);
        return reader.ReadToEnd();
    }
}
