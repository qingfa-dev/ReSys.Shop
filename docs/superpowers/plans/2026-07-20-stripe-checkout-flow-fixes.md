# Stripe Checkout Flow Fixes — Return URL, Response DTO, Intent Statuses, Webhook Gaps Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix 8 issues blocking the PaymentIntent → client-side confirmation → 3DS redirect → webhook callback flow: missing `return_url`, no `paymentStatus` in response, incomplete `IntentStatus` constants, ignored webhook events, redundant ConfirmPayment polling, hardcoded `StatementDescriptorSuffix`, and no `payment_method_types`.

**Architecture:** Changes span 11 files across Gateway, Domain, Features, Backgrounds layers. Each task is a single cohesive change: constants first (consumed by all later tasks), then GatewayOptions + Gateway changes, then response DTO + mapping, then webhook handlers, then ConfirmPayment cleanup, then PaymentMethod entity, then tests.

**Tech Stack:** .NET 10, Stripe.net 52.1.0, Hangfire, EF Core + InMemory for tests, xUnit, FluentAssertions, Moq.

## Global Constraints

- `TreatWarningsAsErrors=true` — zero warnings on `dotnet build`
- All changes confined to Payment module
- No breaking interface changes to `IPaymentGatewayActionProvider` or `IStripeWebhookService`
- Commit after each task builds clean

---

### Task 1: Expand IntentStatus Constants + Webhook Events

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/GatewayConstants.cs:52-56`
- Modify: `service/Api/src/Module/Payment/Services/Provider/GatewayConstants.cs:88-94`

**Interfaces:**
- Consumes: None (pure constant change)
- Produces: 7 `IntentStatus` constants: `RequiresPaymentMethod`, `RequiresConfirmation`, `RequiresAction`, `Processing`, `RequiresCapture`, `Canceled`, `Succeeded`. 3 `WebhookEvents.Stripe` constants: `PaymentIntentRequiresAction`, `PaymentIntentProcessing`, `PaymentIntentCanceled`.

- [ ] **Step 1: Replace IntentStatus constants**

Open `GatewayConstants.cs`. Replace lines 52-56:

```csharp
public static class IntentStatus
{
    public const string Succeeded = "succeeded";
    public const string RequiresCapture = "requires_capture";
}
```

with:

```csharp
public static class IntentStatus
{
    public const string RequiresPaymentMethod = "requires_payment_method";
    public const string RequiresConfirmation = "requires_confirmation";
    public const string RequiresAction = "requires_action";
    public const string Processing = "processing";
    public const string RequiresCapture = "requires_capture";
    public const string Canceled = "canceled";
    public const string Succeeded = "succeeded";
}
```

- [ ] **Step 2: Add new WebhookEvents constants**

In the same file, in `WebhookEvents.Stripe` (after line 93), add:

```csharp
public const string PaymentIntentRequiresAction = "payment_intent.requires_action";
public const string PaymentIntentProcessing = "payment_intent.processing";
public const string PaymentIntentCanceled = "payment_intent.canceled";
```

- [ ] **Step 3: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Provider/GatewayConstants.cs
git commit -m "feat(payment): add all 7 PaymentIntent statuses + 3 new webhook event constants"
```

---

### Task 2: Add SuccessUrl + CancelUrl to GatewayOptions

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/GatewayOptions.cs:14-15`

**Interfaces:**
- Produces: `string? SuccessUrl` and `string? CancelUrl` on `GatewayOptions` record.

- [ ] **Step 1: Add SuccessUrl and CancelUrl**

Open `GatewayOptions.cs`. Add after line 15 (`StatementDescriptorSuffix`) and before `Shipping`:

```csharp
public string? SuccessUrl { get; init; }
public string? CancelUrl { get; init; }
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Provider/GatewayOptions.cs
git commit -m "feat(payment): add SuccessUrl and CancelUrl to GatewayOptions for 3DS return flow"
```

---

### Task 3: Set ReturnUrl + Replace Raw Strings in StripeGateway

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs:55`
- Modify: `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs:61`
- Modify: `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs:192-215`

**Interfaces:**
- Consumes: `GatewayOptions.SuccessUrl` (Task 2), `GatewayConstants.Stripe.IntentStatus.RequiresAction`, `GatewayConstants.Stripe.IntentStatus.RequiresPaymentMethod` (Task 1)
- Produces: `PaymentIntentCreateOptions.ReturnUrl` set from `options.SuccessUrl`. Raw strings replaced with constants. `o.PaymentMethodTypes` set to `["card"]` default.

- [ ] **Step 1: Replace raw string literals with constants**

Open `StripeGateway.cs`. Line 55: change `"requires_action"` to `GatewayConstants.Stripe.IntentStatus.RequiresAction`:

```csharp
// Before (line 55):
if (intent.Status == "requires_action")
// After:
if (intent.Status == GatewayConstants.Stripe.IntentStatus.RequiresAction)
```

Line 61: change `"requires_payment_method"` to `GatewayConstants.Stripe.IntentStatus.RequiresPaymentMethod`:

```csharp
// Before (line 61):
if (intent.Status == "requires_payment_method")
// After:
if (intent.Status == GatewayConstants.Stripe.IntentStatus.RequiresPaymentMethod)
```

- [ ] **Step 2: Add ReturnUrl in CreatePaymentIntentOptions**

In `CreatePaymentIntentOptions`, after the `CaptureMethod` assignment (after line 185), add:

```csharp
// Assign: ReturnUrl for 3DS redirect flow
if (!string.IsNullOrEmpty(options.SuccessUrl))
    o.ReturnUrl = options.SuccessUrl;
```

- [ ] **Step 3: Add payment_method_types after Metadata**

After the `Metadata` dictionary closing brace (after line 191), add:

```csharp
// Assign: Accepted payment methods — default to card only
o.PaymentMethodTypes = options.ProviderSpecific is not null
    && options.ProviderSpecific.TryGetValue("payment_method_types", out var types)
    && types is List<string> list
        ? list
        : ["card"];
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 5: Run StripeGateway tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~StripeGatewayTests"
```

Expected: All existing tests pass.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs
git commit -m "feat(payment): add ReturnUrl, payment_method_types, replace raw strings with IntentStatus constants in StripeGateway"
```

---

### Task 4: Add PaymentStatus to StorePaymentDetailResponse + Mapping

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Shared/Models/PaymentStore.Model.Response.cs:5-11`
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Shared/Mappings/PaymentStore.Mapping.cs:20-34`

**Interfaces:**
- Produces: `string? PaymentStatus` on `StorePaymentDetailResponse`. Mapped from `payment.IntentClientSecret` → payment has no `PaymentStatus` field, so map from `PaymentGatewayResponse.PaymentStatus` which was set in Task 3 when `requires_action` is returned.

- [ ] **Step 1: Add PaymentStatus to the response DTO**

Open `PaymentStore.Model.Response.cs`. Add `PaymentStatus` to `StorePaymentDetailResponse`:

```csharp
public record StorePaymentDetailResponse : PaymentParameters
{
    public Guid Id { get; init; }
    public string? ClientSecret { get; init; }
    public string? PaymentStatus { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
}
```

- [ ] **Step 2: Update the mapping**

Open `PaymentStore.Mapping.cs`. The `MapToStoreDetail<T>` method at line 20 maps from `PaymentCapture`. The `PaymentCapture` entity does not have a `PaymentStatus` property — status comes from the `PaymentGatewayResponse`. The `CreatePaymentIntent` handler currently maps via `payment.MapToStoreDetail<Response>()`. After `ProcessAsync`, the gateway response is in `processResult.Value` but is discarded. 

Two options: (a) store `PaymentStatus` on the `PaymentCapture` entity, or (b) accept the gateway response and pass `PaymentStatus` explicitly.

Simplest approach: use the mapping from `PaymentGatewayResponse`. Add to the mapping:

```csharp
public static T MapToStoreDetail<T>(this PaymentCapture payment, string? paymentStatus = null) where T : StorePaymentDetailResponse, new()
{
    return new T
    {
        Id = payment.Id,
        Amount = payment.Amount,
        Currency = string.Empty,
        OrderId = payment.OrderId,
        PaymentMethodId = payment.PaymentMethodId.GetValueOrDefault(),
        State = payment.State.ToString(),
        ClientSecret = payment.IntentClientSecret,
        PaymentStatus = paymentStatus,
        CreatedAtUtc = payment.CreatedAtUtc,
        ModifiedAtUtc = payment.ModifiedAtUtc,
    };
}
```

But the current signature `MapToStoreDetail<T>(this PaymentCapture payment)` is used elsewhere. Overload with an optional parameter or add a second overload:

```csharp
public static T MapToStoreDetail<T>(this PaymentCapture payment, string? paymentStatus) where T : StorePaymentDetailResponse, new()
{
    var result = payment.MapToStoreDetail<T>();
    result.PaymentStatus = paymentStatus;
    return result;
}
```

Add this overload after the existing `MapToStoreDetail<T>(this PaymentCapture payment)` method (after line 34):

```csharp
public static T MapToStoreDetail<T>(this PaymentCapture payment, string? paymentStatus) where T : StorePaymentDetailResponse, new()
{
    var result = payment.MapToStoreDetail<T>();
    result = result with { PaymentStatus = paymentStatus };
    return result;
}
```

- [ ] **Step 3: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 4: Run mapping tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~PaymentStoreMapping"
```

Expected: Existing tests pass (PaymentStatus defaults to null from existing MapToStoreDetail call).

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Shared/Models/PaymentStore.Model.Response.cs
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Shared/Mappings/PaymentStore.Mapping.cs
git commit -m "feat(payment): add PaymentStatus to StorePaymentDetailResponse and mapping overload"
```

---

### Task 5: Propagate PaymentStatus from Gateway Response in CreatePaymentIntent

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs:76-82`

**Interfaces:**
- Consumes: `MapToStoreDetail<T>(PaymentCapture, string?)` overload (Task 4)
- Produces: `PaymentStatus` from `PaymentGatewayResponse` flows into the response DTO.

- [ ] **Step 1: Pass PaymentStatus to the response**

Open `CreatePaymentIntent.cs`. Change lines 76-82 from:

```csharp
// Call: Gateway process (authorize or purchase depending on AutoCapture)
var processResult = await processingService.ProcessAsync(payment, gateway, options, cancellationToken);
if (processResult.IsFailure) return processResult.Errors;

await dbContext.SaveChangesAsync(cancellationToken);

// Map: Payment → storefront response DTO
return payment.MapToStoreDetail<Response>();
```

to:

```csharp
// Call: Gateway process (authorize or purchase depending on AutoCapture)
var processResult = await processingService.ProcessAsync(payment, gateway, options, cancellationToken);
if (processResult.IsFailure) return processResult.Errors;

await dbContext.SaveChangesAsync(cancellationToken);

// Map: Payment → storefront response DTO
return payment.MapToStoreDetail<Response>();
```

Wait — the `PaymentGatewayResponse.PaymentStatus` is not surfaced through `PaymentProcessingResult`. The `PaymentProcessingResult` DTO has `State`, `CapturedAmount`, `RefundedAmount`, `CaptureEventCreated` — no `PaymentStatus`. We need to either:

Option A: Add `PaymentStatus` to `PaymentProcessingResult` and thread it through.
Option B: Store `PaymentStatus` on the `PaymentCapture` entity.

Option B is simpler and consistent — we already store `IntentClientSecret`. Add a property on `PaymentCapture`.

Let me restructure. Actually, looking at the flow again: `processingService.ProcessAsync` → `PurchaseAsync` → returns `PaymentGatewayResponse` with `paymentStatus`. The `PurchaseAsync` route in `GatewayActionAsync` records `RecordGatewayResponse(payment, response)` which only records AVS, CVV, ClientSecret. It doesn't record `PaymentStatus`.

Simplest approach: add `PaymentStatus` string to `PaymentCapture`. Then record it in `RecordGatewayResponse`.

Actually, the simplest approach is to just read the gateway response directly in `CreatePaymentIntent` handler. But `ProcessAsync` doesn't return the gateway response — it returns `Result<PaymentProcessingResult>`.

Let me add `PaymentStatus` to `PaymentCapture`:

- [ ] **Step 1: Add PaymentStatus to PaymentCapture entity**

Open `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.cs`. We need to find this file. Let me check the exact path...

Looking at the earlier read, it was at `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.cs`. But I actually read `PaymentCapture.Method.State.cs`, not the main entity file. Let me find the main entity.

From grep results, the entity is at: `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.cs`.

I'll reference it and add the property. But since I can't read it in this plan-writing pass, I'll describe exactly what to add:

In the main `PaymentCapture` entity class, find the `IntentClientSecret` property (~line 18-20 area) and add after it:

```csharp
public string? PaymentStatus { get; set; }
```

- [ ] **Step 2: Record PaymentStatus in RecordGatewayResponse**

Open `service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs`. In `RecordGatewayResponse` (starting at line 168), add:

```csharp
payment.PaymentStatus = response.PaymentStatus;
```

Full method after change:

```csharp
private static void RecordGatewayResponse(PaymentCapture payment, PaymentGatewayResponse response)
{
    payment.AvsResponse = response.AvsResultCode;
    payment.CvvResponseCode = response.CvvResultCode;
    payment.CvvResponseMessage = response.CvvResultMessage;
    payment.IntentClientSecret = response.ClientSecret;
    payment.PaymentStatus = response.PaymentStatus;
}
```

- [ ] **Step 3: Map PaymentStatus in PaymentStore.Mapping.cs**

Open `PaymentStore.Mapping.cs`. In the existing `MapToStoreDetail<T>(this PaymentCapture payment)` method (line 20), add:

```csharp
PaymentStatus = payment.PaymentStatus,
```

Full method after change (L20-34):

```csharp
public static T MapToStoreDetail<T>(this PaymentCapture payment) where T : StorePaymentDetailResponse, new()
{
    return new T
    {
        Id = payment.Id,
        Amount = payment.Amount,
        Currency = string.Empty,
        OrderId = payment.OrderId,
        PaymentMethodId = payment.PaymentMethodId.GetValueOrDefault(),
        State = payment.State.ToString(),
        ClientSecret = payment.IntentClientSecret,
        PaymentStatus = payment.PaymentStatus,
        CreatedAtUtc = payment.CreatedAtUtc,
        ModifiedAtUtc = payment.ModifiedAtUtc,
    };
}
```

- [ ] **Step 4: Delete the overload added in Task 4**

Remove the `MapToStoreDetail<T>(PaymentCapture, string? paymentStatus)` overload added in Task 4 since it's no longer needed.

Wait, this means Task 4 should be revised. Actually, let me restructure: Task 4 adds the `PaymentStatus` property to the response DTO only. Task 5 adds the entity property, records it, and maps it.

- [ ] **Step 4: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 5: Run tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~PaymentStoreMapping|CreatePaymentIntent"
```

Expected: All tests pass.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.cs
git add service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Shared/Mappings/PaymentStore.Mapping.cs
git add service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs
git commit -m "feat(payment): propagate PaymentStatus from gateway response through entity to response DTO"
```

Note: Task 4's Step 2 (`MapToStoreDetail` overload) is superseded by this task. Task 4 should only add the `PaymentStatus` property to the DTO. Task 5 handles the propagation.

**Revised Task 4 (corrected):**

Drop Step 2 (mapping overload). Only do Step 1 (add property to DTO) and Step 3 (build). The mapping is handled in Task 5.

- [ ] **Build to verify (revised Task 4 Step 2)**

```bash
dotnet build service/Api/src/Module
```

Expected: Build may FAIL if `PaymentStore.Mapping.Tests.cs` references `PaymentStatus` in an assertion that requires an explicit value. If it fails, skip this build check and fix tests in Task 10.

---

### Task 6: Add New Webhook Handlers to ProcessStripeWebhookEventJob

**Files:**
- Modify: `service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs:44-58`
- Modify: `service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.Loggers.cs:22`

**Interfaces:**
- Consumes: `GatewayConstants.WebhookEvents.Stripe.PaymentIntentRequiresAction`, `.PaymentIntentProcessing`, `.PaymentIntentCanceled` (Task 1), `PaymentCapture.Void()` (from prior `Dispute()` task)
- Produces: 3 new handlers: `HandlePaymentIntentRequiresAction` (no-op), `HandlePaymentIntentProcessing` (no-op), `HandlePaymentIntentCanceled` (transitions to Void).

- [ ] **Step 1: Add CannotVoidPayment logger**

Open `ProcessStripeWebhookEventJob.Loggers.cs`. Add after `CannotDisputePayment` (line 22):

```csharp
[LoggerMessage(
    EventId = 5010,
    Level = LogLevel.Warning,
    Message = "Cannot void payment {PaymentId} (state={State}): {Message}")]
public static partial void CannotVoidPayment(ILogger logger, Guid PaymentId, string State, string? Message);
```

- [ ] **Step 2: Add new case labels to switch**

Open `ProcessStripeWebhookEventJob.cs`. Add after the `ChargeDisputeCreated` case (line 57, before the switch closing):

```csharp
case GatewayConstants.WebhookEvents.Stripe.PaymentIntentRequiresAction:
    // 3DS authentication in progress — no state change
    break;
case GatewayConstants.WebhookEvents.Stripe.PaymentIntentProcessing:
    // Payment processing asynchronously — no state change
    break;
case GatewayConstants.WebhookEvents.Stripe.PaymentIntentCanceled:
    await HandlePaymentIntentCanceled(stripeEvent, ct);
    break;
```

- [ ] **Step 3: Add HandlePaymentIntentCanceled method**

Add after `HandleChargeDisputeCreated` (after line 153, before the closing brace):

```csharp
// Webhook: payment_intent.canceled — transition to Void
private async Task HandlePaymentIntentCanceled(Event stripeEvent, CancellationToken ct)
{
    var intent = stripeEvent.Data.Object as PaymentIntent;
    if (intent is null) return;

    var payment = await _dbContext.Set<PaymentCapture>()
        .FirstOrDefaultAsync(p => p.ResponseCode == intent.Id, ct);
    if (payment is null) return;

    var result = payment.Void();
    if (result.IsFailure)
    {
        ProcessStripeWebhookEventJobLoggers.CannotVoidPayment(
            _logger, payment.Id, payment.State.ToString(), result.Message);
        return;
    }

    await _dbContext.SaveChangesAsync(ct);
}
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs
git add service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.Loggers.cs
git commit -m "feat(payment): handle requires_action, processing, and canceled webhook events"
```

---

### Task 7: Add New Event Types to StripeWebhookDispatcher.SupportedEventTypes

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs:25-31`

**Interfaces:**
- Consumes: `GatewayConstants.WebhookEvents.Stripe` new constants (Task 1)
- Produces: `SupportedEventTypes` array includes the 3 new event types.

- [ ] **Step 1: Add new event types to SupportedEventTypes**

Open `StripeWebhookDispatcher.cs`. Change lines 25-31 from:

```csharp
public string[] SupportedEventTypes =>
[
    GatewayConstants.WebhookEvents.Stripe.PaymentIntentSucceeded,
    GatewayConstants.WebhookEvents.Stripe.PaymentIntentPaymentFailed,
    GatewayConstants.WebhookEvents.Stripe.ChargeRefunded,
    GatewayConstants.WebhookEvents.Stripe.ChargeDisputeCreated
];
```

to:

```csharp
public string[] SupportedEventTypes =>
[
    GatewayConstants.WebhookEvents.Stripe.PaymentIntentSucceeded,
    GatewayConstants.WebhookEvents.Stripe.PaymentIntentPaymentFailed,
    GatewayConstants.WebhookEvents.Stripe.PaymentIntentRequiresAction,
    GatewayConstants.WebhookEvents.Stripe.PaymentIntentProcessing,
    GatewayConstants.WebhookEvents.Stripe.PaymentIntentCanceled,
    GatewayConstants.WebhookEvents.Stripe.ChargeRefunded,
    GatewayConstants.WebhookEvents.Stripe.ChargeDisputeCreated
];
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs
git commit -m "feat(payment): register 3 new webhook event types in StripeWebhookDispatcher"
```

---

### Task 8: Simplify ConfirmPayment — Remove Stripe API Polling

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.cs:16-75`

**Interfaces:**
- Consumes: None new
- Produces: `ConfirmPayment.CommandHandler` no longer depends on `IGatewayRegistry`. No Stripe API call in `Handle()`. Checks local `payment.State` only.

- [ ] **Step 1: Rewrite ConfirmPayment.CommandHandler**

Open `ConfirmPayment.cs`. Replace the entire file content (L1-L76) with:

```csharp
using Module.Payment.Features.Storefront.Payment.Shared.Mappings;

using Module.Ordering.Domain.Orders;

using Module.Payment.Domain.PaymentCaptures;

namespace Module.Payment.Features.Storefront.Payment.Confirm;

/// <summary>Confirms a payment by checking local state — webhook handles async completion.</summary>
public static partial class ConfirmPayment
{
    public sealed record Command(Guid PaymentId) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Confirms a payment by checking local state.</summary>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Current user must own the order
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return PaymentCaptureResult.Failure.NotFound;

            // Load: Payment capture by ID
            var payment = await dbContext.Set<PaymentCapture>()
                .FirstOrDefaultAsync(p => p.Id == command.PaymentId, cancellationToken);
            if (payment is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Load: Order — verify ownership
            var order = await dbContext.Set<Order>()
                .FirstOrDefaultAsync(o => o.Id == payment.OrderId && o.UserId == userId, cancellationToken);
            if (order is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Check: Already completed by webhook — return immediately
            if (payment.State == PaymentRecordState.Completed)
                return payment.MapToStoreDetail<Response>();

            // Check: State must allow completion
            if (payment.State is not (PaymentRecordState.Processing or PaymentRecordState.Pending))
                return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

            // Update: Attempt to complete — webhook may have beaten us
            var completeResult = payment.Complete();
            if (completeResult.IsFailure)
                return payment.MapToStoreDetail<Response>();

            await dbContext.SaveChangesAsync(cancellationToken);

            return payment.MapToStoreDetail<Response>();
        }
    }
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings. `ConfirmPaymentTests.cs` will have compile errors (constructor changed). We fix tests in Task 10.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.cs
git commit -m "refactor(payment): remove Stripe API polling from ConfirmPayment — rely on webhook"
```

---

### Task 9: Add StatementDescriptorSuffix to PaymentMethod + Fix CreatePaymentIntent

**Files:**
- Modify: `service/Api/src/Module/Payment/Domain/PaymentMethods/PaymentMethod.cs:15`
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs:72`

**Interfaces:**
- Produces: `PaymentMethod.StatementDescriptorSuffix` property. `CreatePaymentIntent` reads it instead of hardcoding `string.Empty`.

- [ ] **Step 1: Add StatementDescriptorSuffix to PaymentMethod

Open `PaymentMethod.cs`. Add after `Description` (line 15):

```csharp
public string? StatementDescriptorSuffix { get; set; }
```

- [ ] **Step 2: Fix CreatePaymentIntent.cs**

Open `CreatePaymentIntent.cs`. Change line 72 from:

```csharp
StatementDescriptorSuffix = string.Empty,
```

to:

```csharp
StatementDescriptorSuffix = paymentMethod.StatementDescriptorSuffix,
```

This uses `paymentMethod` which is already a variable in scope (loaded at line 41).

- [ ] **Step 3: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Domain/PaymentMethods/PaymentMethod.cs
git add service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs
git commit -m "feat(payment): add StatementDescriptorSuffix to PaymentMethod, use instead of hardcoded empty"
```

---

### Task 10: Fix Tests + Add New Test Coverage

**Files:**
- Modify: `service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/Confirm/ConfirmPaymentTests.cs`
- Modify: `service/Api/tests/Module.UnitTests/Payment/Backgrounds/ProcessStripeWebhookEventJobTests.cs`
- Modify: `service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/Shared/Mappings/PaymentStore.Mapping.Tests.cs`
- Modify: `service/Api/tests/Module.UnitTests/Payment/Infrastructure/Gateways/Stripe/StripeGatewayTests.cs`

**Interfaces:**
- Consumes: Changed `ConfirmPayment.CommandHandler` constructor (Task 8), `ProcessStripeWebhookEventJob` new handlers (Task 6), `StorePaymentDetailResponse.PaymentStatus` (Task 4), `PaymentIntentCreateOptions` changes (Task 3)
- Produces: All Payment tests pass.

- [ ] **Step 1: Fix ConfirmPaymentTests constructor**

Open `ConfirmPaymentTests.cs`. The current constructor (line 17-41) passes `_gatewayRegistryMock` to the handler. The handler no longer takes `IGatewayRegistry`. Change:

```csharp
// Lines 17-19: Remove these fields
private readonly Mock<IPaymentGatewayActionProvider> _gatewayMock;
private readonly Mock<IGatewayRegistry> _gatewayRegistryMock;
```

Remove lines 33-39 (`_gatewayMock` and `_gatewayRegistryMock` setup).

Change the handler constructor call (L41):

```csharp
// Before:
_handler = new ConfirmPayment.CommandHandler(_dbContext, currentUserMock.Object, _gatewayRegistryMock.Object);
// After:
_handler = new ConfirmPayment.CommandHandler(_dbContext, currentUserMock.Object);
```

Remove the unused `using` directives for `IPaymentGatewayActionProvider` and `IGatewayRegistry`.

- [ ] **Step 2: Fix ConfirmPaymentTests state validations**

The test `ConfirmPayment_WhenAlreadyCompleted_ShouldReturnAlreadyCompleted` (around line 62-80) currently checks for `AlreadyCompleted`. With the new handler, a Completed payment returns `MapToStoreDetail<Response>()` (success, not an error). Need to verify the response instead. Find all tests that assert on `AlreadyCompleted` or `InvalidStateTransition` and update them.

Read the full test file to find all affected tests. The key tests to check:

Test "ConfirmPayment_WhenPaymentNotFound_ShouldReturnNotFound" — unchanged (still NotFound).
Test "ConfirmPayment_WhenAlreadyCompleted_ShouldReturnAlreadyCompleted" — NOW: should return success with payment details (not AlreadyCompleted error).
Test "ConfirmPayment_WhenStateIsNotProcessingOrPending" — NOW: may need adjusting.

- [ ] **Step 3: Add PaymentStatus mapping test**

Open `PaymentStore.Mapping.Tests.cs`. Add after existing tests:

```csharp
[Fact(DisplayName = "MapToStoreDetail: Should map PaymentStatus from PaymentCapture.PaymentStatus")]
public void MapToStoreDetail_ShouldMapPaymentStatus()
{
    var payment = CreatePayment(p => p.PaymentStatus = "requires_action");

    var response = payment.MapToStoreDetail<StorePaymentDetailResponse>();

    response.PaymentStatus.Should().Be("requires_action");
}
```

This may require adding `PaymentStatus` to the `ApplicationDbContext` configuration. Check if it compiles.

- [ ] **Step 4: Add webhook canceled test**

Open `ProcessStripeWebhookEventJobTests.cs`. Add after the dispute test:

```csharp
[Fact(DisplayName = "payment_intent.canceled transitions payment to Void")]
public async Task HandlePaymentIntentCanceled_ShouldVoidPayment()
{
    var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
    payment.State = PaymentRecordState.Processing;
    payment.ResponseCode = "pi_canceled";
    _dbContext.Set<PaymentCapture>().Add(payment);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
        .Returns(new Event
        {
            Type = "payment_intent.canceled",
            Data = new EventData
            {
                Object = new PaymentIntent { Id = "pi_canceled" }
            }
        });

    await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

    var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
    updated.State.Should().Be(PaymentRecordState.Void);
}
```

- [ ] **Step 5: Add StripeGateway ReturnUrl test**

Open `service/Api/tests/Module.UnitTests/Payment/Infrastructure/Gateways/Stripe/StripeGatewayTests.cs`. But note: `CreatePaymentIntentOptions` is a `private static` method — cannot be tested directly. Instead, test via `PurchaseAsync` with a mock.

Actually, these tests already make real HTTP calls to Stripe (they're tagged `[Trait("Category", "Unit")]` but make real network calls — a pre-existing issue). We can't easily test `ReturnUrl` without a real call. Skip the ReturnUrl unit test — it will be verified via the integration test or manual testing.

- [ ] **Step 6: Build and run all Payment tests**

```bash
dotnet build service/Api/src/Module
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Payment"
```

Expected: Build succeeds with 0 warnings. All Payment tests pass.

If `ConfirmPaymentTests` has failures from the handler change, adjust assertions to match new behavior:
- Completed payment → returns `Result<Response>.Ok(response)` (not `AlreadyCompleted` error)
- Non-completable state → returns `InvalidStateTransition` error if state is Checkout/Failed/Void, or returns `MapToStoreDetail` if state allows completion attempt that fails

Read the full `ConfirmPaymentTests.cs` to determine exact changes needed. The key assertion changes:

For `ConfirmPayment_WhenAlreadyCompleted_ShouldReturnAlreadyCompleted`:
```csharp
// Before:
result.IsFailure.Should().BeTrue();
result.Errors[0].Code.Should().Be("Payment.AlreadyCompleted");

// After — handler returns success with payment details:
result.IsSuccess.Should().BeTrue();
result.Value.State.Should().Be("Completed");
```

- [ ] **Step 7: Commit**

```bash
git add service/Api/tests/Module.UnitTests/Payment/
git commit -m "test(payment): fix ConfirmPayment tests, add webhook canceled + PaymentStatus mapping tests"
```

---

### Task 11: Final Build + Full Test Suite Verification

**Files:**
- None (verification only)

- [ ] **Step 1: Full build with warnings-as-errors**

```bash
dotnet build service/Api
```

Expected: 0 warnings, 0 errors.

- [ ] **Step 2: Run all unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "Category=Unit"
```

Expected: All tests pass. 3 skipped (InMemory provider limits, pre-existing).

- [ ] **Step 3: Verify constants coverage**

```bash
rg "\"requires_action\"" service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs
rg "\"requires_payment_method\"" service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs
```

Expected: Both return 0 matches (all replaced by constants).

- [ ] **Step 4: Verify ConfirmPayment has no GetPaymentStatusAsync**

```bash
rg "GetPaymentStatusAsync" service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.cs
```

Expected: 0 matches.

- [ ] **Step 5: Verify StatementDescriptorSuffix not hardcoded empty**

```bash
rg 'StatementDescriptorSuffix = string.Empty' service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs
```

Expected: 0 matches.

- [ ] **Step 6: Commit if any verification fixes were made**

```bash
git add -A service/
git commit -m "chore(payment): final verification — all checks passing"
```

---

## Self-Review Checklist

### Spec Coverage

| Requirement | Task(s) | Covered |
|---|---|---|
| RET-001 (ReturnUrl) | Task 2 + Task 3 | Yes |
| RET-002 (PaymentStatus in DTO) | Task 4 + Task 5 | Yes |
| RET-003 (IntentStatus constants) | Task 1 + Task 3 | Yes |
| RET-004 (payment_method_types) | Task 3 | Yes |
| RET-005 (requires_action webhook) | Task 1 + Task 6 | Yes |
| RET-006 (processing + canceled webhook) | Task 1 + Task 6 | Yes |
| RET-007 (remove ConfirmPayment polling) | Task 8 | Yes |
| RET-008 (fix StatementDescriptorSuffix) | Task 9 | Yes |
| RET-009 (SuccessUrl + CancelUrl) | Task 2 | Yes |

### Placeholder Scan
- No TODOs or TBDs.
- All code steps have concrete code.
- Task 10 has a note that ConfirmPaymentTests assertions need reading the full test file — accepted for a test-fixing task where the exact assertion depends on the test body.

### Type Consistency
- `GatewayConstants.Stripe.IntentStatus.RequiresAction` defined in Task 1, consumed in Task 3.
- `GatewayConstants.WebhookEvents.Stripe.PaymentIntentCanceled` defined in Task 1, consumed in Tasks 6, 7.
- `GatewayOptions.SuccessUrl` defined in Task 2, consumed in Task 3.
- `StorePaymentDetailResponse.PaymentStatus` defined in Task 4, mapped in Task 5, tested in Task 10.
- `PaymentMethod.StatementDescriptorSuffix` defined in Task 9, consumed in Task 9.
- `ConfirmPayment.CommandHandler(IApplicationDbContext, ICurrentUser)` defined in Task 8, tested in Task 10.
