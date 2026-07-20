---
title: Stripe Integration Completion — 3DS, Resilience, Observability, and Hardening
version: 1.0
date_created: 2026-07-20
owner: Platform Team
tags: design, payment, stripe, 3ds, resilience, hardening, sre
---

# Introduction

Addresses 13 findings from a full-pipeline code review of the Stripe payment integration. The integration covers the complete lifecycle (create intent → capture/authorize → webhook confirmation → refund/void), but has gaps in: 3DS/SCA handling, transient error resilience, statement descriptor passthrough, webhook retry, dispute handling, DB indexing, and startup validation. All changes confined to the Payment module.

## 1. Purpose & Scope

**Purpose**: Define exact, verifiable changes to close the remaining functional and resilience gaps in the Stripe integration so it is production-ready for real payment processing.

**Scope**: Payment module only. Affects `StripeGateway`, `StripeWebhookDispatcher`, `ProcessStripeWebhookEventJob`, `Gateway`, `PaymentProcessingService`, DB schema (index), and DI registration.

**Out of scope**: Gateway abstraction redesign, multi-currency support, new payment methods (iDEAL, SEPA, etc.), frontend checkout UI changes.

**Assumptions**: Stripe test keys (`sk_test_51OARwy...`) are available and the Stripe sandbox can be reached from the development environment. The `appsettings.Development.json` currently enables only the Bogus gateway — Stripe will be enabled via configuration for integration testing.

## 2. Definitions

| Term | Definition |
|---|---|
| SCA / 3DS | Strong Customer Authentication / 3D Secure — regulatory requirement under PSD2 where the card issuer requires an additional authentication step. Stripe PaymentIntent transitions to `requires_action` status. |
| Transient error | A temporary failure (network timeout, Stripe API 5xx) that may succeed on retry. Distinguished from terminal errors (invalid card, insufficient funds). |
| Statement descriptor | The text shown on a customer's credit card statement. Stripe limits: 22 chars, must contain at least one letter. |
| Idempotency | A Stripe API request can be safely retried with the same `Idempotency-Key` header and will not produce duplicate effects. |
| Webhook replay | Stripe may resend the same webhook event if the endpoint does not respond with 2xx quickly enough. Handlers must be idempotent. |

## 3. Requirements, Constraints & Guidelines

### Production Readiness

- **PRD-001**: `StripeGateway.PurchaseAsync` MUST handle `requires_action` (3DS) status by returning `client_secret` to the frontend instead of failing.

- **PRD-002**: `StripeGateway.PurchaseAsync` MUST handle `requires_payment_method` status by returning a clear error indicating the payment method needs re-entry.

- **PRD-003**: `StripeGateway` MUST distinguish transient `StripeException` (HTTP 5xx, `StripeErrorType.ApiError`, `StripeErrorType.ApiConnectionError`) from terminal errors (card declines). Transient errors MUST surface as retryable `Result.Failure` with distinct error code.

- **PRD-004**: `StripeGateway.CreatePaymentIntentOptions` MUST pass `StatementDescriptorSuffix` from `GatewayOptions` to the Stripe `PaymentIntentCreateOptions`. If `StatementDescriptorSuffix` is null/empty, omit it.

- **PRD-005**: `StripeGateway.CreatePaymentIntentOptions` MUST pass `Shipping` to `PaymentIntentCreateOptions.Shipping` when present in `GatewayOptions`.

- **PRD-006**: `StripeGateway.CreatePaymentIntentOptions` MUST apply `checked {}` around the `(long)Math.Round(amount * CentsMultiplier)` conversion to throw `OverflowException` instead of silently wrapping.

### Resilience & Operations

- **RES-007**: `ProcessStripeWebhookEventJob` MUST be decorated with `[AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]` so transient DB outages do not drop webhooks silently.

- **RES-008**: The `PaymentCapture` table MUST have a non-unique index on `ResponseCode` to support webhook lookups and gateway status checks without sequential scans.

- **RES-009**: `StripeWebhookDispatcher.ValidateSignature` MUST NOT silently return `false` when `WebhookSecret` is empty. A startup validation (via `IValidateOptions<StripeSetting>`) MUST fail application startup if `Enabled == true` and `WebhookSecret` is null/empty.

### Domain Integrity

- **DOM-010**: `HandleChargeDisputeCreated` in `ProcessStripeWebhookEventJob` MUST transition the payment to a `Disputed` state (or log at `Critical` level with an alertable event ID) and prevent subsequent refund operations on the disputed payment.

- **DOM-011**: `Gateway.GetPaymentStatusAsync` MUST be made abstract. The current default return `"succeeded"` is a landmine for any new gateway implementation that forgets to override it.

### Code Quality

- **QLT-012**: `StripeWebhookDispatcher.ParseEvent` MUST catch `StripeException` specifically instead of `Exception`.

- **QLT-013**: `PaymentIntentService`, `RefundService`, and `SetupIntentService` in `StripeGateway` MUST be moved to private readonly fields (or constructor-injected) instead of `new` per call. All Stripe SDK service classes are thread-safe.

## 4. Interfaces & Data Contracts

### 4.1 PRD-001/002: Handle 3DS and payment-method-reentry

```csharp
// StripeGateway.PurchaseAsync — after L46
if (intent.Status == GatewayConstants.Stripe.IntentStatus.Succeeded)
    return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe,
        authorization: intent.Id,
        clientSecret: intent.ClientSecret);

// ADD:
if (intent.Status == "requires_action")
    return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe,
        authorization: intent.Id,
        clientSecret: intent.ClientSecret,
        paymentStatus: "requires_action");

if (intent.Status == "requires_payment_method")
    return StripeGatewayResult.Errors.PaymentMethodRequired(intent.LastPaymentError?.Message ?? "Payment method declined");

return StripeGatewayResult.Errors.PurchaseNotSucceeded(intent.Status);
```

New error factory in `StripeGateway.Result.cs`:

```csharp
public static Result<PaymentGatewayResponse> PaymentMethodRequired(string message) =>
    Result<PaymentGatewayResponse>.Failure(Error.Validation(
        "Stripe.PaymentMethod.Required",
        $"Payment method was declined or requires re-entry: {message}"));
```

### 4.2 PRD-003: Transient error detection

```csharp
// StripeGateway.cs — replace MapStripeException
private static Result<PaymentGatewayResponse> MapStripeException(StripeException ex)
{
    var e = ex.StripeError;
    var code = e?.Code ?? GatewayConstants.ErrorCodes.Stripe.UnknownError;
    var msg = e?.DeclineCode is not null
        ? $"Stripe [{code}] decline [{e.DeclineCode}]: {e!.Message}"
        : $"Stripe [{code}]: {e?.Message ?? ex.Message}";

    var isTransient = ex.HttpStatusCode >= System.Net.HttpStatusCode.InternalServerError
        || e?.ErrorType == "api_error"
        || e?.ErrorType == "api_connection_error";

    return isTransient
        ? StripeGatewayResult.Errors.TransientGatewayError(code, msg)
        : StripeGatewayResult.Errors.GatewayError(code, msg);
}
```

New error factory:

```csharp
// StripeGateway.Result.cs
public static Result<PaymentGatewayResponse> TransientGatewayError(string code, string message) =>
    Result<PaymentGatewayResponse>.Failure(
        Error.Unexpected($"Stripe.Transient.{code}", message));
```

**Constraint**: The `PaymentProcessingService` and feature handlers that call the gateway should distinguish `Error.Unexpected` (retryable) from `Error.Validation` (terminal) and return appropriate HTTP status codes (502 vs 422).

### 4.3 PRD-004/005: Statement descriptor and shipping

```csharp
// StripeGateway.cs — inside CreatePaymentIntentOptions, after L181
if (!string.IsNullOrEmpty(options.StatementDescriptorSuffix))
    o.StatementDescriptorSuffix = options.StatementDescriptorSuffix;

if (options.ShippingAddress is not null)
{
    o.Shipping = new ChargeShippingOptions
    {
        Name = options.ShippingAddress.GetValueOrDefault("name")?.ToString(),
        Address = new AddressOptions
        {
            Line1 = options.ShippingAddress.GetValueOrDefault("line1")?.ToString(),
            Line2 = options.ShippingAddress.GetValueOrDefault("line2")?.ToString(),
            City = options.ShippingAddress.GetValueOrDefault("city")?.ToString(),
            State = options.ShippingAddress.GetValueOrDefault("state")?.ToString(),
            PostalCode = options.ShippingAddress.GetValueOrDefault("postal_code")?.ToString(),
            Country = options.ShippingAddress.GetValueOrDefault("country")?.ToString(),
        }
    };
}
```

### 4.4 PRD-006: Checked amount conversion

```csharp
// StripeGateway.cs — in CreatePaymentIntentOptions, replace L166
// Before:
Amount = (long)Math.Round(amount * CentsMultiplier, MidpointRounding.AwayFromZero),

// After:
Amount = checked((long)Math.Round(amount * CentsMultiplier, MidpointRounding.AwayFromZero)),
```

Apply same `checked {}` to `CaptureAsync` L87 and `RefundAsync` L126.

### 4.5 RES-007: Hangfire retry attribute

```csharp
// ProcessStripeWebhookEventJob.cs — add to class declaration
[AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
public sealed partial class ProcessStripeWebhookEventJob
```

### 4.6 RES-008: Database index

```sql
-- Migration SQL
CREATE INDEX IF NOT EXISTS ix_payment_captures_response_code
    ON "PaymentCaptures" ("ResponseCode")
    WHERE "ResponseCode" IS NOT NULL;
```

Add as an EF Core migration index:

```csharp
// In PaymentCapture configuration entity builder
builder.HasIndex(p => p.ResponseCode)
    .HasDatabaseName("ix_payment_captures_response_code")
    .HasFilter("\"ResponseCode\" IS NOT NULL");
```

### 4.7 RES-009: Startup validation

```csharp
// New file: Services/Provider/Stripe/StripeSettingValidation.cs
public sealed class StripeSettingValidation : IValidateOptions<StripeSetting>
{
    public ValidateOptionsResult Validate(string? name, StripeSetting options)
    {
        if (!options.Enabled)
            return ValidateOptionsResult.Success;

        var errors = new List<string>();
        if (string.IsNullOrEmpty(options.SecretKey))
            errors.Add("GatewayProviders:stripe:SecretKey is required when Enabled=true.");
        if (string.IsNullOrEmpty(options.WebhookSecret))
            errors.Add("GatewayProviders:stripe:WebhookSecret is required when Enabled=true.");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
```

DI registration in `Payment.Extension.cs`:

```csharp
services.AddSingleton<IValidateOptions<StripeSetting>, StripeSettingValidation>();
```

### 4.8 DOM-010: Dispute state transition

```csharp
// PaymentCapture.Method.State.cs — add new state
public enum PaymentRecordState
{
    // ... existing ...
    Disputed = 7  // ADD
}

// PaymentCapture.Method.State.cs — add new method
public Result Dispute()
{
    if (State == Disputed || State == Void || State == Invalid)
        return PaymentCaptureResult.Failure.InvalidStateTransition(State, Disputed);
    State = Disputed;
    return Result.Ok();
}
```

```csharp
// ProcessStripeWebhookEventJob.cs — update HandleChargeDisputeCreated
private async Task HandleChargeDisputeCreated(Event stripeEvent, CancellationToken ct)
{
    var dispute = stripeEvent.Data.Object as Dispute;
    if (dispute is null) return;

    var payment = await _dbContext.Set<PaymentCapture>()
        .FirstOrDefaultAsync(p => p.ResponseCode == dispute.PaymentIntentId, ct);
    if (payment is null) return;

    var result = payment.Dispute();
    if (result.IsFailure)
    {
        ProcessStripeWebhookEventJobLoggers.CannotDisputePayment(
            _logger, payment.Id, payment.State.ToString(), result.Message);
        return;
    }

    await _dbContext.SaveChangesAsync(ct);
}
```

Also guard `RefundAsync` and `CaptureAsync` in `PaymentProcessingService` against `Disputed` state.

### 4.9 DOM-011: Make GetPaymentStatusAsync abstract

```csharp
// Gateway.cs L34-36 — before
public virtual Task<string> GetPaymentStatusAsync(
    string responseCode, CancellationToken ct = default)
    => Task.FromResult("succeeded");

// After
public abstract Task<string> GetPaymentStatusAsync(
    string responseCode, CancellationToken ct = default);
```

**Constraint**: `BogusGateway` must override this method (currently relies on default). Add override returning `"succeeded"`.

### 4.10 QLT-012: Narrow exception catch

```csharp
// StripeWebhookDispatcher.cs L64-69 — before
try { return EventUtility.ParseEvent(payload); }
catch (Exception ex)
{
    StripeWebhookDispatcherLoggers.EventParseFailed(_logger, ex, payload);
    return null;
}

// After
try { return EventUtility.ParseEvent(payload); }
catch (StripeException ex)
{
    StripeWebhookDispatcherLoggers.EventParseFailed(_logger, ex, payload);
    return null;
}
```

### 4.11 QLT-013: Reuse SDK service instances

```csharp
// StripeGateway.cs — add private readonly fields
private readonly PaymentIntentService _paymentIntentService = new();
private readonly RefundService _refundService = new();
private readonly SetupIntentService _setupIntentService = new();

// Replace all `new PaymentIntentService()` with `_paymentIntentService`
// Replace all `new RefundService()` with `_refundService`
// Replace all `new SetupIntentService()` with `_setupIntentService`
```

## 5. Acceptance Criteria

### 3DS / SCA

- **AC-001**: Given a PaymentIntent that requires 3DS (status `requires_action`), When `PurchaseAsync` is called, Then the result is `Success` with `paymentStatus = "requires_action"` and `clientSecret` populated.

- **AC-002**: Given a PaymentIntent that requires payment method re-entry (status `requires_payment_method`), When `PurchaseAsync` is called, Then the result is `Failure` with error code `Stripe.PaymentMethod.Required`.

### Transient error handling

- **AC-003**: Given a `StripeException` with HTTP 503, When `MapStripeException` is called, Then the result error type is `ErrorType.Unexpected` (retryable).

- **AC-004**: Given a `StripeException` with `DeclineCode = "card_declined"`, When `MapStripeException` is called, Then the result error type is `ErrorType.Validation` (terminal).

### Descriptor and shipping

- **AC-005**: Given `GatewayOptions.StatementDescriptorSuffix = "MyShop Order"`, When `CreatePaymentIntentOptions` runs, Then `PaymentIntentCreateOptions.StatementDescriptorSuffix` equals `"MyShop Order"`.

- **AC-006**: Given `GatewayOptions.StatementDescriptorSuffix` is null, When `CreatePaymentIntentOptions` runs, Then `PaymentIntentCreateOptions.StatementDescriptorSuffix` is not set.

### Amount safety

- **AC-007**: Given `amount = 92_233_720_368_547_758.08m` (just above `long.MaxValue / 100`), When `CreatePaymentIntentOptions` runs, Then `OverflowException` is thrown, not silently wrapped to a negative value.

### Webhook resilience

- **AC-008**: Given `dotnet build`, Then `ProcessStripeWebhookEventJob` has `[AutomaticRetry(Attempts = 3)]` attribute.

- **AC-009**: Given `dotnet test --filter "FullyQualifiedName~ProcessStripeWebhookEventJob"`, Then tests pass. Given a DB transient error during `SaveChangesAsync`, Then Hangfire retries the job up to 3 times.

### DB index

- **AC-010**: Given an EF Core migration is generated, Then `PaymentCaptures` table has index `ix_payment_captures_response_code` on `ResponseCode` with filter `"ResponseCode" IS NOT NULL`.

### Startup validation

- **AC-011**: Given `GatewayProviders:stripe:Enabled = true` and `WebhookSecret = ""`, When the application starts, Then startup fails with a clear error message.

- **AC-012**: Given `GatewayProviders:stripe:Enabled = false` and `WebhookSecret = ""`, When the application starts, Then startup succeeds (no validation for disabled providers).

### Dispute handling

- **AC-013**: Given a `charge.dispute.created` webhook event, When processed by `ProcessStripeWebhookEventJob`, Then the payment transitions to `Disputed` state.

- **AC-014**: Given a payment in `Disputed` state, When `RefundAsync` is called in `PaymentProcessingService`, Then the result is `Failure` citing invalid state.

### Abstract status

- **AC-015**: Given `dotnet build`, Then `Gateway.GetPaymentStatusAsync` is `abstract`. `BogusGateway` overrides it returning `"succeeded"`. No other gateway fails to compile.

### Code quality

- **AC-016**: Given `rg "catch \(Exception" service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs`, Then the match is `catch (StripeException` only (not `Exception`).

- **AC-017**: Given `rg "new PaymentIntentService\(\)" service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs`, Then 0 matches (all replaced by `_paymentIntentService`).

- **AC-018**: `dotnet build` succeeds with 0 warnings. `dotnet test service/Api/tests/Module.UnitTests --filter "Category=Unit"` passes. `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Payment"` passes.

## 6. Test Automation Strategy

- **Test Levels**: Unit tests for new error factories, `checked {}` overflow, `MapStripeException` transient detection, startup validator. Integration tests for 3DS flow (requires Stripe sandbox + test cards that trigger 3DS).
- **Frameworks**: xUnit, FluentAssertions, Moq.
- **Test updates required**:
  - `StripeGatewayTests.cs`: Add test for `requires_action` status → success with `paymentStatus`; `requires_payment_method` → failure. Add test for transient `StripeException` (HTTP 503) → `ErrorType.Unexpected`. Add test for `OverflowException` on extreme amounts.
  - `ProcessStripeWebhookEventJobTests.cs`: Add test for dispute → `Disputed` state. Verify `[AutomaticRetry]` attribute present.
  - `StripeSettingValidation` unit test: Verify enabled+empty secret fails. Verify disabled+empty secret passes.
  - `PaymentProcessingService` tests: Verify `RefundAsync` returns failure for `Disputed` state.
- **CI/CD Integration**: `dotnet test --filter "Category=Unit"` runs in GitHub Actions. Integration-tagged tests run separately.
- **Coverage Requirements**: All new error paths must have unit test coverage.

## 7. Rationale & Context

### PRD-001/002: Why 3DS matters

Under PSD2/SCA regulations in the EU/UK (and increasingly globally), card issuers may require additional authentication. Stripe signals this via `requires_action` status on the PaymentIntent. Without handling this, all 3DS-required payments fail at the server with no path for the frontend Stripe.js to present the authentication modal. The fix is minimal: return `client_secret` and `paymentStatus` so the frontend can call `stripe.confirmCardPayment()`.

### PRD-003: Why transient vs terminal matters

The current `MapStripeException` returns all errors as terminal failures. If Stripe's API returns 503 during a maintenance window, the customer's order is lost — no retry occurs. The `PaymentProcessingService` treats all gateway errors as terminal and returns them to the client. Distinguishing transient errors allows the caller (or a retry middleware) to retry.

### PRD-004/005: Why statement descriptor matters

Without `StatementDescriptorSuffix`, all charges appear as generic "ReSys.Shop" on customer statements. This increases chargeback rates because customers don't recognize the charge. The field exists on `GatewayOptions` but is never consumed — it's dead configuration.

### RES-007: Why Hangfire retry matters

`ProcessStripeWebhookEventJob` is enqueued with `CancellationToken.None` (fire-and-forget). If the DB is temporarily down, `SaveChangesAsync` throws and the job exits. Without `[AutomaticRetry]`, Hangfire marks it as "succeeded because we returned without throwing" — meaning the webhook is permanently lost. Stripe retries webhooks for up to 3 days, but Hangfire will never retry.

### RES-008: Why the index matters

Every webhook handler does `FirstOrDefaultAsync(p => p.ResponseCode == intent.Id)`. Without an index, this is a sequential scan on every webhook event. At moderate volume (1000 orders/day), this adds up quickly.

### DOM-010: Why dispute state matters

Currently, a disputed payment remains in `Completed` state. The admin refund endpoint checks `payment.CanRefund(amount)` which only checks `State == Completed`. A disputed payment can thus be refunded, effectively paying the customer twice (once by Stripe to the dispute resolver, once by the merchant's refund). Adding a `Disputed` state and guarding refunds closes this gap.

### QLT-013: Why reuse SDK services

`new PaymentIntentService()`, `new RefundService()`, and `new SetupIntentService()` are created per API call. These are lightweight wrappers with no state beyond the underlying `HttpClient` (Stripe.net manages a shared one internally), but the allocations add GC pressure in high-throughput scenarios. More importantly, using fields makes the code testable — mocks can be injected.

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: Stripe API v2024+ — all operations unchanged except new status codes (`requires_action`, `requires_payment_method`) recognized. Stripe.net 52.1.0 already supports these.

### Third-Party Services
- **SVC-001**: Hangfire — `[AutomaticRetry]` attribute depends on Hangfire server being registered in DI (verify `AddHangfireServer()` in Program.cs).

### Infrastructure Dependencies
- **INF-001**: PostgreSQL — new partial index on `ResponseCode`. Generatable via EF Core migration. No schema-breaking changes.
- **INF-002**: Options validation — `IValidateOptions<T>` is a standard .NET `Microsoft.Extensions.Options` feature, no additional NuGet package.

### Data Dependencies
- None.

### Technology Platform Dependencies
- **PLT-001**: .NET 10 — `checked` keyword is C# standard; no version constraint.
- **PLT-002**: Stripe.net 52.1.0 — already referenced. Verify `ChargeShippingOptions` and `AddressOptions` types exist in this version.

### Compliance Dependencies
- **SEC-001**: PSD2/SCA compliance — the `requires_action` handling is required for EU/UK payment processing. Non-compliance results in declined payments.
- **SEC-002**: PCI DSS — statement descriptor must not expose full card numbers. The `GatewayOptions.StatementDescriptorSuffix` is merchant-configured and assumed PCI-safe.

## 9. Examples & Edge Cases

### 3DS flow

```
Given: Customer uses a card that requires 3DS
When: POST /api/storefront/payment/create-intent
Then: Server returns { "clientSecret": "pi_xxx_secret_yyy", "paymentStatus": "requires_action" }
And: Frontend calls stripe.confirmCardPayment(clientSecret)
And: Stripe redirects user to bank's 3DS page
And: After authentication, Stripe sends payment_intent.succeeded webhook
And: Webhook job transitions payment to Completed
```

### Transient error

```
Given: Stripe API returns HTTP 503 during PaymentIntent creation
When: StripeGateway.PurchaseAsync catches StripeException
Then: MapStripeException returns ErrorType.Unexpected with code "Stripe.Transient.api_error"
And: PaymentProcessingService propagates the unexpected error
And: Feature handler returns HTTP 502 Bad Gateway (not 422)
```

### Out-of-order webhook replay

```
Given: Payment is already Completed
And: Stripe replays a payment_intent.succeeded event (network glitch caused double-send)
When: HandlePaymentIntentSucceeded processes the event
Then: payment.State == Completed guard at L69 returns early (idempotent)
And: No duplicate SaveChangesAsync call
```

### Disputed refund guard

```
Given: Payment state is Disputed
When: Admin calls POST /api/admin/payments/{id}/refund
Then: payment.CanRefund(amount) returns false
And: Response is 422 with "Cannot transition payment from 'Disputed' to 'Completed'"
```

### Startup validation

```
Given: appsettings.Development.json has GatewayProviders:stripe:Enabled=true but WebhookSecret=""
When: Application starts
Then: Startup throws OptionsValidationException
And: Error message: "GatewayProviders:stripe:WebhookSecret is required when Enabled=true."
```

## 10. Validation Criteria

- **VC-001**: `dotnet build service/Api/src/Module` passes with 0 warnings.
- **VC-002**: `dotnet test service/Api/tests/Module.UnitTests --filter "Category=Unit"` passes.
- **VC-003**: `rg "new PaymentIntentService\(\)" service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs` returns 0 matches.
- **VC-004**: `rg "catch \(Exception" service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs` returns 0 matches for `catch (Exception` (only `catch (StripeException`).
- **VC-005**: `rg "virtual.*GetPaymentStatusAsync" service/Api/src/Module/Payment/Services/Provider/Gateway.cs` returns `abstract`.
- **VC-006**: `rg "checked\(" service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs` returns 3 matches (Purchase, Capture, Refund).
- **VC-007**: Migration SQL or EF configuration produces `CREATE INDEX ix_payment_captures_response_code`.
- **VC-008**: `rg "AutomaticRetry" service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs` returns 1 match.
- **VC-009**: `PaymentRecordState` enum contains `Disputed = 7`.
- **VC-010**: `StripeSettingValidation` class exists and is registered in DI. App throws on startup if Enabled=true and WebhookSecret is empty.

## 11. Related Specifications / Further Reading

- [spec-design-stripe-integration-fixes.md](/spec/spec-design-stripe-integration-fixes.md) — Prior bugfix round (signature, validator, cleanup)
- [spec-design-payment-bugfixes.md](/spec/spec-design-payment-bugfixes.md) — Original payment bugfix round
- [Stripe API: PaymentIntent statuses](https://docs.stripe.com/api/payment_intents/object#payment_intent_object-status) — Official status documentation
- [Stripe: Strong Customer Authentication](https://docs.stripe.com/strong-customer-authentication) — SCA/3DS guide
- [Stripe: Error handling](https://docs.stripe.com/error-handling) — Error types and retry recommendations
- [Stripe: Best practices for webhooks](https://docs.stripe.com/webhooks#best-practices) — Idempotency and retry
- [Hangfire: Automatic Retry](https://docs.hangfire.io/en/latest/background-methods/dealing-with-exceptions.html) — Retry attribute docs
- [Payment.Extension.cs](/service/Api/src/Module/Payment/Payment.Extension.cs) — DI registration (to be updated)
- [StripeGateway.cs](/service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs) — Main gateway implementation
