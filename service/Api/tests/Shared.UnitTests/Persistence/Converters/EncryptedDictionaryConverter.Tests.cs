using Shared.Operational.Security.Encryption;
using Shared.Persistence.Converters;

namespace Shared.UnitTests.Persistence.Converters;

[Trait("Category", "Unit")]
[Trait("Module", "Persistence")]
[Trait("Feature", "EncryptedConverter")]
public sealed class EncryptedDictionaryConverterTests : IDisposable
{
    private readonly IEncryptionService _encryptionService;

    public EncryptedDictionaryConverterTests()
    {
        _encryptionService = new AesEncryptionService("0123456789abcdef0123456789abcdef");
        EncryptedDictionaryConverter.Configure(() => _encryptionService);
    }

    public void Dispose() => GC.SuppressFinalize(this);

    [Fact(DisplayName = "Roundtrip: write encrypted, read back decrypted")]
    public void Roundtrip_ShouldReturnOriginal()
    {
        var converter = new EncryptedDictionaryConverter();
        var original = new Dictionary<string, string>
        {
            ["merchant_id"] = "acct_12345",
            ["endpoint_url"] = "https://api.example.com/v1",
            ["region"] = "us-east-1"
        };

        var encrypted = (string)converter.ConvertToProvider(original)!;
        var decrypted = (Dictionary<string, string>)converter.ConvertFromProvider(encrypted)!;

        decrypted.Count.Should().Be(3);
        decrypted["merchant_id"].Should().Be("acct_12345");
        decrypted["endpoint_url"].Should().Be("https://api.example.com/v1");
        decrypted["region"].Should().Be("us-east-1");
    }

    [Fact(DisplayName = "Encrypted output is not human-readable")]
    public void Encrypted_ShouldNotContainOriginalValues()
    {
        var converter = new EncryptedDictionaryConverter();
        var original = new Dictionary<string, string> { ["secret"] = "super-secret-value-999" };

        var encrypted = (string)converter.ConvertToProvider(original)!;

        encrypted.Should().NotContain("super-secret-value-999");
        encrypted.Should().NotContain("secret");
    }

    [Fact(DisplayName = "Empty dictionary roundtrips")]
    public void EmptyDictionary_Roundtrips()
    {
        var converter = new EncryptedDictionaryConverter();
        var original = new Dictionary<string, string>();

        var encrypted = (string)converter.ConvertToProvider(original)!;
        var decrypted = (Dictionary<string, string>)converter.ConvertFromProvider(encrypted)!;

        decrypted.Should().NotBeNull();
        decrypted.Count.Should().Be(0);
    }

    [Fact(DisplayName = "Null/empty encrypted string returns empty dictionary")]
    public void Decrypt_EmptyOrNullString_ReturnsEmpty()
    {
        var converter = new EncryptedDictionaryConverter();

        var fromEmpty = (Dictionary<string, string>)converter.ConvertFromProvider("")!;

        fromEmpty.Count.Should().Be(0);
    }

    [Fact(DisplayName = "Unconfigured converter throws")]
    public void Unconfigured_ShouldThrow()
    {
        EncryptedDictionaryConverter.Configure(null!);

        var converter = new EncryptedDictionaryConverter();
        var act = () => converter.ConvertToProvider(new Dictionary<string, string> { ["x"] = "y" });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Configure()*");
    }
}
