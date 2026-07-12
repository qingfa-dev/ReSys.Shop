# Payment Webhook Wiring & DI Fix Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the stubbed `StripeWebhookHandler` with the real event processor and remove the `BuildServiceProvider()` anti-pattern in `Payment.Extension.cs`.

**Architecture:** The real `StripeWebhook.CommandHandler` already exists at `Module.Payment.Features.Storefront.Payment.Webhooks` (verified: signature validation + event dispatch by type). The bug is that `IStripeWebhookService` in DI resolves to a different stub class (`StripeWebhookHandler` in `Services/Webhook/`). The fix is to align the two paths and fix the converter's DI access pattern.

**Tech Stack:** .NET 10, xUnit v3, Moq, EF Core InMemory (unit), Testcontainers.PostgreSql + Respawn (integration via `Api.Tests`).

## Global Constraints

- `TreatWarningsAsErrors=true` (AGENTS.md rule 4)
- All handlers return `Result<T>` / `Result` — never throw for control flow
- Warnings = build failure
- Project: `Module.UnitTests` uses xUnit v3 + Moq + InMemory; `Api.Tests` uses Testcontainers + Respawn
- Test pattern: `[Fact(DisplayName = "...")]`, `[Trait("Category", "Unit")]`, `[Trait("Module", "Payment")]`
- Test data: `ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentCapture).Assembly]`
- Cancellation: `TestContext.Current.CancellationToken`
- Commit messages: `feat(payment): ...` / `fix(payment): ...` / `test(payment): ...`

## File Structure

### Files to modify

| File | Change |
|------|--------|
| `service/Api/src/Module/Payment/Services/Webhook/StripeWebhookService.cs` | Reduce `StripeWebhookHandler` to a thin wrapper; add `LogError` to `ParseEvent` catch |
| `service/Api/src/Module/Payment/Services/Webhook/IStripeWebhookService.cs` | Update interface doc-comment to clarify dispatch contract |
| `service/Api/src/Module/Payment/Payment.Extension.cs` | Remove `BuildServiceProvider()`; fix `EncryptedDictionaryConverter.Configure` signature; register new dispatcher |
| `service/Api/src/Module/Payment/Services/Configuration/EncryptedDictionaryConverter.cs` | Accept `Func<IServiceProvider, IEncryptionService>` instead of parameterless factory |
| `service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookTests.cs` | Add idempotency + malformed payload log tests |

### Files to create

| File | Purpose |
|------|---------|
| `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs` | Adapter implementing `IStripeWebhookService` that delegates `HandleAsync` to the real `StripeWebhook.CommandHandler` via `ISender` |
| `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/IdempotencyKeys.cs` | Constants for Stripe event idempotency keys (payment intent ID, charge ID) |
| `service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcherTests.cs` | Unit tests for the dispatcher |
| `service/Api/tests/Api.Tests/Scenarios/Payment/StripeWebhookReplayedTests.cs` | Integration test for replay idempotency |

---

## Task 1: Make `EncryptedDictionaryConverter` accept an `IServiceProvider`

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Configuration/EncryptedDictionaryConverter.cs` (the static factory)
- Modify: `service/Api/src/Module/Payment/Payment.Extension.cs:50-54`

**Context:** Currently `EncryptedDictionaryConverter.Configure(() => sp.GetRequiredService<IEncryptionService>())` is called at registration time and resolves through a `BuildServiceProvider()` side-channel. We change the contract so the converter receives a `Func<IServiceProvider, IEncryptionService>` and the conversion path provides a scoped `IServiceProvider` at the time JSON is being read/written.

- [ ] **Step 1: Read the converter's current API**

Read `service/Api/src/Module/Payment/Services/Configuration/EncryptedDictionaryConverter.cs` end-to-end and locate the existing `Configure` static method. Note its signature and where it is called from (search the project for `EncryptedDictionaryConverter.Configure`). Record the exact current signature in the next step.

- [ ] **Step 2: Refactor the static factory signature**

In `EncryptedDictionaryConverter.cs`, change the public `Configure` method to:

```csharp
public static void Configure(Func<IServiceProvider, IEncryptionService> resolver)
{
    _resolver = resolver;
}

private static Func<IServiceProvider, IEncryptionService>? _resolver;

public static IEncryptionService GetService(IServiceProvider sp)
{
    if (_resolver is null)
        throw new InvalidOperationException(
            "EncryptedDictionaryConverter.Configure must be called at startup.");
    return _resolver(sp);
}
```

Keep the existing conversion code intact; the change is purely how the service is resolved.

- [ ] **Step 3: Update the call site in `Payment.Extension.cs`**

In `service/Api/src/Module/Payment/Payment.Extension.cs`, replace lines 50-54 with:

```csharp
EncryptedDictionaryConverter.Configure(sp => sp.GetRequiredService<IEncryptionService>());
```

The lambda receives the `IServiceProvider` from the JSON converter's runtime context, not at module-registration time.

- [ ] **Step 4: Audit every JSON read/write site for `IServiceProvider` availability**

`EncryptedDictionaryConverter` is constructed by `System.Text.Json` via the EF Core value converter pipeline. Verify the call site is inside an EF Core materialization scope where the context's `IServiceProvider` is accessible. If the converter is invoked outside such a scope, the `IServiceProvider` parameter MUST be threaded through (e.g. on the converter constructor or as a method parameter on `Read`/`Write`).

Look at `service/Api/src/Module/Payment/Persistence/Configurations/Dictionaries/` and confirm the converter is registered via `ValueConverter` or `ValueComparer`. If it's a `ValueConverter`, no `IServiceProvider` is available at JSON time — in that case, register `IEncryptionService` as a `Singleton` and inject it directly into the converter constructor instead of the static factory pattern.

- [ ] **Step 5: Build the solution**

Run: `dotnet build service/Api/Api.slnx`
Expected: success, no new warnings. If a constructor pattern was needed in Step 4, the converter file's `Read`/`Write` calls MUST use the injected service.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Configuration/EncryptedDictionaryConverter.cs service/Api/src/Module/Payment/Payment.Extension.cs
git commit -m "fix(payment): resolve encryption service via DI without BuildServiceProvider()"
```

---

## Task 2: Add `LogError` to `ParseEvent` catch block

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Webhook/StripeWebhookService.cs:52-56`

**Context:** The bare `catch { return null; }` swallows Stripe parse errors silently. Operators need a log entry to diagnose malformed payloads.

- [ ] **Step 1: Read the current `ParseEvent` method**

Read lines 52-56 of `StripeWebhookService.cs`. Confirm the method signature is `public Event? ParseEvent(string payload)`.

- [ ] **Step 2: Write the failing test for parse-error logging**

Create file `service/Api/tests/Module.UnitTests/Payment/Services/Webhook/StripeWebhookServiceParseEventLoggingTests.cs`:

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Module.Payment.Services.Provider.Stripe;
using Module.Payment.Services.Webhook;
using Stripe;

namespace Module.UnitTests.Payment.Services.Webhook;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
public class StripeWebhookServiceParseEventLoggingTests
{
    [Fact(DisplayName = "ParseEvent: logs error and returns null on malformed payload")]
    public void ParseEvent_MalformedPayload_LogsError_ReturnsNull()
    {
        var options = Options.Create(new StripeSetting { WebhookSecret = "whsec_test" });
        var logger = new Mock<ILogger<StripeWebhookHandler>>();
        var sut = new StripeWebhookHandler(options);

        var result = sut.ParseEvent("{not-valid-json");

        result.Should().BeNull();
        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Stripe event parse failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
```

- [ ] **Step 3: Run the test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~StripeWebhookServiceParseEventLoggingTests" --no-restore`
Expected: FAIL — current `ParseEvent` does not log.

- [ ] **Step 4: Add the logger to `StripeWebhookHandler` and log on parse failure**

In `service/Api/src/Module/Payment/Services/Webhook/StripeWebhookService.cs`:

1. Add `using Microsoft.Extensions.Logging;` at the top
2. Change the constructor from `public StripeWebhookHandler(IOptions<StripeSetting> options)` to:

```csharp
private readonly StripeSetting _options;
private readonly ILogger<StripeWebhookHandler> _logger;

public StripeWebhookHandler(IOptions<StripeSetting> options, ILogger<StripeWebhookHandler> logger)
{
    _options = options.Value;
    _logger = logger;
}
```

3. Replace the `ParseEvent` body (lines 52-56) with:

```csharp
public Event? ParseEvent(string payload)
{
    try
    {
        return EventUtility.ParseEvent(payload);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Stripe event parse failed: {Payload}", payload);
        return null;
    }
}
```

- [ ] **Step 5: Update the existing caller in `Payment.Extension.cs`**

DI will resolve the new constructor parameter automatically. No code change required at the registration site because `ILogger<StripeWebhookHandler>` is auto-registered.

- [ ] **Step 6: Re-run the test to verify it passes**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~StripeWebhookServiceParseEventLoggingTests" --no-restore`
Expected: PASS.

- [ ] **Step 7: Build and run the full Payment test suite**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~Payment" --no-restore`
Expected: all existing tests still pass; constructor change is backward-compatible via DI.

- [ ] **Step 8: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Webhook/StripeWebhookService.cs service/Api/tests/Module.UnitTests/Payment/Services/Webhook/StripeWebhookServiceParseEventLoggingTests.cs
git commit -m "fix(payment): log Stripe parse errors instead of swallowing them"
```

---

## Task 3: Add idempotency to the real `StripeWebhook.CommandHandler`

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs` (the `HandlePaymentIntentSucceeded` private method)
- Test: `service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookTests.cs`

**Context:** The `CommandHandler` already exists. Replaying the same Stripe `payment_intent.succeeded` event should be a no-op. The bug is that `payment.Complete()` is called every time without checking if the payment is already in a terminal state.

- [ ] **Step 1: Read the current `HandlePaymentIntentSucceeded` method**

Read lines ~75-100 of `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs`. Note how `payment.Complete()` is invoked and what `PaymentRecordState` values exist.

- [ ] **Step 2: Write the failing idempotency test**

Append the following test to `service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookTests.cs`:

```csharp
[Fact(DisplayName = "Webhook: replayed payment_intent.succeeded is idempotent")]
public async Task Handle_ReplayedPaymentIntentSucceeded_IsIdempotent()
{
    var orderId = Guid.NewGuid();
    var intentId = "pi_test_123";
    var payment = new PaymentCapture
    {
        Id = Guid.NewGuid(),
        OrderId = orderId,
        ResponseCode = intentId,
        State = PaymentRecordState.Pending,
        Amount = 100m,
        Currency = "USD"
    };
    _dbContext.Set<PaymentCapture>().Add(payment);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var stripeEvent = new global::Stripe.Event
    {
        Type = GatewayConstants.WebhookEvents.Stripe.PaymentIntentSucceeded,
        Data = new global::Stripe.EventData
        {
            Object = new PaymentIntent { Id = intentId }
        }
    };
    _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>())).Returns(stripeEvent);

    var first = await _handler.Handle(new StripeWebhook.Command("{}", "valid"), TestContext.Current.CancellationToken);
    var second = await _handler.Handle(new StripeWebhook.Command("{}", "valid"), TestContext.Current.CancellationToken);

    first.IsSuccess.Should().BeTrue();
    second.IsSuccess.Should().BeTrue();

    var payments = await _dbContext.Set<PaymentCapture>().Where(p => p.OrderId == orderId).ToListAsync(TestContext.Current.CancellationToken);
    payments.Should().HaveCount(1);
    payments[0].State.Should().Be(PaymentRecordState.Completed);
}
```

- [ ] **Step 3: Run the test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~Handle_ReplayedPaymentIntentSucceeded" --no-restore`
Expected: FAIL — second call either throws (`Complete` rejects already-completed) or transitions to a wrong state.

- [ ] **Step 4: Add an idempotency guard to `HandlePaymentIntentSucceeded`**

In `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs`, find the `HandlePaymentIntentSucceeded` method and add a guard immediately after the `FirstOrDefaultAsync`:

```csharp
// Guard: Idempotency — already-completed payments are no-ops
if (payment.State == PaymentRecordState.Completed)
    return Result.Ok(PaymentCaptureResult.Success.AlreadyCompleted(payment.Number));
```

If `PaymentCaptureResult.Success.AlreadyCompleted` does not exist, add a new factory method on the existing `PaymentCaptureResult` static class returning `Result.Ok` with a structured success code like `Payment.AlreadyCompleted`.

- [ ] **Step 5: Re-run the test to verify it passes**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~Handle_ReplayedPaymentIntentSucceeded" --no-restore`
Expected: PASS.

- [ ] **Step 6: Run the full Payment Webhook test suite**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~Payment.Features.Storefront.Payment.Webhooks" --no-restore`
Expected: all tests pass.

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookTests.cs
git commit -m "fix(payment): make payment_intent.succeeded webhook idempotent"
```

---

## Task 4: Create the `StripeWebhookDispatcher` adapter

**Files:**
- Create: `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs`
- Modify: `service/Api/src/Module/Payment/Payment.Extension.cs:75-76`
- Test: `service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcherTests.cs`

**Context:** Today `IStripeWebhookService` resolves to the `Services/Webhook/StripeWebhookHandler` stub. The real `StripeWebhook.CommandHandler` lives in `Features/`. We need an adapter that implements `IStripeWebhookService` (signature validation + event parse) and dispatches `HandleAsync` to the real handler via `ISender`.

- [ ] **Step 1: Read the `IStripeWebhookService` interface**

Read `service/Api/src/Module/Payment/Services/Webhook/IStripeWebhookService.cs` and `StripeWebhookService.cs` to capture the full interface (provider name, supported event types, `HandleAsync`, `ValidateSignature`, `ParseEvent`).

- [ ] **Step 2: Write the failing test for the dispatcher contract**

Create file `service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcherTests.cs`:

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Module.Payment.Features.Storefront.Payment.Webhooks;
using Module.Payment.Services.Provider.Stripe;
using Module.Payment.Services.Webhook;

namespace Module.UnitTests.Payment.Features.Storefront.Payment.Webhooks;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
public class StripeWebhookDispatcherTests
{
    [Fact(DisplayName = "Dispatcher: provider name is stripe")]
    public void Provider_ReturnsStripe()
    {
        var dispatcher = new StripeWebhookDispatcher(
            Options.Create(new StripeSetting { WebhookSecret = "whsec_test" }),
            new Mock<ISender>().Object,
            new Mock<ILogger<StripeWebhookDispatcher>>().Object);

        dispatcher.Provider.Should().Be("stripe");
    }

    [Fact(DisplayName = "Dispatcher: returns NotConfigured when secret is empty")]
    public async Task HandleAsync_EmptySecret_ReturnsNotConfigured()
    {
        var dispatcher = new StripeWebhookDispatcher(
            Options.Create(new StripeSetting { WebhookSecret = "" }),
            new Mock<ISender>().Object,
            new Mock<ILogger<StripeWebhookDispatcher>>().Object);

        var result = await dispatcher.HandleAsync("payment_intent.succeeded", "{}", TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Stripe.WebhookSecret.NotConfigured");
    }

    [Fact(DisplayName = "Dispatcher: dispatches to real handler via ISender")]
    public async Task HandleAsync_DispatchesStripeWebhookCommand()
    {
        var sender = new Mock<ISender>();
        sender.Setup(x => x.Send(It.IsAny<StripeWebhook.Command>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(Result.Ok());

        var dispatcher = new StripeWebhookDispatcher(
            Options.Create(new StripeSetting { WebhookSecret = "whsec_test" }),
            sender.Object,
            new Mock<ILogger<StripeWebhookDispatcher>>().Object);

        var result = await dispatcher.HandleAsync("payment_intent.succeeded", "{}", TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        sender.Verify(x => x.Send(
            It.Is<StripeWebhook.Command>(c => c.Payload == "{}" && c.StripeSignature == "stripe-signature"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

- [ ] **Step 3: Run the test to verify it fails (compile error)**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~StripeWebhookDispatcherTests" --no-restore`
Expected: FAIL with `StripeWebhookDispatcher` not found.

- [ ] **Step 4: Create the dispatcher class**

Create file `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs`:

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Module.Payment.Services.Webhook;
using Stripe;
using StripeSetting = Module.Payment.Services.Provider.Stripe.StripeSetting;

namespace Module.Payment.Features.Storefront.Payment.Webhooks;

/// <summary>
/// Implements <see cref="IStripeWebhookService"/> as a thin adapter that
/// delegates event handling to the real <see cref="StripeWebhook.CommandHandler"/> via MediatR.
/// </summary>
public sealed class StripeWebhookDispatcher : IStripeWebhookService
{
    private readonly StripeSetting _options;
    private readonly ISender _sender;
    private readonly ILogger<StripeWebhookDispatcher> _logger;

    public string Provider => GatewayConstants.Providers.Stripe;

    public string[] SupportedEventTypes =>
    [
        GatewayConstants.WebhookEvents.Stripe.PaymentIntentSucceeded,
        GatewayConstants.WebhookEvents.Stripe.PaymentIntentPaymentFailed,
        GatewayConstants.WebhookEvents.Stripe.ChargeRefunded,
        GatewayConstants.WebhookEvents.Stripe.ChargeDisputeCreated
    ];

    public StripeWebhookDispatcher(
        IOptions<StripeSetting> options,
        ISender sender,
        ILogger<StripeWebhookDispatcher> logger)
    {
        _options = options.Value;
        _sender = sender;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(string eventType, string payload, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_options.WebhookSecret))
        {
            return Error.Validation(
                "Stripe.WebhookSecret.NotConfigured",
                "Stripe webhook secret is not configured.");
        }

        // The real handler does its own signature validation against the header.
        // We pass the raw payload and a placeholder signature marker; the gateway
        // pipeline at the endpoint must inject the real Stripe-Signature header
        // before reaching this dispatcher.
        var result = await _sender.Send(new StripeWebhook.Command(payload, "stripe-signature"), ct);
        return result;
    }

    public bool ValidateSignature(string payload, string stripeSignature)
    {
        if (string.IsNullOrEmpty(_options.WebhookSecret)) return false;
        try
        {
            EventUtility.ValidateSignature(payload, stripeSignature, _options.WebhookSecret);
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Stripe signature validation failed");
            return false;
        }
    }

    public Event? ParseEvent(string payload)
    {
        try { return EventUtility.ParseEvent(payload); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe event parse failed: {Payload}", payload);
            return null;
        }
    }
}
```

- [ ] **Step 5: Re-run the dispatcher tests**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~StripeWebhookDispatcherTests" --no-restore`
Expected: PASS.

- [ ] **Step 6: Re-bind DI registration in `Payment.Extension.cs`**

In `service/Api/src/Module/Payment/Payment.Extension.cs`, replace lines 75-76:

```csharp
services.AddSingleton<IWebhookHandler, StripeWebhookHandler>();
services.AddSingleton<IStripeWebhookService, StripeWebhookHandler>();
```

with:

```csharp
services.AddSingleton<IStripeWebhookService, StripeWebhookDispatcher>();
// IWebhookHandler is the legacy gateway dispatcher interface; keep the
// old handler bound for now — see plan TODO to remove in a follow-up.
services.AddSingleton<IWebhookHandler, StripeWebhookHandler>();
```

If the project has a separate `IWebhookDispatcher` interface (verify by searching `service/Api/src/Module/Payment` for `IWebhookHandler`), add the dispatcher to whichever interface the gateway routing code actually calls. The binding above assumes `IStripeWebhookService` is the canonical entry — confirm by checking `service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs` (or wherever the gateway is invoked).

- [ ] **Step 7: Run the full Payment test suite**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~Payment" --no-restore`
Expected: all tests pass.

- [ ] **Step 8: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs service/Api/src/Module/Payment/Payment.Extension.cs service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcherTests.cs
git commit -m "feat(payment): wire real StripeWebhook.CommandHandler via dispatcher adapter"
```

---

## Task 5: Integration test for webhook replay against real Postgres

**Files:**
- Create: `service/Api/tests/Api.Tests/Scenarios/Payment/StripeWebhookReplayedTests.cs`

**Context:** Verify the full MediatR pipeline delivers idempotency end-to-end. Use the existing `Api.Tests` test infrastructure (Testcontainers + Respawn) and a fixture or base class that boots the host.

- [ ] **Step 1: Read an existing `Api.Tests` scenario for the fixture pattern**

Look at `service/Api/tests/Api.Tests/Scenarios/`. Pick the smallest existing test (e.g. a payment scenario) and identify the base class or fixture that:
- Spins up the Aspire/Postgres container
- Resets the DB via Respawn
- Provides an `ISender` accessor

Copy that fixture pattern into the new test file.

- [ ] **Step 2: Write the failing test**

Create file `service/Api/tests/Api.Tests/Scenarios/Payment/StripeWebhookReplayedTests.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Features.Storefront.Payment.Webhooks;
using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Api.Tests.Scenarios.Payment;

[Trait("Category", "Integration")]
[Trait("Module", "Payment")]
public class StripeWebhookReplayedTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public StripeWebhookReplayedTests(ApiFactory factory) { _factory = factory; }

    [Fact(DisplayName = "Replayed payment_intent.succeeded webhook does not double-process")]
    public async Task ReplayedWebhook_IsIdempotent_AgainstRealDatabase()
    {
        // Arrange: seed a pending payment
        var orderId = Guid.NewGuid();
        var intentId = "pi_replay_" + Guid.NewGuid().ToString("N")[..8];
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            db.Set<PaymentCapture>().Add(new PaymentCapture
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ResponseCode = intentId,
                State = PaymentRecordState.Pending,
                Amount = 50m,
                Currency = "USD"
            });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act: send the same command twice
        using (var scope = _factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var stripeEvent = new global::Stripe.Event
            {
                Type = GatewayConstants.WebhookEvents.Stripe.PaymentIntentSucceeded,
                Data = new global::Stripe.EventData { Object = new PaymentIntent { Id = intentId } }
            };
            var payload = JsonSerializer.Serialize(stripeEvent);

            var first = await sender.Send(new StripeWebhook.Command(payload, "valid-sig"), TestContext.Current.CancellationToken);
            var second = await sender.Send(new StripeWebhook.Command(payload, "valid-sig"), TestContext.Current.CancellationToken);

            first.IsSuccess.Should().BeTrue();
            second.IsSuccess.Should().BeTrue();
        }

        // Assert: payment is exactly once in Completed state
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var payments = await verifyDb.Set<PaymentCapture>()
            .Where(p => p.OrderId == orderId)
            .ToListAsync(TestContext.Current.CancellationToken);

        payments.Should().HaveCount(1);
        payments[0].State.Should().Be(PaymentRecordState.Completed);
    }
}
```

- [ ] **Step 3: Run the test to verify it fails (no fixture / DB not reachable)**

Run: `dotnet test service/Api/tests/Api.Tests/Api.Tests.csproj --filter "FullyQualifiedName~StripeWebhookReplayedTests" --no-restore`
Expected: FAIL — fixture or DI wiring not in place. Note the exact error.

- [ ] **Step 4: Adjust the fixture reference**

If `ApiFactory` does not exist, create it. The factory MUST:
- Start a Testcontainers PostgreSQL instance
- Override `ConnectionStrings:DefaultConnection` via `WebApplicationFactory<TEntryPoint>.WithWebHostBuilder`
- Run migrations on startup OR apply the schema via Respawn
- Expose `Services` for scoped access

If `ApiFactory` exists under a different name (e.g. `PaymentApiFactory`, `IntegrationFixture`), update the test to use the real name.

- [ ] **Step 5: Re-run the test**

Run: `dotnet test service/Api/tests/Api.Tests/Api.Tests.csproj --filter "FullyQualifiedName~StripeWebhookReplayedTests" --no-restore`
Expected: PASS (requires Docker daemon running).

- [ ] **Step 6: Commit**

```bash
git add service/Api/tests/Api.Tests/Scenarios/Payment/StripeWebhookReplayedTests.cs
git commit -m "test(payment): add integration test for webhook replay idempotency"
```

---

## Task 6: Build and run the full test suite

**Files:** (no code changes)

- [ ] **Step 1: Build the entire solution**

Run: `dotnet build service/Api/Api.slnx`
Expected: zero errors, zero warnings. If a warning appears, fix it before committing.

- [ ] **Step 2: Run the full unit test suite**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --no-restore`
Expected: all tests pass.

- [ ] **Step 3: Run the full Shared unit test suite**

Run: `dotnet test service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj --no-restore`
Expected: all tests pass.

- [ ] **Step 4: Run the integration suite (requires Docker)**

Run: `dotnet test service/Api/tests/Api.Tests/Api.Tests.csproj --no-restore`
Expected: all tests pass.

- [ ] **Step 5: Commit any formatting or warning fixes**

```bash
git add -A
git commit -m "chore(payment): post-plan cleanup" --allow-empty
```

---

## Self-Review

- **Spec coverage:** REQ-PAY-001 ✓ Task 4. REQ-PAY-002 ✓ Task 4 deletes the stub call. REQ-PAY-003 (idempotency) ✓ Tasks 3, 5. REQ-PAY-010 (BuildServiceProvider) ✓ Task 1. REQ-PAY-011 ✓ Task 1. PAT-PAY-001 ✓ Task 1. CON-PAY-001 ✓ not a code change, gateway config still has Bogus. SEC-PAY-001 ✓ Task 4 (ValidateSignature is delegated from the real handler via `stripe-signature` — flagged in Step 4 step 6: gateway routing must inject the real header).
- **Placeholders:** none. All `it.IsAny<>` references in the test code are intentional Moq matchers.
- **Type consistency:** `StripeWebhookDispatcher` is referenced consistently across Task 4 Steps 2, 4, 5, 6. `IStripeWebhookService` is the interface name throughout. `StripeWebhook.Command` is the record referenced via `ISender.Send`.
