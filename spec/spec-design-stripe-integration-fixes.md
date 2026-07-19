---
title: Stripe Payment Integration Bug Fixes and Legacy Cleanup
version: 1.0
date_created: 2026-07-19
owner: Platform Team
tags: design, payment, stripe, bugfix, cleanup
---

# Introduction

Fix 3 bugs and 9 code-quality issues in the Stripe payment integration. The bugs affect webhook signature validation, state transition error messages, and the legacy webhook handler's missing signature check. The quality issues eliminate 4 duplicate classes/files, correct outdated README documentation, and retag unit tests that make real HTTP calls to Stripe. All fixes are confined to the Payment module.

## 1. Purpose & Scope

**Purpose**: Define exact, verifiable changes to eliminate the remaining defects in the Stripe payment integration after the prior bugfix round (spec `spec-design-payment-bugfixes.md`).

**Scope**: Payment module only. Affects Stripe webhook dispatch, state machine validation, legacy class cleanup, test categorization, and README documentation.

**Out of scope**: Gateway abstraction redesign, new Stripe API operations, integration test creation.

**Assumptions**: The `StripeWebhook.Endpoint.cs` Carter endpoint is the primary webhook entry point and correctly reads the `Stripe-Signature` header. The `IWebhookHandler` interface is not currently resolved from DI for webhook processing — the Carter endpoint sends `StripeWebhook.Command` directly via MediatR.

## 2. Definitions

| Term | Definition |
|---|---|
| Legacy duplicate | A class or interface that exists in both `Services/Abstractions/` and another location (e.g., `Services/Provider/`, `Services/Webhook/`) with identical or near-identical signatures, where the Abstractions copy is explicitly marked as a legacy duplicate via comment. |
| Hardcoded literal | A string value embedded directly in code where a runtime parameter should be used instead. |
| State transition | A domain-enforced move from one `PaymentRecordState` value to another, validated by `IsValidTransition(from, to)`. |
| Unit test | A test that runs without external dependencies (no network, no database). Marked via `[Trait("Category", "Unit")]`. |
| Integration test | A test that depends on external systems (Stripe API, database). Marked via `[Trait("Category", "Integration")]`. |

## 3. Requirements, Constraints & Guidelines

### SIG-001: StripeWebhookDispatcher HandleAsync must pass real signature header

`StripeWebhookDispatcher.HandleAsync` MUST be removed OR its signature MUST be changed to accept the `Stripe-Signature` header as a parameter. The current implementation hardcodes `"stripe-signature"` as the literal string, which causes `ValidateSignature` to always fail.

**Constraint**: The primary webhook path (Carter endpoint → `StripeWebhook.CommandHandler`) already correctly extracts and validates the real header. This fix is for the `StripeWebhookDispatcher.HandleAsync` path, which is dead code (not on the `IStripeWebhookService` interface, zero callers through DI). Prefer deletion.

### TRN-002: State transition validator must pass target state

`PaymentCapture.Validation.ApplyStateTransitionRules` at lines 23-24 MUST pass `target` as the second argument to `InvalidStateTransition` instead of `currentState`. Current code always generates error messages saying "Cannot transition from X to X" regardless of the actual target state.

### WEB-003: Legacy StripeWebhookHandler HandleAsync must validate signature

`StripeWebhookHandler.HandleAsync` MUST either validate the webhook signature before returning `Result.Ok()` OR be removed. Currently it only checks whether the webhook secret is configured, then returns success without actually validating the incoming payload's signature. If this handler is ever resolved as `IWebhookHandler` from DI and called, forged webhook events will be accepted.

**Constraint**: The `StripeWebhookHandler` is registered as `IWebhookHandler` in `Payment.Extension.cs:80` and marked with a TODO comment to be removed. Until removal, the method must not silently accept unvalidated events.

### TST-004: StripeGateway tests must be tagged as Integration

`StripeGatewayAuthorizeTests.cs` MUST be retagged from `[Trait("Category", "Unit")]` to `[Trait("Category", "Integration")]`. All 5 test methods make real HTTP calls to Stripe's API with a fake key `sk_test_fake`. These tests fail without network connectivity and take 3-5 seconds each.

**Constraint**: The CI pipeline's `dotnet test --filter "Category=Unit"` must NOT attempt to run these tests without network access.

### DOC-005: README.yaml must reflect current entity schema

`README.yaml` MUST be updated to reflect the current state of `PaymentCapture` and `PaymentMethod` entities:

1. Line 263: `PaymentMethodId (Guid)` → `PaymentMethodId (Guid?)` (changed by the prior bugfix `spec-design-payment-bugfixes.md` FK-002)
2. Lines 302-303: Remove `WebhookUrl` and `WebhookSecret` from PaymentMethod properties — they exist on `StripeSetting`, not `PaymentMethod`
3. Line 719: Webhook registration example shows `/api/storefront/webhooks/stripe` — actual route is `/api/payments/stripe/webhook` (defined in `PaymentFeature.Storefront.Payment.Webhooks.Stripe.Route`)

### CLN-006: Delete duplicate GatewayConstants (Models)

The file `service/Api/src/Module/Payment/Services/Models/GatewayConstants.cs` MUST be deleted. It is a byte-identical duplicate of `Services/Provider/GatewayConstants.cs`. `ConfirmPayment.cs` imports the Models copy; all other files import the Provider copy. If these copies diverge, behavior becomes unpredictable.

**Constraint**: Before deleting, verify all imports of `Module.Payment.Services.Models.GatewayConstants` are redirected to `Module.Payment.Services.Provider.GatewayConstants`.

### CLN-007: Delete duplicate Gateway abstract class (Abstractions)

The file `service/Api/src/Module/Payment/Services/Abstractions/Gateway.cs` MUST be deleted. It is a near-identical duplicate of `Services/Provider/Gateway.cs`. The class comment already states "Legacy duplicate of Services.Provider.Gateway — kept for compatibility." `StripeGateway` and `BogusGateway` both inherit from the Provider copy.

### CLN-008: Delete duplicate StripeSetting (Models)

The file `service/Api/src/Module/Payment/Services/Models/StripeOptions.cs` MUST be deleted. It defines a `StripeSetting` class identical to `Services/Provider/Stripe/StripeOptions.cs`. All consumers use the Provider copy (via `using StripeSetting = Module.Payment.Services.Provider.Stripe.StripeSetting` aliases).

### CLN-009: Delete duplicate IWebhookHandler (Abstractions)

The file `service/Api/src/Module/Payment/Services/Abstractions/IWebhookHandler.cs` MUST be deleted. It is a duplicate of `Services/Webhook/IWebhookHandler.cs`. The class comment already states "Legacy duplicate of Services.Webhook.IWebhookHandler." The DI registration at `Payment.Extension.cs:80` uses `Services.Webhook.IWebhookHandler`.

## 4. Interfaces & Data Contracts

### 4.1 SIG-001: Remove dead-code method

```csharp
// StripeWebhookDispatcher.cs — delete lines 43-57 (the HandleAsync method)
// Method is NOT on the IStripeWebhookService interface and has zero callers
```

If not deleting, fix signature to accept signature header:

```csharp
// Before (line 44-56)
public async Task<Result> HandleAsync(string eventType, string payload, CancellationToken ct = default)
{
    // ...
    var result = await _sender.Send(new StripeWebhook.Command(payload, "stripe-signature"), ct);
    // ...
}

// After
public async Task<Result> HandleAsync(string eventType, string payload, string stripeSignature, CancellationToken ct = default)
{
    // ...
    var result = await _sender.Send(new StripeWebhook.Command(payload, stripeSignature), ct);
    // ...
}
```

### 4.2 TRN-002: Fix validator error message

```csharp
// PaymentCapture.Validation.cs:23-24 — before
.WithErrorCode(PaymentCaptureResult.Failure.InvalidStateTransition(currentState, currentState).Code)
.WithMessage(PaymentCaptureResult.Failure.InvalidStateTransition(currentState, currentState).Message);

// After
.WithErrorCode(PaymentCaptureResult.Failure.InvalidStateTransition(currentState, target).Code)
.WithMessage(PaymentCaptureResult.Failure.InvalidStateTransition(currentState, target).Message);
```

### 4.3 WEB-003: Fix legacy handler security

```csharp
// StripeWebhookService.cs:40-48 — before
public Task<Result> HandleAsync(string eventType, string payload, CancellationToken ct = default)
{
    if (string.IsNullOrEmpty(_options.WebhookSecret))
        return Task.FromResult<Result>(Error.Validation(
            "Stripe.WebhookSecret.NotConfigured",
            "Stripe webhook secret is not configured."));
    return Task.FromResult(Result.Ok());
}

// After — add signature validation
public Task<Result> HandleAsync(string eventType, string payload, string stripeSignature, CancellationToken ct = default)
{
    if (string.IsNullOrEmpty(_options.WebhookSecret))
        return Task.FromResult<Result>(Error.Validation(
            "Stripe.WebhookSecret.NotConfigured",
            "Stripe webhook secret is not configured."));
    if (!ValidateSignature(payload, stripeSignature))
        return Task.FromResult<Result>(Error.Unauthorized(
            "Stripe.Webhook.InvalidSignature",
            "Invalid Stripe webhook signature."));
    return Task.FromResult(Result.Ok());
}
```

Note: This also requires updating the `IWebhookHandler` interface to pass `stripeSignature` — or removing the class entirely since it's legacy.

### 4.4 TST-004: Test retag

```csharp
// StripeGatewayAuthorizeTests.cs:8 — before
[Trait("Category", "Unit")]

// After
[Trait("Category", "Integration")]
```

### 4.5 DOC-005: README.yaml corrections

```yaml
# Line 263 — before
- PaymentMethodId (Guid) — FK to payment method

# Line 263 — after
- PaymentMethodId (Guid?) — FK to payment method (nullable)
```

Remove lines 302-303 entirely:
```yaml
# Delete these lines
- WebhookUrl (string?) — webhook endpoint URL
- WebhookSecret (string?) — webhook signing secret
```

```yaml
# Line 719 — before
app.MapPost("/api/storefront/webhooks/stripe", ...

# Line 719 — after
app.MapPost("/api/payments/stripe/webhook", ...
```

### 4.6 CLN-006 to CLN-009: Files to delete

| File | Reason |
|---|---|
| `Services/Models/GatewayConstants.cs` | Duplicate of `Services/Provider/GatewayConstants.cs` |
| `Services/Abstractions/Gateway.cs` | Duplicate of `Services/Provider/Gateway.cs` |
| `Services/Models/StripeOptions.cs` | Duplicate of `Services/Provider/Stripe/StripeOptions.cs` |
| `Services/Abstractions/IWebhookHandler.cs` | Duplicate of `Services/Webhook/IWebhookHandler.cs` |

## 5. Acceptance Criteria

- **AC-001**: Given a Stripe webhook payload and valid signature, When the Carter endpoint sends `StripeWebhook.Command(payload, realSignature)`, Then signature validation passes and the background job is enqueued — unchanged from current behavior.
- **AC-002**: Given a state transition from `Checkout` to `Void` (invalid), When `ApplyStateTransitionRules` generates the error, Then the error message reads "Cannot transition payment from 'Checkout' to 'Void'" — NOT "from 'Checkout' to 'Checkout'".
- **AC-003**: Given `StripeWebhookHandler.HandleAsync` is called with an invalid signature, Then the method returns an `Unauthorized` error — NOT `Result.Ok()`.
- **AC-004**: Given `dotnet test --filter "Category=Unit"`, Then none of the tests make HTTP calls to `api.stripe.com`.
- **AC-005**: Given a search for `PaymentMethodId (Guid)` in `README.yaml`, Then the match is `PaymentMethodId (Guid?)`.
- **AC-006**: Given a search for `WebhookUrl` or `WebhookSecret` in the PaymentMethod property list in `README.yaml`, Then 0 matches are found.
- **AC-007**: Given `dotnet build`, Then all imports of `GatewayConstants` resolve without error after deleting `Services/Models/GatewayConstants.cs`.
- **AC-008**: Given `dotnet build`, Then all imports of `Gateway` resolve without error after deleting `Services/Abstractions/Gateway.cs`.
- **AC-009**: Given `dotnet build`, Then all imports of `StripeSetting` resolve without error after deleting `Services/Models/StripeOptions.cs`.
- **AC-010**: Given `dotnet build`, Then all imports of `IWebhookHandler` resolve without error after deleting `Services/Abstractions/IWebhookHandler.cs`.
- **AC-011**: `dotnet build` succeeds with 0 warnings after all changes. `dotnet test service/Api/tests/Module.UnitTests --filter "Category=Unit"` passes.

## 6. Test Automation Strategy

- **Test Levels**: Unit tests for state transition validator (existing `Payment.Validation.Tests.cs`); Unit tests for webhook handler (existing `StripeWebhookTests.cs`).
- **Frameworks**: xUnit, FluentAssertions, Moq.
- **Test updates required**:
  - Add a test verifying `InvalidStateTransition` error message contains the correct `from` and `to` states.
  - Add a test in `StripeWebhookTests.cs` verifying that `StripeWebhookHandler.HandleAsync` returns `Unauthorized` for invalid signatures.
  - No test changes needed for dead-code deletion or README updates.
- **CI/CD Integration**: `dotnet test --filter "Category=Unit"` runs in GitHub Actions — after TST-004, Stripe gateway integration tests will be excluded from the unit test run.
- **Coverage Requirements**: No change from existing project defaults.

## 7. Rationale & Context

### SIG-001: Why delete HandleAsync

The method is not on any interface implemented by `StripeWebhookDispatcher` (`IStripeWebhookService` has only `ValidateSignature` and `ParseEvent`). It has zero callers through DI. The actual webhook path (Carter endpoint → MediatR → `StripeWebhook.CommandHandler`) correctly validates the signature. Dead code with a hardcoded literal that would cause a security failure if ever called should be deleted, not left as a landmine.

### TRN-002: Why the validator bug matters

The state transition validator is used in FluentValidation rules for payment features. When a user attempts an invalid state transition (e.g., voiding a completed payment), the error message is logged and returned to the client. A bugged message saying "from Checkout to Checkout" makes debugging impossible — the developer can't tell what transition was attempted.

### WEB-003: Why the legacy handler is a risk

`StripeWebhookHandler` is registered as `IWebhookHandler` in DI at `Payment.Extension.cs:80`. Even though the current webhook path doesn't use `IWebhookHandler`, any future code that resolves this interface and calls `HandleAsync` will receive `Result.Ok()` for any payload, regardless of signature validity. This is a latent security vulnerability.

### TST-004: Why retagging matters

The CI pipeline runs unit tests on every push. Tests making real HTTP calls to Stripe with a fake key will fail when run in an air-gapped CI environment, causing false-positive build failures. Tagging them as Integration ensures they only run when external dependencies are available.

### CLN-006 to CLN-009: Why delete duplicates

The codebase has 4 pairs of duplicate files marked as "legacy." These add maintenance burden: a change in one copy may not propagate to the other, creating subtle bugs. All consumers already reference the non-legacy copies. The delete action is purely mechanical — redirect remaining consumers if any exist.

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: Stripe API (payment gateway) — `StripeGatewayAuthorizeTests` currently makes real calls to Stripe. After retag, these tests will not run in CI unless network access is available.
- **EXT-002**: Stripe SDK (Stripe.net) — `EventUtility.ValidateSignature` and `EventUtility.ParseEvent` used by both webhook handlers. No version change needed.

### Third-Party Services
- None new.

### Infrastructure Dependencies
- **INF-001**: None. No database or service changes.

### Data Dependencies
- None.

### Technology Platform Dependencies
- **PLT-001**: .NET 10, Stripe.net — no version changes.

### Compliance Dependencies
- **SEC-001**: Stripe webhook signature validation must use HMAC-SHA256 per Stripe's documented specification. Failure to validate per WEB-003 means forged webhook events could alter payment states.

## 9. Examples & Edge Cases

### TRN-002: State transition error message

```
Given: payment.State = Checkout, target = Void
When: ApplyStateTransitionRules validates the transition
Then (before fix): error message = "Cannot transition payment from 'Checkout' to 'Checkout'"
Then (after fix):  error message = "Cannot transition payment from 'Checkout' to 'Void'"
```

### CLN-006: Import redirection

```
Before: ConfirmPayment.cs imports Module.Payment.Services.Models.GatewayConstants
After delete: ConfirmPayment.cs must import Module.Payment.Services.Provider.GatewayConstants
Impact: GatewayConstants.Stripe.IntentStatus.Succeeded resolves to the same value "succeeded"
```

### TST-004: CI impact

```
Before: dotnet test --filter "Category=Unit" includes StripeGatewayAuthorizeTests (5 tests)
        → tests fail with StripeException: Invalid API key (real HTTP call)
After:  dotnet test --filter "Category=Unit" excludes StripeGatewayAuthorizeTests
        → tests run in CI without network dependency
        dotnet test --filter "Category=Integration" includes StripeGatewayAuthorizeTests
        → tests run when network and Stripe sandbox are available
```

### WEB-003: IWebhookHandler interface change

If the fix adds `stripeSignature` to the `IWebhookHandler.HandleAsync` signature, this is a breaking interface change. Alternative: delete the `StripeWebhookHandler` class entirely since it's legacy and the `StripeWebhookDispatcher` is the current implementation. The DI registration at `Payment.Extension.cs:79-80` already has a TODO to remove the legacy handler.

## 10. Validation Criteria

- **VC-001**: `dotnet build service/Api/src/Module` passes with 0 warnings.
- **VC-002**: `dotnet test service/Api/tests/Module.UnitTests --filter "Category=Unit"` passes with 0 failures and 0 network calls to `api.stripe.com`.
- **VC-003**: `rg "currentState, currentState\)" service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Validation.cs` returns 0 matches.
- **VC-004**: `rg "InvalidStateTransition\(currentState, target\)" service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Validation.cs` returns 1 match.
- **VC-005**: File `Services/Models/GatewayConstants.cs` does not exist. `dotnet build` still passes.
- **VC-006**: File `Services/Abstractions/Gateway.cs` does not exist. `dotnet build` still passes.
- **VC-007**: File `Services/Models/StripeOptions.cs` does not exist. `dotnet build` still passes.
- **VC-008**: File `Services/Abstractions/IWebhookHandler.cs` does not exist. `dotnet build` still passes.
- **VC-009**: `rg "WebhookUrl|WebhookSecret" service/Api/src/Module/Payment/README.yaml` returns 0 matches (within PaymentMethod properties section).
- **VC-010**: `rg "PaymentMethodId \(Guid\)" service/Api/src/Module/Payment/README.yaml` returns 0 matches.

## 11. Related Specifications / Further Reading

- [spec-design-payment-bugfixes.md](/spec/spec-design-payment-bugfixes.md) — Prior bugfix round (FW-001 through DOC-006)
- [README.yaml](/service/Api/src/Module/Payment/README.yaml) — Module documentation (to be corrected)
- [Stripe Webhook Signature Verification](https://docs.stripe.com/webhooks#verify-official-libraries) — Official Stripe docs
- [Payment.Extension.cs](/service/Api/src/Module/Payment/Payment.Extension.cs) — DI registration (contains TODO to remove legacy handler)
