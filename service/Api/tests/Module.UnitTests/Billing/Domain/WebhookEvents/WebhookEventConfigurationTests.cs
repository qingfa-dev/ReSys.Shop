using Microsoft.EntityFrameworkCore.Metadata;

using Module.Billing.Domain.WebhookEvents;
using Module.Billing.Persistence;

namespace Module.UnitTests.Payment.Domain.WebhookEvents;

[Trait("Category", "Unit")]
[Trait("Module", "Billing")]
[Trait("Feature", "WebhookEventConfiguration")]
public class WebhookEventConfigurationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;

    public WebhookEventConfigurationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(WebhookEvent).Assembly];
        _dbContext = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "WebhookEvent: Should map to payment.webhook_events")]
    public void WebhookEvent_ShouldMapToWebhookEventsTable()
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(WebhookEvent));

        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be(PaymentSchema.TableNames.WebhookEvents);
        entityType.GetSchema().Should().Be(PaymentSchema.Name);
    }

    [Fact(DisplayName = "WebhookEvent: Should have a unique index on StripeEventId")]
    public void WebhookEvent_ShouldHaveUniqueStripeEventIdIndex()
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(WebhookEvent));

        entityType.Should().NotBeNull();

        var uniqueIndex = entityType!
            .GetIndexes()
            .SingleOrDefault(i => i.IsUnique && i.Properties.Any(p => p.Name == nameof(WebhookEvent.StripeEventId)));

        uniqueIndex.Should().NotBeNull();
        uniqueIndex!.Properties.Select(p => p.Name).Should().Contain(nameof(WebhookEvent.StripeEventId));
    }

    [Fact(DisplayName = "WebhookEvent: State should use a string value converter")]
    public void WebhookEvent_StateShouldUseStringConverter()
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(WebhookEvent));

        entityType.Should().NotBeNull();

        var stateProperty = entityType!.FindProperty(nameof(WebhookEvent.State));
        stateProperty.Should().NotBeNull();

        var converter = stateProperty!.GetTypeMapping().Converter;
        converter.Should().NotBeNull();
        converter!.ProviderClrType.Should().Be(typeof(string));
    }
}
