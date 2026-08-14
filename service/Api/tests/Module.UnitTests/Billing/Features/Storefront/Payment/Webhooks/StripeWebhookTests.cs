using Hangfire;
using Hangfire.Common;
using Hangfire.States;

using Module.Billing.Domain.WebhookEvents;

using Stripe;

using IStripeWebhookService = Module.Billing.Services.Webhook.IStripeWebhookService;

using Module.Billing.Features.Storefront.Payment.Webhooks;

namespace Module.UnitTests.Payment.Features.Storefront.Payment.Webhooks;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "StripeWebhook")]
public class StripeWebhookTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStripeWebhookService> _webhookMock;
    private readonly Mock<IBackgroundJobClient> _bgJobClientMock;
    private readonly StripeWebhook.CommandHandler _handler;

    public StripeWebhookTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(WebhookEvent).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _webhookMock = new Mock<IStripeWebhookService>();
        _webhookMock.Setup(x => x.ValidateSignature(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event { Id = "evt_default_1", Type = "payment_intent.succeeded" });
        _bgJobClientMock = new Mock<IBackgroundJobClient>();
        _bgJobClientMock.Setup(b => b.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>())).Returns("job-1");
        _handler = new StripeWebhook.CommandHandler(
            _webhookMock.Object, _dbContext, _bgJobClientMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Webhook: invalid signature returns failure")]
    public async Task Handle_ShouldFail_WhenInvalidSignature()
    {
        _webhookMock.Setup(x => x.ValidateSignature(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        var result = await _handler.Handle(new StripeWebhook.Command("{}", "invalid"), TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Stripe.Webhook.InvalidSignature");
    }

    [Fact(DisplayName = "Webhook: unparseable payload returns InvalidPayload failure")]
    public async Task Handle_ShouldFail_WhenPayloadUnparseable()
    {
        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>())).Returns((Event?)null);
        var result = await _handler.Handle(new StripeWebhook.Command("{}", "sig"), TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Stripe.Webhook.InvalidPayload");
    }

    [Fact(DisplayName = "Webhook: persists a WebhookEvent and enqueues the job by event id")]
    public async Task Handle_ShouldPersistEventAndEnqueueById()
    {
        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event { Id = "evt_persist_1", Type = "payment_intent.succeeded" });

        Job? enqueuedJob = null;
        _bgJobClientMock.Setup(b => b.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()))
            .Callback<Job, IState>((job, _) => enqueuedJob = job)
            .Returns("job-1");

        var result = await _handler.Handle(
            new StripeWebhook.Command("{}", "sig"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var saved = await _dbContext.Set<WebhookEvent>().FirstAsync();
        saved.StripeEventId.Should().Be("evt_persist_1");
        saved.Type.Should().Be("payment_intent.succeeded");
        saved.Payload.Should().Be("{}");
        saved.State.Should().Be(WebhookEventState.Pending);

        _bgJobClientMock.Verify(b => b.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()), Times.Once);
        enqueuedJob.Should().NotBeNull();
        enqueuedJob!.Method.Name.Should().Be("ExecuteAsync");
        var enqueuedEventId = (Guid)enqueuedJob.Args![0]!;
        enqueuedEventId.Should().Be(saved.Id);
    }

    [Fact(DisplayName = "Webhook: a duplicate StripeEventId returns Ok without re-enqueuing")]
    public async Task Handle_DuplicateEvent_DoesNotEnqueue()
    {
        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event { Id = "evt_dup_1", Type = "payment_intent.succeeded" });

        var first = await _handler.Handle(new StripeWebhook.Command("{}", "sig"), CancellationToken.None);
        var second = await _handler.Handle(new StripeWebhook.Command("{}", "sig"), CancellationToken.None);

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        second.Message.Should().Be("Webhook already accepted.");

        (await _dbContext.Set<WebhookEvent>().CountAsync()).Should().Be(1);
        _bgJobClientMock.Verify(b => b.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()), Times.Once);
    }

    [Fact(DisplayName = "Webhook: Returns Ok immediately after queueing")]
    public async Task Handle_ShouldReturnOk_ForValidSignature()
    {
        var result = await _handler.Handle(
            new StripeWebhook.Command("{}", "sig"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }
}
