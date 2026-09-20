namespace Shared.Operational.Security.Encryption;

/// <summary>Encrypts and decrypts strings using the configured algorithm.</summary>
public interface IEncryptionService
{
    /// <summary>Encrypts a plaintext string.</summary>
    /// <param name="plaintext">The plaintext to encrypt.</param>
    /// <returns>The encrypted ciphertext.</returns>
    string Encrypt(string plaintext);

    /// <summary>Decrypts a ciphertext string.</summary>
    /// <param name="ciphertext">The ciphertext to decrypt.</param>
    /// <returns>The decrypted plaintext.</returns>
    string Decrypt(string ciphertext);
}
