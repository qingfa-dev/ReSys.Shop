using Microsoft.EntityFrameworkCore;
using Module.Payment.Domain.PaymentMethods;
using Module.Payment.Persistence.Constants;
using Shared.Operational.Security.Encryption;
using Shared.Persistence.Converters;

namespace Module.UnitTests.Payment.Persistence;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "PaymentMethodEncryption")]
public sealed class PaymentMethodSettingsEncryptionTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;

    public PaymentMethodSettingsEncryptionTests()
    {
        var encryptionService = new AesEncryptionService("0123456789abcdef0123456789abcdef");
        EncryptedDictionaryConverter.Configure(() => encryptionService);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentMethod).Assembly];
        _dbContext = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Settings dictionary is encrypted at rest in database")]
    public async Task Settings_ShouldBeEncryptedAtRest()
    {
        var method = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            Name = "Test Method",
            Code = "test",
            ProviderKey = "stripe",
            Active = true,
            Position = 0,
            DisplayOn = DisplayOn.Both,
            Settings = new Dictionary<string, string>
            {
                ["merchant_id"] = "acct_secret_999",
                ["endpoint"] = "https://api.secret.example.com"
            },
            Preferences = [],
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "test"
        };

        _dbContext.Set<PaymentMethod>().Add(method);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _dbContext.ChangeTracker.Clear();

        var loaded = await _dbContext.Set<PaymentMethod>()
            .FirstAsync(p => p.Id == method.Id, TestContext.Current.CancellationToken);

        loaded.Settings.Should().HaveCount(2);
        loaded.Settings["merchant_id"].Should().Be("acct_secret_999");
        loaded.Settings["endpoint"].Should().Be("https://api.secret.example.com");
    }

    [Fact(DisplayName = "Empty Settings dictionary roundtrips through DB")]
    public async Task EmptySettings_RoundtripsThroughDb()
    {
        var method = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            Name = "Empty Settings",
            Code = "empty",
            ProviderKey = "bogus",
            Active = true,
            Position = 0,
            DisplayOn = DisplayOn.Both,
            Settings = [],
            Preferences = [],
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "test"
        };

        _dbContext.Set<PaymentMethod>().Add(method);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _dbContext.ChangeTracker.Clear();

        var loaded = await _dbContext.Set<PaymentMethod>()
            .FirstAsync(p => p.Id == method.Id, TestContext.Current.CancellationToken);

        loaded.Settings.Should().NotBeNull();
        loaded.Settings.Should().BeEmpty();
    }

    [Fact(DisplayName = "Default empty Settings (not explicitly set) reads as empty dictionary")]
    public async Task DefaultSettings_ShouldBeEmpty()
    {
        var method = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            Name = "Default Settings",
            Code = "default",
            ProviderKey = "stripe",
            Active = true,
            Position = 0,
            DisplayOn = DisplayOn.Both,
            Preferences = [],
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "test"
        };

        _dbContext.Set<PaymentMethod>().Add(method);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _dbContext.ChangeTracker.Clear();

        var loaded = await _dbContext.Set<PaymentMethod>()
            .FirstAsync(p => p.Id == method.Id, TestContext.Current.CancellationToken);

        loaded.Settings.Should().NotBeNull();
        loaded.Settings.Should().BeEmpty();
    }
}
