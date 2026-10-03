using System.Text;

using Shared.Operational.Storages.Helpers;

namespace Shared.Operational.Security.Encryption;

/// <summary>Encrypts and decrypts strings using AES with a configured key (minimum 32 bytes).</summary>
// Contract: pre=encryptionKey.Length >= 32 bytes, post=ciphertext is Base64-encoded AES cipher
public sealed class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    /// <summary>Creates an AES encryption service with the specified key.</summary>
    /// <param name="encryptionKey">The encryption key. Must be at least 32 bytes.</param>
    /// <exception cref="InvalidOperationException">Thrown when key is missing or too short.</exception>
    public AesEncryptionService(string encryptionKey)
    {
        if (string.IsNullOrEmpty(encryptionKey))
            throw new InvalidOperationException("Encryption key is not configured.");
        if (Encoding.UTF8.GetByteCount(encryptionKey) < 32)
            throw new InvalidOperationException("Encryption key must be at least 32 bytes.");
        _key = Encoding.UTF8.GetBytes(encryptionKey);
    }

    /// <summary>Encrypts plaintext to Base64-encoded ciphertext using AES.</summary>
    /// <param name="plaintext">The text to encrypt.</param>
    /// <returns>Base64-encoded AES ciphertext.</returns>
    public string Encrypt(string plaintext)
    {
        using var plainStream = new MemoryStream(Encoding.UTF8.GetBytes(plaintext));
        using var cipherStream = EncryptionHelper.EncryptAsync(plainStream, _key).GetAwaiter().GetResult();
        var bytes = ((MemoryStream)cipherStream).ToArray();
        return Convert.ToBase64String(bytes);
    }

    /// <summary>Decrypts Base64-encoded AES ciphertext back to plaintext.</summary>
    /// <param name="ciphertext">The Base64-encoded ciphertext to decrypt.</param>
    /// <returns>The decrypted plaintext, or empty string if input is null/empty.</returns>
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
