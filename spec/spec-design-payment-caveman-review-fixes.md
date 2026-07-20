---
title: Payment Domain State Machine Integrity and Deduplication — 13 Caveman Review Findings
version: 1.0
date_created: 2026-07-20
owner: Platform Team
tags: design, payment, domain, state-machine, refactor, bugfix
---

# Introduction

Addresses 13 remaining findings from the caveman review of the Payment module (~114 files). The issues span three categories: domain state machine integrity (missing transitions in the FluentValidation guard, `Capture()` domain method doesn't mutate state, service layer direct mutations bypassing domain methods), deduplication (refund logic split across two layers, duplicate error codes, legacy interfaces, cents constant duplication), edge case handling (uncaught `OverflowException` from `checked{}`, redundant transaction rollback, `BogusGateway` false-success for unknown auth codes, arbitrary first payment method selection), and dead code (`CaptureEventCreated` field never read).

## 1. Purpose & Scope

**Purpose**: Fix 13 defects that compromise the Payment module's domain state machine correctness, maintainability, and edge-case safety. The primary goal is to ensure the FluentValidation `IsValidTransition` guard matches the actual domain methods, eliminate duplicate state mutation and refund logic, and clean up dead code and legacy duplication.

**Scope**: Payment module only. Domain layer (`PaymentCapture.Method.State.cs`, `PaymentCapture.Validation.cs`, `PaymentCapture.cs`, `PaymentMethod.cs`), Service layer (`PaymentProcessingService.cs`), Infrastructure layer (`StripeGateway.cs`), Background layer (`VoidOrderPayments.cs`), Features layer (`CreatePaymentIntent.cs`).

**Out of scope**: Gateway abstraction redesign, new Stripe API operations, integration test creation.

**Assumptions**: The existing state machine transitions in `PaymentCapture.Method.State.cs` are canonical and correct. The FluentValidation `IsValidTransition` is the guard used by feature-level validators when pre-validating state transitions before domain methods are called.

## 2. Definitions

| Term | Definition |
|---|---|
| State machine | The set of `PaymentRecordState` values and the `Result Transition(this PaymentCapture)` extension methods that enforce valid from/to pairs. |
| IsValidTransition | A `private static bool` in `PaymentCapture.Validation.cs` (L136-149) used by FluentValidation's `ApplyStateTransitionRules` to pre-validate state transitions at the feature handler level before domain methods execute. |
| Direct mutation | Setting `payment.State = ...` or `payment.SomeProperty = ...` from the service layer instead of calling the domain extension method (e.g., `payment.Complete()`). |
| Domain method | A `static Result Xxx(this PaymentCapture payment)` extension method in `PaymentCapture.Method.State.cs` that validates the transition, mutates state, sets `ModifiedAtUtc`, and returns a typed Result. |
| Deduplication | Removing a second copy of logic or code that already exists elsewhere, so there is a single source of truth. |

## 3. Requirements, Constraints & Guidelines

### Category A: Domain State Machine Integrity

- **STM-001**: `IsValidTransition` in `PaymentCapture.Validation.cs` MUST include all valid `(from, to)` pairs that the domain transition methods (`Fail`, `Dispute`) accept. Currently missing: `(Checkout, Failed)`, `(Checkout, Disputed)`, `(Processing, Disputed)`, `(Pending, Disputed)`, `(Completed, Disputed)`, `(Failed, Disputed)`.

- **STM-002**: `PaymentCapture.Capture(decimal amount)` MUST mutate `payment.State = PaymentRecordState.Completed` and `payment.ModifiedAtUtc = DateTimeOffset.UtcNow` before returning success. Currently it only validates and returns `Result.Ok()` without mutating state. The service `CaptureAsync()` currently bypasses it by writing `payment.State = Completed` directly.

- **STM-003**: `PaymentProcessingService.CaptureAsync()` MUST call `payment.Complete()` instead of `payment.State = PaymentRecordState.Completed`. The `Complete()` domain method already handles the `AlreadyCompleted` guard (idempotency) and sets `ModifiedAtUtc`.

- **STM-004**: `PaymentProcessingService.GatewayActionAsync()` (L284) and `VoidTransactionAsync()` (L141) MUST call `payment.Complete()`/`payment.Pend()`/`payment.Void()` instead of `payment.State = ...`. The service layer MUST NOT directly mutate `payment.State`.

- **STM-005**: The `IsValidTransition` switch expression at `PaymentCapture.Validation.cs:136-149` MUST be the single canonical list of valid transitions. Domain methods MUST NOT independently duplicate transition rules that conflict with this list. Any feature handler using `ApplyStateTransitionRules` for pre-validation must be able to trust this list.

### Category B: Deduplication

- **DED-001**: `PaymentProcessingService.RefundAsync()` (L107) MUST NOT directly increment `payment.RefundedAmount`. It MUST call `payment.Refund(amount)` which already increments `RefundedAmount` and sets `ModifiedAtUtc`. The service-level mutation duplicates the domain method and can drift.

- **DED-002**: The 6 error codes in `ProcessingResult.Errors` that duplicate identical codes in `PaymentCaptureResult.Failure` MUST be consolidated. Specifically: `AlreadyCompleted`, `AlreadyVoided`, `AmountExceedsAuthorized`, `ProcessingSourceRequired`, `ProcessingAlreadyProcessing`, `CreditNotAllowed`. These exists in both classes with the same `.Code` and `.Message` values. ProcessingResult.Errors should delegate to PaymentCaptureResult.Failure.

- **DED-003**: The legacy duplicate files in `Services/Abstractions/` MUST be deleted: `IPaymentGatewayActionProvider.cs`, `IGatewayRegistry.cs`, `IPaymentProcessingService.cs`. All consumers reference the canonical copies in `Services/Provider/` and `Services/Processing/`.

- **DED-004**: The `CentsMultiplier` constant (100) duplicated across `StripeGateway.cs:L11`, `BogusGateway.cs:L10`, and `ProcessStripeWebhookEventJob.cs:L117` (raw `/100m`) MUST be consolidated to a single constant in `GatewayConstants`.

### Category C: Edge Case Handling

- **EDG-001**: The `checked((long)Math.Round(amount * CentsMultiplier, ...))` casts in `StripeGateway.cs` (lines 101, 141, 180) MUST be guarded by a `MAX_SAFE_DOLLAR_AMOUNT` constant. If `amount > MAX_SAFE_DOLLAR_AMOUNT`, the method MUST return `StripeGatewayResult.Errors.AmountExceedsMaximum` before the `checked` cast. This prevents `OverflowException` from escaping the `catch (StripeException ex)` blocks and becoming an unhandled 500.

- **EDG-002**: `VoidOrderPayments.cs:L78-82` MUST remove the redundant `catch { await transaction.RollbackAsync(ct); throw; }` block. The `await using` declaration on the transaction auto-rolls back on unhandled exceptions. This is identical to the pattern already removed from `CreateOrderFromCart`.

- **EDG-003**: `BogusGateway.GetPaymentStatusAsync()` (L76) MUST NOT return `"succeeded"` when a `responseCode` is not found in `_intentStatuses`. It MUST return `"unknown"` to surface test setup errors instead of silently passing all status checks.

- **EDG-004**: `CreatePaymentIntent.CommandHandler` (L41-42) MUST accept an optional `PaymentMethodId` in the request. If no `PaymentMethodId` is provided, it MAY pick the first active payment method (current behavior). If provided, it MUST use that specific payment method. The current `FirstOrDefaultAsync(c => c.Active && !c.IsDeleted)` gives the user no control over payment method selection when multiple are active.

### Category D: Dead Code

- **DED-005**: The `CaptureEventCreated` property on `PaymentCapture` entity and `PaymentProcessingResult` DTO MUST be removed if never read in any business logic. Grep confirms it is only SET (L214 in `PaymentProcessingService.cs`, L10 in two DTOs, L24 in EF config) but never READ in any handler, controller, or mapping. If it is intended for future analytics, add a comment and move to a separate tracking concern.

## 4. Interfaces & Data Contracts

### 4.1 STM-001: IsValidTransition additions

```csharp
// PaymentCapture.Validation.cs:136-149 — add 6 missing arms
private static bool IsValidTransition(PaymentRecordState from, PaymentRecordState to) => (from, to) switch
{
    (PaymentRecordState.Checkout, PaymentRecordState.Processing) => true,
    (PaymentRecordState.Checkout, PaymentRecordState.Failed) => true,           // ADD
    (PaymentRecordState.Checkout, PaymentRecordState.Disputed) => true,         // ADD
    (PaymentRecordState.Processing, PaymentRecordState.Pending) => true,
    (PaymentRecordState.Processing, PaymentRecordState.Completed) => true,
    (PaymentRecordState.Processing, PaymentRecordState.Failed) => true,
    (PaymentRecordState.Processing, PaymentRecordState.Void) => true,
    (PaymentRecordState.Processing, PaymentRecordState.Disputed) => true,       // ADD
    (PaymentRecordState.Pending, PaymentRecordState.Completed) => true,
    (PaymentRecordState.Pending, PaymentRecordState.Failed) => true,
    (PaymentRecordState.Pending, PaymentRecordState.Void) => true,
    (PaymentRecordState.Pending, PaymentRecordState.Disputed) => true,          // ADD
    (PaymentRecordState.Completed, PaymentRecordState.Disputed) => true,        // ADD
    (PaymentRecordState.Failed, PaymentRecordState.Disputed) => true,           // ADD
    (PaymentRecordState.Failed, PaymentRecordState.Invalid) => true,
    (PaymentRecordState.Void, PaymentRecordState.Invalid) => true,
    _ => false
};
```

### 4.2 STM-002: Capture() domain method fix

```csharp
// PaymentCapture.Method.State.cs:119-129 — make Capture() mutate state
public static Result Capture(this PaymentCapture payment, decimal amount)
{
    if (!payment.CanCapture(amount))
    {
        return amount > payment.Amount
            ? PaymentCaptureResult.Failure.AmountExceedsAuthorized
            : PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);
    }
    payment.State = PaymentRecordState.Completed;
    payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
    return Result.Ok(PaymentCaptureResult.Success.Captured(payment.Number, amount));
}
```

### 4.3 STM-003/004: Replace direct State= with domain methods

```csharp
// PaymentProcessingService.cs:58 — CaptureAsync
// BEFORE:
payment.State = PaymentRecordState.Completed;
payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
return ProcessingResult.Success.Captured(payment.Number, amount.Value);

// AFTER:
var captureResult = payment.Capture(amount.Value);
if (captureResult.IsFailure) return Result<PaymentProcessingResult>.Failure(captureResult.Errors[0]);
payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
return ProcessingResult.Success.Captured(payment.Number, amount.Value);
```

```csharp
// PaymentProcessingService.cs:284 — GatewayActionAsync (called by Authorize, Purchase)
// BEFORE:
payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
payment.State = successState;
return successState == PaymentRecordState.Pending

// AFTER:
payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
// Delegate to domain method based on successState
var transitionResult = successState switch
{
    PaymentRecordState.Completed => payment.Complete(),
    PaymentRecordState.Pending => payment.Pend(),
    _ => Result.Ok()
};
if (transitionResult.IsFailure)
    return Result<PaymentProcessingResult>.Failure(transitionResult.Errors[0]);
return successState == PaymentRecordState.Pending
```

```csharp
// PaymentProcessingService.cs:141 — VoidTransactionAsync
// BEFORE:
payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
payment.State = PaymentRecordState.Void;

// AFTER:
payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
var voidResult = payment.Void();
if (voidResult.IsFailure)
    return Result<PaymentProcessingResult>.Failure(voidResult.Errors[0]);
```

```csharp
// PaymentProcessingService.cs:230 — CancelAsync
// BEFORE:
payment.State = PaymentRecordState.Void;

// AFTER:
var voidResult = payment.Void();
if (voidResult.IsFailure)
    return Result<PaymentProcessingResult>.Failure(voidResult.Errors[0]);
```

### 4.4 DED-001: RefundAsync uses domain Refund()

```csharp
// PaymentProcessingService.cs:89-112 — RefundAsync
public async Task<Result<PaymentProcessingResult>> RefundAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal amount, CancellationToken ct = default)
{
    if (payment.State is PaymentRecordState.Disputed)
        return ProcessingResult.Errors.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

    if (!payment.CanRefund(amount))
    {
        if (payment.State is not PaymentRecordState.Completed)
            return ProcessingResult.Errors.InvalidStateTransition(payment.State, PaymentRecordState.Completed);
        return ProcessingResult.Errors.AmountExceedsAuthorized;
    }

    var result = await CreditAsync(payment, gateway, options, amount, ct).ConfigureAwait(false);
    if (result.IsSuccess)
    {
        var refundResult = payment.Refund(amount);
        if (refundResult.IsFailure)
            return Result<PaymentProcessingResult>.Failure(refundResult.Errors[0]);
    }

    return result;
}
```

### 4.5 DED-002: Consolidate duplicate error codes

`ProcessingResult.Errors` delegates to `PaymentCaptureResult.Failure`:

```csharp
// ProcessingResult.Errors — change from local definitions to delegation
public static class Errors
{
    public static Error InvalidStateTransition(PaymentRecordState from, PaymentRecordState to)
        => PaymentCaptureResult.Failure.InvalidStateTransition(from, to);

    public static Error AlreadyCompleted
        => PaymentCaptureResult.Failure.AlreadyCompleted;

    public static Error AlreadyVoided
        => PaymentCaptureResult.Failure.AlreadyVoided;

    public static Error AmountExceedsAuthorized
        => PaymentCaptureResult.Failure.AmountExceedsAuthorized;

    public static Error ProcessingSourceRequired
        => PaymentCaptureResult.Failure.ProcessingSourceRequired;

    public static Error ProcessingAlreadyProcessing
        => PaymentCaptureResult.Failure.ProcessingAlreadyProcessing;

    public static Error CreditNotAllowed
        => PaymentCaptureResult.Failure.CreditNotAllowed;

    // Unique to processing layer — keep local
    public static Error GatewayDeclined(string detail) => Error.BadRequest(
        code: "Payment.Gateway.Declined",
        message: detail);
}
```

### 4.6 DED-003: Delete legacy Abstractions

Delete these files:
```
service/Api/src/Module/Payment/Services/Abstractions/IPaymentGatewayActionProvider.cs
service/Api/src/Module/Payment/Services/Abstractions/IGatewayRegistry.cs
service/Api/src/Module/Payment/Services/Abstractions/IPaymentProcessingService.cs
```

Verify no remaining consumers via `rg` on each namespace before deletion.

### 4.7 DED-004: Cents constant consolidation

```csharp
// GatewayConstants.cs — add
public static class Amounts
{
    public const long CentsMultiplier = 100;
    public const decimal MaxSafeDollarAmount = 92_233_720_368_547_758.07m;
}
```

Replace `private const long CentsMultiplier = 100;` in `StripeGateway.cs:L11` and `BogusGateway.cs:L10` with `GatewayConstants.Amounts.CentsMultiplier`.

Replace `charge.AmountRefunded / 100m` in `ProcessStripeWebhookEventJob.cs:L117` with `charge.AmountRefunded / GatewayConstants.Amounts.CentsMultiplier`.

### 4.8 EDG-001: Amount overflow guard

```csharp
// StripeGateway.cs — add MaxSafeDollarAmount to CentsMultiplier area (L11)
private const decimal MaxSafeDollarAmount = GatewayConstants.Amounts.MaxSafeDollarAmount;

// StripeGateway.cs — inside CreatePaymentIntentOptions, before Amount assignment
if (amount > MaxSafeDollarAmount)
    return StripeGatewayResult.Errors.AmountExceedsMaximum;

// StripeGateway.Result.cs — add new error factory
public static Result<PaymentGatewayResponse> AmountExceedsMaximum =>
    Result<PaymentGatewayResponse>.Failure(
        Error.Validation(
            "Stripe.Amount.ExceedsMaximum",
            $"Payment amount exceeds the maximum supported value."));
```

Apply the same guard in `CaptureAsync` and `RefundAsync` before their respective `checked` casts.

### 4.9 EDG-002: Remove redundant rollback

```csharp
// VoidOrderPayments.cs:74-83 — delete the catch block entirely
// BEFORE:
await dbContext.SaveChangesAsync(ct);
await transaction.CommitAsync(ct);
return Result.Ok();
}
catch
{
    // Catch: Roll back on unexpected exception — payment state remains unchanged
    await transaction.RollbackAsync(ct);
    throw;
}

// AFTER:
await dbContext.SaveChangesAsync(ct);
await transaction.CommitAsync(ct);
return Result.Ok();
}
```

`await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);` auto-rolls back on exception.

### 4.10 EDG-003: BogusGateway unknown code

```csharp
// BogusGateway.cs:73-78 — change default
public override Task<string> GetPaymentStatusAsync(string responseCode, CancellationToken ct = default)
{
    if (_intentStatuses.TryGetValue(responseCode, out var status))
        return Task.FromResult(status);
    return Task.FromResult("unknown");
}
```

### 4.11 EDG-004: PaymentMethodId in CreatePaymentIntent request

```csharp
// CreatePaymentIntent.Request.cs — add optional PaymentMethodId
public sealed record Request
{
    public Guid OrderId { get; init; }
    public Guid? PaymentMethodId { get; init; }
}
```

```csharp
// CreatePaymentIntent.cs:40-44 — use provided PaymentMethodId or fallback to first active
// BEFORE:
var paymentMethod = await dbContext.Set<PaymentMethod>()
    .FirstOrDefaultAsync(c => c.Active && !c.IsDeleted, cancellationToken);

// AFTER:
var paymentMethod = command.PaymentMethodId.HasValue
    ? await dbContext.Set<PaymentMethod>()
        .FirstOrDefaultAsync(c => c.Id == command.PaymentMethodId.Value && c.Active && !c.IsDeleted, cancellationToken)
    : await dbContext.Set<PaymentMethod>()
        .FirstOrDefaultAsync(c => c.Active && !c.IsDeleted, cancellationToken);
```

### 4.12 DED-005: Remove CaptureEventCreated

Remove `CaptureEventCreated` from these locations:
- `PaymentCapture.cs:L22` — property declaration
- `PaymentProcessingService.cs:L214` — `payment.CaptureEventCreated = true;` assignment
- `PaymentProcessingResult.cs:L10` — DTO property
- `Services/Models/PaymentProcessingResult.cs:L10` — legacy duplicate DTO
- `PaymentRecordConfiguration.cs:L24` — EF config

## 5. Acceptance Criteria

- **AC-001**: Given `IsValidTransition((PaymentRecordState.Checkout, PaymentRecordState.Failed))`, Then it returns `true`.
- **AC-002**: Given `IsValidTransition((PaymentRecordState.Completed, PaymentRecordState.Disputed))`, Then it returns `true`.
- **AC-003**: Given `payment.Capture(amount)` succeeds, Then `payment.State == PaymentRecordState.Completed` and `payment.ModifiedAtUtc` is set.
- **AC-004**: Given `rg "payment.State = PaymentRecordState\." service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs`, Then 0 matches (all replaced by domain method calls).
- **AC-005**: Given `rg "payment.RefundedAmount \+= amount" service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs`, Then 0 matches.
- **AC-006**: Given `rg "AlreadyCompleted|AlreadyVoided|AmountExceedsAuthorized|ProcessingSourceRequired|ProcessingAlreadyProcessing|CreditNotAllowed" service/Api/src/Module/Payment/Services/Processing/PaymentProcessingResult.cs`, Then each code delegates to `PaymentCaptureResult.Failure.*`.
- **AC-007**: Directory `service/Api/src/Module/Payment/Services/Abstractions/` does not exist. `dotnet build service/Api/src/Module` passes.
- **AC-008**: Given `rg "const.*CentsMultiplier" service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs`, Then 0 matches (uses `GatewayConstants.Amounts.CentsMultiplier`).
- **AC-009**: Given an amount > `92_233_720_368_547_758m`, When `CreatePaymentIntentOptions` runs, Then it returns `Stripe.Amount.ExceedsMaximum` error before the `checked` cast.
- **AC-010**: Given `rg "transaction\.RollbackAsync" service/Api/src/Module/Payment/Features/Shared/Commands/VoidOrderPayments.cs`, Then 0 matches.
- **AC-011**: Given `BogusGateway.GetPaymentStatusAsync("nonexistent_code")`, Then it returns `"unknown"`, not `"succeeded"`.
- **AC-012**: Given `CreatePaymentIntent.Request` has `PaymentMethodId = someGuid`, Then the handler queries for that specific payment method, not the first active one.
- **AC-013**: Given `rg "CaptureEventCreated" service/Api/src/Module/Payment/`, Then 0 matches.
- **AC-014**: `dotnet build service/Api/src/Module` passes with 0 warnings.
- **AC-015**: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Payment"` passes all tests.

## 6. Test Automation Strategy

- **Test Levels**: Unit tests for domain state machine, service layer, gateway, and integration.
- **Frameworks**: xUnit, FluentAssertions, Moq.
- **Test files to create/modify**:
  - `PaymentDisputeTests.cs`: Add tests verifying `Dispute()` from `Checkout`, `Processing`, `Pending`, `Completed`, `Failed` states all succeed; from `Void`, `Invalid` fail.
  - `PaymentValidationTests.cs` (new): Test `IsValidTransition` for all 16 arms, including the 6 newly added.
  - `PaymentCaptureTests.cs` (new or extend existing): Test `Capture()` sets state to `Completed`.
  - `PaymentProcessingServiceTests.cs`: Update tests for `CaptureAsync` and `RefundAsync` to verify domain methods are called instead of direct mutations. Mock gateway to succeed, then assert `payment.State` changes via domain transitions.
  - `BogusGatewayTests.cs`: Add test for `GetPaymentStatusAsync` with unknown code returns `"unknown"`.
  - `CreatePaymentIntentTests.cs`: Add test for explicit `PaymentMethodId` in request.
  - `StripeGatewayTests.cs`: Test amount overflow guard returns error.
- **CI/CD Integration**: `dotnet test --filter "Category=Unit"` in GitHub Actions.
- **Coverage Requirements**: All 13 changes must have at least 1 unit test.

## 7. Rationale & Context

### STM-001: Why missing transitions matter

`ApplyStateTransitionRules` is used by feature-level validators (FluentValidation `Must(target => IsValidTransition(current, target))`). Any feature handler that pre-validates state transitions before calling domain methods will reject valid operations if the transition isn't in `IsValidTransition`. For example, a future handler that tries to dispute a Completed payment would pass the domain `Dispute()` method's guard but fail FluentValidation pre-validation, returning a confusing "Invalid state transition" error.

### STM-002 through STM-005: Why domain methods must be the sole mutators

The `Result` object pattern requires that all state mutations happen through domain methods that:
1. Validate the transition
2. Set `ModifiedAtUtc` for auditing
3. Return a typed `Result` with the correct success/failure code

When the service layer writes `payment.State = Completed` directly, it bypasses all three. The `AlreadyCompleted` idempotency guard in `Complete()` only works if `Complete()` is called. Direct mutation means calling `CaptureAsync` twice on the same payment would silently succeed both times, producing an incorrect audit trail.

### DED-001: Why refund duplication is risky

`RefundAsync()` increments `RefundedAmount` at L107, but the `Refund()` domain method also increments it at L146. The webhook handler `HandleChargeRefunded` calls `payment.Refund(delta)` which uses the domain method. If `RefundAsync()` is called, it increments via service code. If later `HandleChargeRefunded` processes a partial refund webhook for the same payment, it calls `payment.Refund(delta)` which increment via domain code. Having two code paths for the same mutation means a future change to one won't affect the other, creating subtle drift bugs.

### EDG-001: Why checked overflow is dangerous

`checked((long)Math.Round(amount * CentsMultiplier))` is inside `PurchaseAsync`/`CaptureAsync`/`RefundAsync`/`CreatePaymentIntentOptions`, all of which catch `StripeException` but NOT `OverflowException`. An extremely large amount (e.g., from a bug in the order calculation) would throw `OverflowException` that escapes the catch block → unhandled exception → HTTP 500. A guard before the cast avoids this.

### EDG-003: Why BogusGateway default matters

In tests, a typo in the auth code (e.g., `auth_abc` instead of `auth_acb`) would make `GetPaymentStatusAsync` return `"succeeded"` — the test would pass but test the wrong code path. Returning `"unknown"` makes the bug immediately visible.

## 8. Dependencies & External Integrations

### External Systems
- None.

### Third-Party Services
- None.

### Infrastructure Dependencies
- **INF-001**: EF Core — removing `CaptureEventCreated` from `PaymentRecordConfiguration.cs` requires an EF Core migration.

### Data Dependencies
- None.

### Technology Platform Dependencies
- **PLT-001**: .NET 10 — no version changes.
- **PLT-002**: Stripe.net 52.1.0 — no version changes.

### Compliance Dependencies
- None.

## 9. Examples & Edge Cases

### State machine validation before and after

```
Given: payment.State = Completed
When: Feature handler calls ApplyStateTransitionRules(target: Disputed)
Before fix: IsValidTransition(Completed, Disputed) returns false → validation fails → "Cannot transition payment from 'Completed' to 'Disputed'"
After fix: IsValidTransition(Completed, Disputed) returns true → validation passes → handler calls payment.Dispute() → succeeds
```

### RefundAsync double-increment protection

```
Given: payment.Amount = 100, payment.RefundedAmount = 0
When: RefundAsync called for 50
Before fix: Service does payment.RefundedAmount += 50 → RefundedAmount = 50
            If payment.Refund(50) is ALSO called (by webhook later) → RefundedAmount = 100 (double-counted)
After fix: Service calls payment.Refund(50) which increments internally → RefundedAmount = 50
           Webhook calls payment.Refund(25) → domain method computes CanRefund correctly → RefundedAmount = 75 ✓
```

### Unhandled OverflowException

```
Given: amount = 100_000_000_000_000_000m (1e17)
When: PurchaseAsync processes this amount
Before fix: checked() throws OverflowException → not caught by catch(StripeException) → HTTP 500
After fix: amount > MaxSafeDollarAmount → returns AmountExceedsMaximum error → HTTP 422
```

## 10. Validation Criteria

- **VC-001**: `dotnet build service/Api/src/Module` passes with 0 warnings.
- **VC-002**: `dotnet test service/Api/tests/Module.UnitTests` passes all tests.
- **VC-003**: `rg "IsValidTransition" service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Validation.cs` shows 17+ switch arms (11 original + 6 new).
- **VC-004**: `rg "payment.State = PaymentRecordState\." service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs` returns 0 matches.
- **VC-005**: `rg "payment\.Capture\(amount" service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs` returns 1 match (in CaptureAsync).
- **VC-006**: `rg "payment\.Refund\(amount" service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs` returns 1 match (in RefundAsync).
- **VC-007**: Directory `service/Api/src/Module/Payment/Services/Abstractions/` does not exist.
- **VC-008**: `rg "const.*CentsMultiplier" service/Api/src/Module/Payment/Services/Provider/` returns 0 matches.
- **VC-009**: `BogusGateway.GetPaymentStatusAsync("nonexistent")` returns `"unknown"` in unit test.
- **VC-010**: `rg "CaptureEventCreated" service/Api/src/Module/Payment/` returns 0 matches.

## 11. Related Specifications / Further Reading

- [spec-design-stripe-integration-completion.md](/spec/spec-design-stripe-integration-completion.md) — Prior round (3DS, transient errors, dispute, hardening)
- [spec-design-stripe-checkout-flow-fixes.md](/spec/spec-design-stripe-checkout-flow-fixes.md) — Prior round (ReturnUrl, PaymentStatus, webhook gaps)
- [payment-caveman-review.md](/.superpowers/sdd/payment-caveman-review.md) — Source review that identified these findings
- [.harness/principles.yml](/.harness/principles.yml) — Result objects, not exceptions principle
- [PaymentCapture.Method.State.cs](/service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Method.State.cs)
- [PaymentProcessingService.cs](/service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs)
