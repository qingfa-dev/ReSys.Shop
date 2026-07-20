---
title: Stripe Checkout Flow Fixes — Return URL, Response DTO, Intent Statuses, and Webhook Gaps
version: 1.0
date_created: 2026-07-20
owner: Platform Team
tags: design, payment, stripe, checkout, 3ds, webhook, sre
---

# Introduction

Fixes 8 issues discovered in the third round of Stripe integration review. The core gap: the PaymentIntent creation flow does not support 3DS redirect (`return_url` missing), the response DTO doesn't carry `paymentStatus` for the frontend, the `IntentStatus` constants are incomplete, the webhook handler ignores critical event types, `ConfirmPayment` redundantly polls Stripe, and `StatementDescriptorSuffix` is hardcoded to empty. All fixes confined to the Payment module.

## 1. Purpose & Scope

**Purpose**: Define exact, verifiable changes to make the PaymentIntent → client-side confirmation → 3DS redirect → webhook callback flow production-ready for real customers using cards that require Strong Customer Authentication (SCA/3DS).

**Scope**: Payment module only. Affects `StripeGateway`, `GatewayConstants`, `CreatePaymentIntent`, `StorePaymentDetailResponse`, `ProcessStripeWebhookEventJob`, `ConfirmPayment`, `GatewayOptions`.

**Out of scope**: Stripe Checkout Sessions (hosted page), Apple Pay / Google Pay integration, multi-currency support.

**Assumptions**: The frontend uses Stripe.js `confirmCardPayment(clientSecret)` or `handleCardAction(clientSecret)` for client-side confirmation. Stripe test keys (`sk_test_51OARwy...`) are configured. The webhook handler receives Stripe events at `POST /api/payments/stripe/webhook`.

## 2. Definitions

| Term | Definition |
|---|---|
| return_url | The URL Stripe redirects the customer to after completing 3DS authentication. Must be an HTTPS URL on the merchant's domain. |
| client_secret | The `PaymentIntent.client_secret` — a one-time token the frontend passes to `stripe.confirmCardPayment()` to finalize payment and trigger 3DS if required. |
| payment_method_types | Stripe PaymentIntent parameter listing accepted payment methods (e.g., `["card", "link", "us_bank_account"]`). If omitted, Stripe defaults to `["card"]` only. |
| 3DS redirect flow | User clicks pay → frontend calls `confirmCardPayment()` → Stripe detects SCA required → redirects user to bank's 3DS page → user authenticates → redirected back to `return_url` → Stripe webhook fires `payment_intent.succeeded`. |

## 3. Requirements, Constraints & Guidelines

### RET-001: Set ReturnUrl on PaymentIntentCreateOptions

`StripeGateway.CreatePaymentIntentOptions` MUST set `o.ReturnUrl` to a configured success URL when the gateway is used in a web-based checkout flow. The URL MUST be passed via `GatewayOptions` as a new property `SuccessUrl`.

**Constraint**: `SuccessUrl` is required only for card payments with `ConfirmationMethod=Manual`. Bank transfers and other non-redirect payment methods do not need it.

### RET-002: Add paymentStatus to StorePaymentDetailResponse

`StorePaymentDetailResponse` MUST include a `string? PaymentStatus` property. When `PurchaseAsync` returns `"requires_action"`, the response payload MUST contain `"paymentStatus": "requires_action"` so the frontend can conditionally call `stripe.handleCardAction()` or `stripe.confirmCardPayment()`.

### RET-003: Add missing IntentStatus constants

`GatewayConstants.Stripe.IntentStatus` MUST define all PaymentIntent statuses returned by the Stripe API: `requires_payment_method`, `requires_confirmation`, `requires_action`, `processing`, `requires_capture`, `canceled`, `succeeded`. These constants replace raw string literals currently used at `StripeGateway.cs:55,61`.

### RET-004: Add payment_method_types to PaymentIntentCreateOptions

`StripeGateway.CreatePaymentIntentOptions` MUST set `o.PaymentMethodTypes` to a configurable list. The default MUST be `["card"]` unless overridden via `GatewayOptions.ProviderSpecific["payment_method_types"]`.

### RET-005: Handle requires_action in webhook job

`ProcessStripeWebhookEventJob` MUST handle `payment_intent.requires_action` events by leaving the payment in its current state and logging an informational message. This prevents a `requires_action` event from falling through the switch with no handling.

### RET-006: Handle payment_intent.processing and payment_intent.canceled in webhook job

`ProcessStripeWebhookEventJob` MUST handle:
- `payment_intent.processing` — log informational, leave state unchanged (payment is being processed asynchronously)
- `payment_intent.canceled` — transition payment to `Void` state via `payment.Void()`

### RET-007: Remove redundant Stripe API call from ConfirmPayment

`ConfirmPayment.CommandHandler` MUST NOT call `gateway.GetPaymentStatusAsync()`. Instead, it MUST only check the local `payment.State`. If `State == Completed` (webhook already processed), return success. If `State == Pending` or `Processing`, return a status indicating the payment is still processing. The webhook handler is the authoritative source of payment completion.

**Rationale**: The existing webhook handler already transitions state. Polling Stripe synchronously in the confirm endpoint creates an unnecessary API call, races with the webhook, and adds latency.

### RET-008: Set StatementDescriptorSuffix from GatewayOptions, not hardcoded empty

`CreatePaymentIntent.cs` MUST NOT set `StatementDescriptorSuffix = string.Empty`. Instead, it MUST read a value from the order context or payment method configuration. If no value is configured, omitting the property entirely is acceptable (Stripe will use the default merchant descriptor).

### RET-009: Add SuccessUrl to GatewayOptions

`GatewayOptions` MUST include a `string? SuccessUrl` property and a `string? CancelUrl` property for the 3DS return flow.

## 4. Interfaces & Data Contracts

### 4.1 RET-001/009: GatewayOptions additions + ReturnUrl

```csharp
// GatewayOptions.cs — add before Shipping (after L15)
public string? SuccessUrl { get; init; }
public string? CancelUrl { get; init; }
```

```csharp
// StripeGateway.cs — inside CreatePaymentIntentOptions, after CaptureMethod assignment
if (!string.IsNullOrEmpty(options.SuccessUrl))
    o.ReturnUrl = options.SuccessUrl;
```

### 4.2 RET-002: StorePaymentDetailResponse addition

```csharp
// PaymentStore.Model.Response.cs
public record StorePaymentDetailResponse : PaymentParameters
{
    public Guid Id { get; init; }
    public string? ClientSecret { get; init; }
    public string? PaymentStatus { get; init; }  // ADD
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
}
```

Mapping: `PaymentStore.Mapping.cs` — add `PaymentStatus = src.PaymentStatus ?? ...` or map from gateway response.

### 4.3 RET-003: IntentStatus constants

```csharp
// GatewayConstants.cs — replace Stripe.IntentStatus class (L52-56)
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

Replace all raw string literals in `StripeGateway.cs`:
- L55: `"requires_action"` → `GatewayConstants.Stripe.IntentStatus.RequiresAction`
- L61: `"requires_payment_method"` → `GatewayConstants.Stripe.IntentStatus.RequiresPaymentMethod`

### 4.4 RET-004: payment_method_types

```csharp
// StripeGateway.cs — after CaptureMethod assignment in CreatePaymentIntentOptions
o.PaymentMethodTypes = options.ProviderSpecific is not null
    && options.ProviderSpecific.TryGetValue("payment_method_types", out var types)
    && types is List<string> list
        ? list
        : ["card"];
```

### 4.5 RET-005/006: Webhook event type additions

```csharp
// GatewayConstants.cs — add to WebhookEvents.Stripe
public const string PaymentIntentRequiresAction = "payment_intent.requires_action";
public const string PaymentIntentProcessing = "payment_intent.processing";
public const string PaymentIntentCanceled = "payment_intent.canceled";
```

```csharp
// ProcessStripeWebhookEventJob.cs — add to switch statement
case GatewayConstants.WebhookEvents.Stripe.PaymentIntentRequiresAction:
    HandlePaymentIntentRequiresAction(stripeEvent);
    break;
case GatewayConstants.WebhookEvents.Stripe.PaymentIntentProcessing:
    HandlePaymentIntentProcessing(stripeEvent);
    break;
case GatewayConstants.WebhookEvents.Stripe.PaymentIntentCanceled:
    await HandlePaymentIntentCanceled(stripeEvent, ct);
    break;
```

New handler methods:

```csharp
private static void HandlePaymentIntentRequiresAction(Event stripeEvent)
{
    // 3DS authentication in progress — no state change, just ack
}

private static void HandlePaymentIntentProcessing(Event stripeEvent)
{
    // Payment is processing asynchronously (e.g., bank transfer) — no state change
}

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

Add new logger for void failures:

```csharp
// ProcessStripeWebhookEventJob.Loggers.cs
[LoggerMessage(
    EventId = 5010,
    Level = LogLevel.Warning,
    Message = "Cannot void payment {PaymentId} (state={State}): {Message}")]
public static partial void CannotVoidPayment(ILogger logger, Guid PaymentId, string State, string? Message);
```

Add new supported event types in `StripeWebhookDispatcher`:

```csharp
// StripeWebhookDispatcher.cs — SupportedEventTypes array
GatewayConstants.WebhookEvents.Stripe.PaymentIntentRequiresAction,
GatewayConstants.WebhookEvents.Stripe.PaymentIntentProcessing,
GatewayConstants.WebhookEvents.Stripe.PaymentIntentCanceled,
```

### 4.6 RET-007: ConfirmPayment simplification

```csharp
// ConfirmPayment.cs — replace L55-62
// BEFORE:
// Check: Response code (PaymentIntent ID) required for status check
if (string.IsNullOrEmpty(payment.ResponseCode))
    return PaymentCaptureResult.Failure.NotSucceeded;

// Call: Gateway status API — verify Stripe PaymentIntent succeeded
var status = await gateway.GetPaymentStatusAsync(payment.ResponseCode, cancellationToken);
if (status != GatewayConstants.Stripe.IntentStatus.Succeeded)
    return PaymentCaptureResult.Failure.NotSucceeded;

// Update: Transition payment to Completed
var completeResult = payment.Complete();

// AFTER:
// Check: Webhook may have already completed the payment
if (payment.State == PaymentRecordState.Completed)
    return payment.MapToStoreDetail<Response>();

// Check: Payment must be in Processing or Pending state to confirm
if (!string.IsNullOrEmpty(payment.ResponseCode))
{
    // Update: Attempt to complete — will succeed only if state allows
    var completeResult = payment.Complete();
    if (completeResult.IsFailure)
    {
        // Return current state as-is — webhook may process later
        return payment.MapToStoreDetail<Response>();
    }
    payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
    await dbContext.SaveChangesAsync(cancellationToken);
}

// Map: Payment → storefront response DTO
return payment.MapToStoreDetail<Response>();
```

Also remove the `IGatewayRegistry` dependency from the handler constructor since it's no longer used.

### 4.7 RET-008: Fix hardcoded StatementDescriptorSuffix

```csharp
// CreatePaymentIntent.cs L72 — BEFORE:
StatementDescriptorSuffix = string.Empty,

// AFTER:
StatementDescriptorSuffix = paymentMethod.StatementDescriptorSuffix,
```

Add `StatementDescriptorSuffix` property to PaymentMethod entity:

```csharp
// PaymentMethod — add property
public string? StatementDescriptorSuffix { get; set; }
```

## 5. Acceptance Criteria

- **AC-001**: Given a PaymentIntent creation for a card payment, When `CreatePaymentIntentOptions` runs, Then `PaymentIntentCreateOptions.ReturnUrl` is set to the value from `GatewayOptions.SuccessUrl`.

- **AC-002**: Given a PaymentIntent created with `SuccessUrl = "https://shop.example.com/checkout/complete"`, When `PurchaseAsync` returns `requires_action`, Then the response DTO contains `"paymentStatus": "requires_action"` and `"clientSecret": "pi_xxx_secret_yyy"`.

- **AC-003**: Given `rg "\"requires_action\"" service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs`, Then 0 matches (all replaced by `GatewayConstants.Stripe.IntentStatus.RequiresAction`).

- **AC-004**: Given `PaymentIntentCreateOptions` is created with no `ProviderSpecific["payment_method_types"]`, Then `PaymentMethodTypes` defaults to `["card"]`.

- **AC-005**: Given a `payment_intent.canceled` webhook event, When the background job processes it, Then the payment transitions to `Void` state.

- **AC-006**: Given `POST /api/storefront/payment/confirm/{paymentId}` when the payment is already Completed (via webhook), Then the endpoint returns 200 with the payment details and makes zero outbound API calls to Stripe.

- **AC-007**: Given `rg "StatementDescriptorSuffix = string.Empty" service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs`, Then 0 matches.

- **AC-008**: Given `rg "GatewayConstants.Stripe.ConfirmationMethod.Manual"` matches, Then `IntentStatus` constants include all 7 statuses (`RequiresPaymentMethod`, `RequiresConfirmation`, `RequiresAction`, `Processing`, `RequiresCapture`, `Canceled`, `Succeeded`).

- **AC-009**: `dotnet build service/Api/src/Module` succeeds with 0 warnings.

- **AC-010**: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Payment"` passes all Payment tests.

## 6. Test Automation Strategy

- **Test Levels**: Unit tests for new constants, webhook handlers (canceled), ConfirmPayment behavior.
- **Frameworks**: xUnit, FluentAssertions, Moq.
- **Test updates required**:
  - `StripeGatewayTests.cs`: Test that `ReturnUrl` is set when `GatewayOptions.SuccessUrl` is provided.
  - `ProcessStripeWebhookEventJobTests.cs`: Add test for `payment_intent.canceled` → `Void` state. Add test for `payment_intent.processing` → no state change.
  - `ConfirmPaymentTests.cs` (new or existing): Test that confirm on already-Completed payment returns the payment without gateway calls. Test that confirm on Processing payment returns the payment as-is.
  - `PaymentStore.Mapping.cs` tests: Verify `PaymentStatus` is mapped.
- **CI/CD Integration**: `dotnet test --filter "Category=Unit"` runs in GitHub Actions.
- **Coverage Requirements**: All new error paths and webhook event types must have unit test coverage.

## 7. Rationale & Context

### RET-001: Why ReturnUrl is required

Stripe requires a `return_url` on PaymentIntents when `ConfirmationMethod=Manual` and the payment method requires customer action (3DS, bank redirect). Without it, `stripe.confirmCardPayment()` throws a client-side error: "You must provide a `return_url` when confirming a PaymentIntent with the manual confirmation method." This means every card payment that triggers 3DS will fail at the frontend.

### RET-007: Why remove the polling call

The existing flow is:
1. Create PaymentIntent → returns `clientSecret` to frontend
2. Frontend calls `stripe.confirmCardPayment()` → Stripe processes → fires webhook
3. Webhook handler transitions payment to Completed
4. Frontend calls `POST /confirm/{paymentId}` → polls Stripe API again → transitions to Completed (redundant)

The webhook is asynchronous by design. Polling Stripe in step 4 adds latency (200-500ms), burns an API call, and races with the webhook. Since the webhook is the authoritative source, the confirm endpoint should only check local state and return.

### RET-005/006: Why missing webhook events matter

Stripe sends `payment_intent.processing` for delayed payment methods (bank transfers, SEPA). If ignored, the payment stays in `Processing`/`Pending` indefinitely. `payment_intent.canceled` fires when the PaymentIntent is canceled (e.g., 3DS timeout). If ignored, the payment stays in `Processing`/`Pending` forever — the order can never be released. `payment_intent.requires_action` fires when 3DS is triggered — if ignored, no harm but it's a gap in observability.

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: Stripe API v2024+ — `ReturnUrl`, `PaymentMethodTypes`, and all 7 PaymentIntent statuses are standard in Stripe.net 52.1.0.

### Third-Party Services
- None new.

### Infrastructure Dependencies
- None.

### Data Dependencies
- **DAT-001**: `PaymentMethod` entity needs a new `StatementDescriptorSuffix` property for RET-008. Requires an EF Core migration.

### Technology Platform Dependencies
- **PLT-001**: .NET 10, Stripe.net 52.1.0 — no version changes needed.

### Compliance Dependencies
- **SEC-001**: PSD2/SCA — 3DS redirect requires `return_url` as mandated by Stripe. Non-compliance results in declined payments for all SCA-required cards in EEA/UK.

## 9. Examples & Edge Cases

### 3DS redirect flow (fixed)

```
Given: Customer checks out with a card requiring 3DS
When: POST /api/storefront/payment/create-intent
Then: Server creates PaymentIntent with ReturnUrl="https://shop.example.com/checkout/complete"
  and returns { "clientSecret": "pi_xxx_secret_yyy", "paymentStatus": "requires_action" }

When: Frontend calls stripe.confirmCardPayment(clientSecret, {
       return_url: "https://shop.example.com/checkout/complete"
     })
Then: User is redirected to bank's 3DS page → authenticates → redirected back to shop

When: Stripe fires payment_intent.succeeded webhook
Then: Background job transitions payment to Completed

When: Frontend calls POST /api/storefront/payment/confirm/{paymentId}
Then: Server checks payment.State == Completed → returns 200 immediately (no Stripe API call)
```

### Canceled PaymentIntent (new)

```
Given: PaymentIntent is in requires_action state (3DS pending)
And: Customer abandons the 3DS page (timeout)
When: Stripe fires payment_intent.canceled webhook after 24h
Then: Background job transitions payment to Void
And: Order can be released from "awaiting payment" state
```

### No ReturnUrl configured

```
Given: GatewayOptions.SuccessUrl is null (not configured)
When: CreatePaymentIntentOptions runs
Then: PaymentIntentCreateOptions.ReturnUrl is not set
And: For non-card payments (bank transfers, etc.), this is correct behavior
```

## 10. Validation Criteria

- **VC-001**: `dotnet build service/Api/src/Module` passes with 0 warnings.
- **VC-002**: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Payment"` passes.
- **VC-003**: `rg "\"requires_action\"" service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs` returns 0 matches.
- **VC-004**: `rg "\"requires_payment_method\"" service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs` returns 0 matches.
- **VC-005**: `rg "SuccessUrl" service/Api/src/Module/Payment/Services/Provider/GatewayOptions.cs` returns 1 match.
- **VC-006**: `rg "PaymentStatus" service/Api/src/Module/Payment/Features/Storefront/Payment/Shared/Models/PaymentStore.Model.Response.cs` returns 1 match.
- **VC-007**: `rg "GetPaymentStatusAsync" service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.cs` returns 0 matches.
- **VC-008**: `rg "StatementDescriptorSuffix = string.Empty" service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs` returns 0 matches.
- **VC-009**: `rg "IntentStatus" service/Api/src/Module/Payment/Services/Provider/GatewayConstants.cs` shows 7 sub-constants (RequiresPaymentMethod through Succeeded).
- **VC-010**: `rg "payment_intent.canceled" service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs` returns 1 match (in switch case).

## 11. Related Specifications / Further Reading

- [spec-design-stripe-integration-completion.md](/spec/spec-design-stripe-integration-completion.md) — Prior round (3DS, transient errors, dispute, hardening)
- [spec-design-stripe-integration-fixes.md](/spec/spec-design-stripe-integration-fixes.md) — First bugfix round (signature, validator, cleanup)
- [Stripe: PaymentIntent confirmation](https://docs.stripe.com/api/payment_intents/confirm) — Confirm vs Create + Confirm flow
- [Stripe: Handle 3DS with confirmCardPayment](https://docs.stripe.com/payments/3d-secure#handle-3d-secure-with-stripejs) — Client-side flow
- [Stripe: PaymentIntent statuses](https://docs.stripe.com/api/payment_intents/object#payment_intent_object-status) — All status values
- [StripeGateway.cs](/service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs)
- [ConfirmPayment.cs](/service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.cs)
