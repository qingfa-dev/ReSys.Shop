using Shared.Operational.Security.Encryption;

namespace Shared.UnitTests.Operational.Security.Encryption;

[Trait("Category", "Unit")]
[Trait("Module", "Security")]
[Trait("Feature", "Encryption")]
public sealed class AesEncryptionServiceTests
{
    private const string TestKey = "0123456789abcdef0123456789abcdef";

    [Fact(DisplayName = "Encrypt then Decrypt should return original string")]
    public void EncryptThenDecrypt_ShouldReturnOriginal()
    {
        var service = new AesEncryptionService(TestKey);
        var original = "Hello, World! This is a test string.";

        var encrypted = service.Encrypt(original);
        var decrypted = service.Decrypt(encrypted);

        decrypted.Should().Be(original);
    }

    [Fact(DisplayName = "Encrypted output should not contain original plaintext")]
    public void Encrypt_ShouldNotContainOriginal()
    {
        var service = new AesEncryptionService(TestKey);
        var original = "sensitive-data-12345";

        var encrypted = service.Encrypt(original);

        encrypted.Should().NotContain(original);
        encrypted.Should().NotBe(original);
    }

    [Fact(DisplayName = "Same input produces different ciphertext (random IV)")]
    public void Encrypt_SameInput_ProducesDifferentCiphertext()
    {
        var service = new AesEncryptionService(TestKey);
        var input = "same data";

        var encrypted1 = service.Encrypt(input);
        var encrypted2 = service.Encrypt(input);

        encrypted1.Should().NotBe(encrypted2);
    }

    [Fact(DisplayName = "Constructor throws when key is null or empty")]
    public void Constructor_NullOrEmptyKey_ShouldThrow()
    {
        Assert.Throws<InvalidOperationException>(() => new AesEncryptionService(null!));
        Assert.Throws<InvalidOperationException>(() => new AesEncryptionService(string.Empty));
    }

    [Fact(DisplayName = "Constructor throws when key is shorter than 32 bytes")]
    public void Constructor_ShortKey_ShouldThrow()
    {
        Assert.Throws<InvalidOperationException>(() => new AesEncryptionService("short-key"));
    }

    [Fact(DisplayName = "Decrypt of empty string returns empty string")]
    public void Decrypt_EmptyString_ReturnsEmpty()
    {
        var service = new AesEncryptionService(TestKey);

        var result = service.Decrypt("");

        result.Should().BeEmpty();
    }
}
