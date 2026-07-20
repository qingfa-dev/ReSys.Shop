# Payment Domain State Machine Integrity and Deduplication Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix 13 caveman review findings: domain state machine gaps (6 missing transitions, Capture() doesn't mutate state, 4 direct State= mutations), deduplication (refund split, 6 duplicate error codes, 3 legacy interface files, cents constant duplication, dead CaptureEventCreated field), and edge cases (overflow guard, redundant rollback, BogusGateway false-success, arbitrary first payment method).

**Architecture:** Two-phase approach. Phase 1 (Tasks 1-2): fix domain layer first — IsValidTransition + Capture() domain method. Phase 2 (Tasks 3-5): fix service layer — replace all direct `payment.State =` mutations with domain method calls. Phase 3 (Tasks 6-12): deduplication and edge cases in any order. Phase 4 (Task 13): test updates. Phase 5 (Task 14): cleanup and verification.

**Tech Stack:** .NET 10, FluentAssertions, Moq, xUnit, EF Core InMemory.

## Global Constraints

- `TreatWarningsAsErrors=true` — zero warnings on `dotnet build`
- All changes confined to Payment module
- Domain methods are the sole mutators — service layer never writes `payment.State =` directly
- Commit after each task builds clean
- `dotnet test service/Api/tests/Module.UnitTests` must pass after each task

---

### Task 1: Fix IsValidTransition — Add 6 Missing Transitions

**Files:**
- Modify: `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Validation.cs:136-149`

**Interfaces:**
- Consumes: `PaymentRecordState` enum (all 8 values)
- Produces: `IsValidTransition(PaymentRecordState, PaymentRecordState) → bool` covering all 16 valid transitions, including 6 new: `(Checkout,Failed)`, `(Checkout,Disputed)`, `(Processing,Disputed)`, `(Pending,Disputed)`, `(Completed,Disputed)`, `(Failed,Disputed)`.

- [ ] **Step 1: Add the 6 missing switch arms**

Open `PaymentCapture.Validation.cs`. Replace lines 136-149 with:

```csharp
private static bool IsValidTransition(PaymentRecordState from, PaymentRecordState to) => (from, to) switch
{
    (PaymentRecordState.Checkout, PaymentRecordState.Processing) => true,
    (PaymentRecordState.Checkout, PaymentRecordState.Failed) => true,
    (PaymentRecordState.Checkout, PaymentRecordState.Disputed) => true,
    (PaymentRecordState.Processing, PaymentRecordState.Pending) => true,
    (PaymentRecordState.Processing, PaymentRecordState.Completed) => true,
    (PaymentRecordState.Processing, PaymentRecordState.Failed) => true,
    (PaymentRecordState.Processing, PaymentRecordState.Void) => true,
    (PaymentRecordState.Processing, PaymentRecordState.Disputed) => true,
    (PaymentRecordState.Pending, PaymentRecordState.Completed) => true,
    (PaymentRecordState.Pending, PaymentRecordState.Failed) => true,
    (PaymentRecordState.Pending, PaymentRecordState.Void) => true,
    (PaymentRecordState.Pending, PaymentRecordState.Disputed) => true,
    (PaymentRecordState.Completed, PaymentRecordState.Disputed) => true,
    (PaymentRecordState.Failed, PaymentRecordState.Disputed) => true,
    (PaymentRecordState.Failed, PaymentRecordState.Invalid) => true,
    (PaymentRecordState.Void, PaymentRecordState.Invalid) => true,
    _ => false
};
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Validation.cs
git commit -m "fix(payment): add 6 missing transitions to IsValidTransition — Fail and Dispute paths"
```

---

### Task 2: Fix Capture() Domain Method — Make It Mutate State

**Files:**
- Modify: `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Method.State.cs:119-129`

**Interfaces:**
- Consumes: None (domain method only)
- Produces: `Capture(decimal amount)` now sets `payment.State = Completed` and `payment.ModifiedAtUtc` before returning `Result.Ok()`.

- [ ] **Step 1: Make Capture() mutate state**

Open `PaymentCapture.Method.State.cs`. Replace lines 119-129 with:

```csharp
// Update: Capture amount — validates CanCapture precondition, transitions to Completed
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

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Method.State.cs
git commit -m "fix(payment): make Capture() domain method set State=Completed and ModifiedAtUtc"
```

---

### Task 3: Replace Direct State Mutations in PaymentProcessingService

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs:50-60`
- Modify: `service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs:128-145`
- Modify: `service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs:220-231`
- Modify: `service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs:267-289`

**Interfaces:**
- Consumes: `payment.Capture(amount)` (Task 2), `payment.Complete()`, `payment.Pend()`, `payment.Void()` (existing domain methods)
- Produces: Zero direct `payment.State =` assignments in `PaymentProcessingService.cs`. All state transitions via domain methods.

- [ ] **Step 1: Replace CaptureAsync direct mutation with payment.Capture()**

Open `PaymentProcessingService.cs`. Replace lines 56-60:

```csharp
// BEFORE:
var response = gatewayResult.Value;
RecordGatewayResponse(payment, response);
payment.State = PaymentRecordState.Completed;
payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
return ProcessingResult.Success.Captured(payment.Number, amount.Value);

// AFTER:
var response = gatewayResult.Value;
RecordGatewayResponse(payment, response);
var captureResult = payment.Capture(amount.Value);
if (captureResult.IsFailure)
    return Result<PaymentProcessingResult>.Failure(captureResult.Errors[0]);
payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
return ProcessingResult.Success.Captured(payment.Number, amount.Value);
```

- [ ] **Step 2: Replace VoidTransactionAsync direct mutation with payment.Void()**

Replace lines 132-135:

```csharp
// BEFORE:
var response = gatewayResult.Value;
RecordGatewayResponse(payment, response);
payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
payment.State = PaymentRecordState.Void;
return ProcessingResult.Success.Voided(payment.Number);

// AFTER:
var response = gatewayResult.Value;
RecordGatewayResponse(payment, response);
payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
var voidResult = payment.Void();
if (voidResult.IsFailure)
    return Result<PaymentProcessingResult>.Failure(voidResult.Errors[0]);
return ProcessingResult.Success.Voided(payment.Number);
```

- [ ] **Step 3: Replace CancelAsync direct mutation with payment.Void()**

Replace lines 228-231:

```csharp
// BEFORE:
var response = gatewayResult.Value;
RecordGatewayResponse(payment, response);
payment.State = PaymentRecordState.Void;
return ProcessingResult.Success.Voided(payment.Number);

// AFTER:
var response = gatewayResult.Value;
RecordGatewayResponse(payment, response);
var voidResult = payment.Void();
if (voidResult.IsFailure)
    return Result<PaymentProcessingResult>.Failure(voidResult.Errors[0]);
return ProcessingResult.Success.Voided(payment.Number);
```

- [ ] **Step 4: Replace GatewayActionAsync direct mutation with domain methods**

Replace lines 283-289:

```csharp
// BEFORE:
var response = gatewayResult.Value;
RecordGatewayResponse(payment, response);
payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
payment.State = successState;
return successState == PaymentRecordState.Pending
    ? ProcessingResult.Success.Pended(payment.Number)
    : ProcessingResult.Success.Completed(payment.Number);

// AFTER:
var response = gatewayResult.Value;
RecordGatewayResponse(payment, response);
payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
var transitionResult = successState switch
{
    PaymentRecordState.Completed => payment.Complete(),
    PaymentRecordState.Pending => payment.Pend(),
    _ => Result.Ok()
};
if (transitionResult.IsFailure)
    return Result<PaymentProcessingResult>.Failure(transitionResult.Errors[0]);
return successState == PaymentRecordState.Pending
    ? ProcessingResult.Success.Pended(payment.Number)
    : ProcessingResult.Success.Completed(payment.Number);
```

- [ ] **Step 5: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs
git commit -m "fix(payment): replace 4 direct State= mutations with domain methods (Capture/Complete/Pend/Void)"
```

---

### Task 4: Fix RefundAsync — Use Domain Refund() Instead of Direct Increment

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs:89-112`

**Interfaces:**
- Consumes: `payment.Refund(amount)` (existing domain method at `PaymentCapture.Method.State.cs:137`)
- Produces: `RefundAsync` delegates `RefundedAmount` increment to domain method. Single source of truth.

- [ ] **Step 1: Replace direct RefundedAmount increment with payment.Refund()**

Open `PaymentProcessingService.cs`. Replace lines 103-111:

```csharp
// BEFORE:
// Call: Gateway refund API — Stripe Refund Create
var result = await CreditAsync(payment, gateway, options, amount, ct).ConfigureAwait(false);
if (result.IsSuccess)
{
    payment.RefundedAmount += amount;
    payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
}

return result;

// AFTER:
// Call: Gateway refund API — Stripe Refund Create
var result = await CreditAsync(payment, gateway, options, amount, ct).ConfigureAwait(false);
if (result.IsSuccess)
{
    var refundResult = payment.Refund(amount);
    if (refundResult.IsFailure)
        return Result<PaymentProcessingResult>.Failure(refundResult.Errors[0]);
}

return result;
```

The full `RefundAsync` method should now be (L89-112):

```csharp
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

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs
git commit -m "fix(payment): RefundAsync delegates to domain Refund() — single source of truth for RefundedAmount"
```

---

### Task 5: Consolidate Duplicate Error Codes in ProcessingResult.Errors

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Processing/PaymentProcessingResult.cs:58-91`

**Interfaces:**
- Consumes: `PaymentCaptureResult.Failure` existing error factories
- Produces: `ProcessingResult.Errors` delegates 6 duplicate codes to `PaymentCaptureResult.Failure`. No downstream breakage — same `.Code` and `.Message` values.

- [ ] **Step 1: Replace local error definitions with delegation**

Open `PaymentProcessingResult.cs` (`Services/Processing/`). Replace lines 58-91 with:

```csharp
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

    public static Error GatewayDeclined(string detail) => Error.BadRequest(
        code: "Payment.Gateway.Declined",
        message: detail);
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Run Payment processing tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~PaymentProcessingService|Refund|Capture|Void"
```

Expected: All tests pass (error codes unchanged).

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Processing/PaymentProcessingResult.cs
git commit -m "refactor(payment): consolidate 6 duplicate error codes to PaymentCaptureResult.Failure"
```

---

### Task 6: Delete Legacy Abstractions + Models Duplicates

**Files:**
- Delete: `service/Api/src/Module/Payment/Services/Abstractions/IPaymentGatewayActionProvider.cs`
- Delete: `service/Api/src/Module/Payment/Services/Abstractions/IGatewayRegistry.cs`
- Delete: `service/Api/src/Module/Payment/Services/Abstractions/IPaymentProcessingService.cs`
- Delete: `service/Api/src/Module/Payment/Services/Models/PaymentProcessingResult.cs`
- Delete: `service/Api/src/Module/Payment/Services/Models/PaymentGatewayResponse.cs` (if present)

**Interfaces:**
- None — no consumers remain for these legacy files.

- [ ] **Step 1: Verify no consumers (read-only check)**

```bash
rg "Services\.Abstractions\.IPaymentGatewayActionProvider" service/Api/src/Module/ | wc -l
rg "Services\.Abstractions\.IGatewayRegistry" service/Api/src/Module/ | wc -l
rg "Services\.Abstractions\.IPaymentProcessingService" service/Api/src/Module/ | wc -l
```

Each should return 1 (the file itself) or 0.

- [ ] **Step 2: Check Services/Models/ consumers**

```bash
rg "Services\.Models\.PaymentProcessingResult" service/Api/src/Module/ | wc -l
rg "Services\.Models\.PaymentGatewayResponse" service/Api/src/Module/ | wc -l
```

Expected: 1 or 0 for each.

- [ ] **Step 3: Delete the files**

```bash
rm service/Api/src/Module/Payment/Services/Abstractions/IPaymentGatewayActionProvider.cs
rm service/Api/src/Module/Payment/Services/Abstractions/IGatewayRegistry.cs
rm service/Api/src/Module/Payment/Services/Abstractions/IPaymentProcessingService.cs
rm service/Api/src/Module/Payment/Services/Models/PaymentProcessingResult.cs
# Only if it exists:
rm -f service/Api/src/Module/Payment/Services/Models/PaymentGatewayResponse.cs
rmdir service/Api/src/Module/Payment/Services/Abstractions/ 2>/dev/null
rmdir service/Api/src/Module/Payment/Services/Models/ 2>/dev/null
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 5: Commit**

```bash
git add -A service/Api/src/Module/Payment/Services/Abstractions/
git add -A service/Api/src/Module/Payment/Services/Models/
git commit -m "refactor(payment): delete legacy duplicate interfaces and models"
```

---

### Task 7: Consolidate CentsMultiplier to GatewayConstants.Amounts

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/GatewayConstants.cs:39-57`
- Modify: `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs:11`
- Modify: `service/Api/src/Module/Payment/Services/Provider/Bogus/BogusGateway.cs:10`
- Modify: `service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs:117`

**Interfaces:**
- Produces: `GatewayConstants.Amounts.CentsMultiplier` (long = 100), `GatewayConstants.Amounts.MaxSafeDollarAmount` (decimal).
- Consumed by: StripeGateway, BogusGateway, ProcessStripeWebhookEventJob.

- [ ] **Step 1: Add Amounts constants to GatewayConstants**

Open `GatewayConstants.cs`. Add after the `Bogus` class (after line 69), before `Webhook`:

```csharp
public static class Amounts
{
    public const long CentsMultiplier = 100;
    public const decimal MaxSafeDollarAmount = 92_233_720_368_547_758.07m;
}
```

- [ ] **Step 2: Replace StripeGateway CentsMultiplier**

Open `StripeGateway.cs`. Replace line 11:

```csharp
// BEFORE:
private const long CentsMultiplier = 100;

// AFTER — delete line 11. Replace all CentsMultiplier references:
// StripeGateway.cs: use GatewayConstants.Amounts.CentsMultiplier
```

Replace all 6 references to `CentsMultiplier` in StripeGateway.cs:
- L98: `checked((long)Math.Round(amount * GatewayConstants.Amounts.CentsMultiplier, MidpointRounding.AwayFromZero))`
- L137: `checked((long)Math.Round(amount * GatewayConstants.Amounts.CentsMultiplier, MidpointRounding.AwayFromZero))`
- L180: `checked((long)Math.Round(amount * GatewayConstants.Amounts.CentsMultiplier, MidpointRounding.AwayFromZero))`

- [ ] **Step 3: Replace BogusGateway CentsMultiplier**

Open `BogusGateway.cs`. Replace line 10:

```csharp
// BEFORE:
private const long CentsMultiplier = 100;

// AFTER — delete the field. If it's used, replace with GatewayConstants.Amounts.CentsMultiplier.
```

Check if `CentsMultiplier` is referenced anywhere else in BogusGateway.cs:

```bash
rg "CentsMultiplier" service/Api/src/Module/Payment/Services/Provider/Bogus/BogusGateway.cs
```

If only the field declaration, delete it. If used in code elsewhere, replace.

- [ ] **Step 4: Replace raw /100m in ProcessStripeWebhookEventJob**

Open `ProcessStripeWebhookEventJob.cs`. Line 117:

```csharp
// BEFORE:
var newRefunded = charge.AmountRefunded / 100m;

// AFTER:
var newRefunded = charge.AmountRefunded / GatewayConstants.Amounts.CentsMultiplier;
```

- [ ] **Step 5: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Provider/GatewayConstants.cs
git add service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs
git add service/Api/src/Module/Payment/Services/Provider/Bogus/BogusGateway.cs
git add service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs
git commit -m "refactor(payment): consolidate CentsMultiplier to GatewayConstants.Amounts"
```

---

### Task 8: Add Amount Overflow Guard Before checked{} Cast

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs:98,137,180` (3 sites)
- Modify: `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.Result.cs:31-41`

**Interfaces:**
- Consumes: `GatewayConstants.Amounts.MaxSafeDollarAmount` (Task 7)
- Produces: `StripeGatewayResult.Errors.AmountExceedsMaximum` error. All 3 amount-to-cents conversions guarded.

- [ ] **Step 1: Add AmountExceedsMaximum error factory**

Open `StripeGateway.Result.cs`. Add after `PaymentMethodRequired` (line 41):

```csharp
public static Result<PaymentGatewayResponse> AmountExceedsMaximum =>
    Result<PaymentGatewayResponse>.Failure(
        Error.Validation(
            "Stripe.Amount.ExceedsMaximum",
            "Payment amount exceeds the maximum supported value."));
```

- [ ] **Step 2: Add guard to CreatePaymentIntentOptions** (called by PurchaseAsync + AuthorizeAsync)

Open `StripeGateway.cs`. In `CreatePaymentIntentOptions`, before the `Amount` assignment (before L180), add:

```csharp
if (amount > GatewayConstants.Amounts.MaxSafeDollarAmount)
    return StripeGatewayResult.Errors.AmountExceedsMaximum;
```

Wait — `CreatePaymentIntentOptions` returns `PaymentIntentCreateOptions`, not `Result<PaymentGatewayResponse>`. It can't return an error. The guard needs to be placed in `PurchaseAsync` and `AuthorizeAsync` BEFORE calling `CreatePaymentIntentOptions`.

Instead, add the guard to `PurchaseAsync` (after L43, before `CreatePaymentIntentOptions` call):

```csharp
if (amount > GatewayConstants.Amounts.MaxSafeDollarAmount)
    return StripeGatewayResult.Errors.AmountExceedsMaximum;
```

Add the same guard to `AuthorizeAsync` (after L74, before `CreatePaymentIntentOptions` call).

- [ ] **Step 3: Add guard to CaptureAsync**

After the `ResponseCode` null check (after L94), before the `checked` cast:

```csharp
if (amount > GatewayConstants.Amounts.MaxSafeDollarAmount)
    return StripeGatewayResult.Errors.AmountExceedsMaximum;
```

- [ ] **Step 4: Add guard to RefundAsync**

After the `ResponseCode` null check (after L132), before the `checked` cast:

```csharp
if (amount > GatewayConstants.Amounts.MaxSafeDollarAmount)
    return StripeGatewayResult.Errors.AmountExceedsMaximum;
```

- [ ] **Step 5: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs
git add service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.Result.cs
git commit -m "fix(payment): add amount overflow guard before checked{} casts in StripeGateway"
```

---

### Task 9: Remove Redundant catch{rollback} from VoidOrderPayments

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Shared/Commands/VoidOrderPayments.cs:78-83`

**Interfaces:**
- None — `await using var transaction = await dbContext.Database.BeginTransactionAsync(ct)` auto-rolls back on exception.

- [ ] **Step 1: Delete the redundant catch block**

Open `VoidOrderPayments.cs`. Replace lines 74-83:

```csharp
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

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Shared/Commands/VoidOrderPayments.cs
git commit -m "fix(payment): remove redundant catch{rollback} from VoidOrderPayments — await using auto-rolls back"
```

---

### Task 10: Fix BogusGateway — Return "unknown" Not "succeeded"

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/Bogus/BogusGateway.cs:73-78`

**Interfaces:**
- Produces: `GetPaymentStatusAsync` returns `"unknown"` for unrecognized auth codes instead of falsely returning `"succeeded"`.

- [ ] **Step 1: Change the default return value**

Open `BogusGateway.cs`. Replace lines 76-77:

```csharp
// BEFORE:
return Task.FromResult("succeeded");

// AFTER:
return Task.FromResult("unknown");
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Run BogusGateway tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~BogusGateway"
```

Expected: All tests pass. If any test asserts `"succeeded"` for an unrecognized code, fix the test.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Provider/Bogus/BogusGateway.cs
git commit -m "fix(payment): BogusGateway returns unknown not succeeded for unrecognized auth codes"
```

---

### Task 11: Add Optional PaymentMethodId to CreatePaymentIntent

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.Request.cs:7-10`
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs:40-44`

**Interfaces:**
- Consumes: `Guid? PaymentMethodId` in request
- Produces: Handler uses specific payment method if provided, otherwise falls back to first active.

- [ ] **Step 1: Add PaymentMethodId to the request DTO**

Open `CreatePaymentIntent.Request.cs`. Add the property:

```csharp
public record Request : StorePaymentRequest
{
    public string? ReturnUrl { get; init; }
    public Guid? PaymentMethodId { get; init; }
}
```

- [ ] **Step 2: Update handler to use specific PaymentMethodId**

Open `CreatePaymentIntent.cs`. Replace lines 41-42:

```csharp
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

- [ ] **Step 3: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.Request.cs
git add service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs
git commit -m "feat(payment): add optional PaymentMethodId to CreatePaymentIntent request"
```

---

### Task 12: Remove Dead CaptureEventCreated Field

**Files:**
- Modify: `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.cs:22`
- Modify: `service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs:213-214`
- Modify: `service/Api/src/Module/Payment/Services/Processing/PaymentProcessingResult.cs:10`
- Modify: `service/Api/src/Module/Payment/Persistence/Configurations/Payments/PaymentRecordConfiguration.cs:24`

**Interfaces:**
- Produces: `CaptureEventCreated` removed from entity, service, DTO, and EF config.

- [ ] **Step 1: Remove from PaymentCapture entity**

Open `PaymentCapture.cs`. Delete line 22:
```csharp
// DELETE:
public bool CaptureEventCreated { get; set; }
```

- [ ] **Step 2: Remove from PaymentProcessingResult DTO**

Open `PaymentProcessingResult.cs` (`Services/Processing/`). Delete line 10:
```csharp
// DELETE:
public bool CaptureEventCreated { get; init; }
```

- [ ] **Step 3: Remove from PaymentProcessingService**

Open `PaymentProcessingService.cs`. Delete lines 213-214:
```csharp
// DELETE:
if (result.IsSuccess)
    payment.CaptureEventCreated = true;
```

- [ ] **Step 4: Remove from EF Core config**

Open `PaymentRecordConfiguration.cs`. Delete line 24:
```csharp
// DELETE:
builder.Property(x => x.CaptureEventCreated);
```

- [ ] **Step 5: Check legacy Services/Models/ copy**

If `Services/Models/PaymentProcessingResult.cs` was NOT deleted in Task 6 (directory non-empty, e.g., PaymentGatewayResponse also there), delete the `CaptureEventCreated` line from it too.

- [ ] **Step 6: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 7: Run all Payment tests**

```bash
dotnet test service/Api/tests/Module.UnitTests
```

Expected: All tests pass. If any test references `CaptureEventCreated`, remove that reference.

- [ ] **Step 8: Commit**

```bash
git add service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.cs
git add service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs
git add service/Api/src/Module/Payment/Services/Processing/PaymentProcessingResult.cs
git add service/Api/src/Module/Payment/Persistence/Configurations/Payments/PaymentRecordConfiguration.cs
git commit -m "refactor(payment): remove dead CaptureEventCreated field — never read in any handler"
```

---

### Task 13: Update Tests for All Changes

**Files:**
- Create: `service/Api/tests/Module.UnitTests/Payment/Domain/PaymentCaptures/PaymentTransitionTests.cs`
- Modify: `service/Api/tests/Module.UnitTests/Payment/Domain/PaymentCaptures/PaymentDisputeTests.cs`
- Modify: `service/Api/tests/Module.UnitTests/Payment/Features/Admin/Payments/Services/PaymentProcessingServiceTests.cs`
- Modify: `service/Api/tests/Module.UnitTests/Payment/Infrastructure/BogusGatewayTests.cs`
- Modify: `service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs`

**Interfaces:**
- Consumes: Updated domain methods (Tasks 1-2), updated service (Tasks 3-4), updated BogusGateway (Task 10), updated request (Task 11)
- Produces: Test coverage for all 13 changes.

- [ ] **Step 1: Create PaymentTransitionTests.cs for IsValidTransition**

Create `PaymentTransitionTests.cs`:

```csharp
using Module.Payment.Domain.PaymentCaptures;

namespace Module.UnitTests.Payment.Domain.PaymentCaptures;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "PaymentTransition")]
public class PaymentTransitionTests
{
    // Test the 6 newly-added transitions that were missing from IsValidTransition

    [Fact(DisplayName = "Dispute from Completed should succeed")]
    public void Dispute_FromCompleted_ShouldSucceed()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Completed;

        var result = payment.Dispute();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Disputed);
    }

    [Fact(DisplayName = "Dispute from Failed should succeed")]
    public void Dispute_FromFailed_ShouldSucceed()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Failed;

        var result = payment.Dispute();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Disputed);
    }

    [Fact(DisplayName = "Dispute from Checkout should succeed")]
    public void Dispute_FromCheckout_ShouldSucceed()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        // Already Checkout by default

        var result = payment.Dispute();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Disputed);
    }

    [Fact(DisplayName = "Fail from Checkout should succeed")]
    public void Fail_FromCheckout_ShouldSucceed()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = payment.Fail();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Failed);
    }

    [Fact(DisplayName = "Capture should set State=Completed")]
    public void Capture_ShouldSetStateToCompleted()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.Process();
        payment.Pend();

        var result = payment.Capture(50m);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Completed);
    }

    [Fact(DisplayName = "Capture with excessive amount should fail")]
    public void Capture_ExcessiveAmount_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.Process();
        payment.Pend();

        var result = payment.Capture(200m);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Payment.Amount.ExceedsAuthorized");
    }
}
```

- [ ] **Step 2: Add BogusGateway unknown test**

Open `/service/Api/tests/Module.UnitTests/Payment/Infrastructure/BogusGatewayTests.cs`. Add:

```csharp
[Fact(DisplayName = "BogusGateway: GetPaymentStatusAsync should return unknown for unrecognized code")]
public async Task GetPaymentStatusAsync_ShouldReturnUnknown_ForUnknownCode()
{
    var gateway = new BogusGateway(Options.Create(new BogusSetting { Enabled = true }));

    var result = await gateway.GetPaymentStatusAsync("nonexistent_code");

    result.Should().Be("unknown");
}
```

- [ ] **Step 3: Update PaymentProcessingServiceTests**

Open `PaymentProcessingServiceTests.cs`. Verify tests for `CaptureAsync`, `RefundAsync`, `VoidAsync` still pass with the new domain method delegation. If any tests mock behavior that the domain methods now handle, update the mocks.

- [ ] **Step 4: Update CreatePaymentIntentTests**

Open `CreatePaymentIntentTests.cs`. Add test for explicit `PaymentMethodId`:

```csharp
[Fact(DisplayName = "Handler: Should use specific PaymentMethodId when provided")]
public async Task Handle_ShouldUseSpecificPaymentMethod()
{
    // Seed a specific PaymentMethod and pass its ID in the command
    // Assert it queries by that ID, not FirstOrDefaultAsync without filter
}
```

- [ ] **Step 5: Build and run all Payment tests**

```bash
dotnet build service/Api/tests/Module.UnitTests
dotnet test service/Api/tests/Module.UnitTests
```

Expected: All tests pass. Fix any failing tests.

- [ ] **Step 6: Commit**

```bash
git add service/Api/tests/Module.UnitTests/Payment/
git commit -m "test(payment): add tests for IsValidTransition gaps, Capture() mutation, BogusGateway unknown, PaymentMethodId"
```

---

### Task 14: Final Verification — Build, Tests, Validation Checks

**Files:**
- None (verification only)

- [ ] **Step 1: Full build with warnings-as-errors**

```bash
dotnet build service/Api
```

Expected: 0 warnings, 0 errors.

- [ ] **Step 2: Run all tests**

```bash
dotnet test service/Api/tests/Module.UnitTests
```

Expected: All tests pass.

- [ ] **Step 3: Verify no direct State= mutations remain in service layer**

```bash
rg 'payment\.State = PaymentRecordState\.' service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs
```

Expected: 0 matches.

- [ ] **Step 4: Verify no direct RefundedAmount increments in service layer**

```bash
rg 'payment\.RefundedAmount \+= amount' service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs
```

Expected: 0 matches.

- [ ] **Step 5: Verify IsValidTransition has 17 arms**

```bash
rg '=> true' service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Validation.cs | wc -l
```

Expected: 16 (plus `_ => false`).

- [ ] **Step 6: Verify legacy files deleted**

```bash
ls service/Api/src/Module/Payment/Services/Abstractions/ 2>/dev/null && echo "STILL EXISTS" || echo "DELETED"
```

Expected: DELETED.

- [ ] **Step 7: Verify CaptureEventCreated removed**

```bash
rg 'CaptureEventCreated' service/Api/src/Module/Payment/
```

Expected: 0 matches.

- [ ] **Step 8: Verify CentsMultiplier consolidated**

```bash
rg 'const.*CentsMultiplier' service/Api/src/Module/Payment/Services/Provider/
```

Expected: 0 matches.

- [ ] **Step 9: Verify BogusGateway returns unknown**

```bash
rg 'Task\.FromResult\("succeeded"\)' service/Api/src/Module/Payment/Services/Provider/Bogus/BogusGateway.cs
```

Expected: 0 matches (for the default return — the status check lookup still returns "succeeded" for known codes).

- [ ] **Step 10: Commit if any fixes**

```bash
git add -A service/
git commit -m "chore(payment): final verification — all 13 caveman review findings addressed"
```

---

## Self-Review Checklist

### Spec Coverage

| Requirement | Task(s) | Covered |
|---|---|---|
| STM-001 (IsValidTransition +6) | Task 1 | Yes |
| STM-002 (Capture() mutates state) | Task 2 | Yes |
| STM-003 (CaptureAsync uses Complete()) | Task 3 | Yes |
| STM-004 (GatewayActionAsync/VoidTx uses domain methods) | Task 3 | Yes |
| STM-005 (IsValidTransition canonical) | Task 1 | Yes |
| DED-001 (RefundAsync uses Refund()) | Task 4 | Yes |
| DED-002 (consolidate error codes) | Task 5 | Yes |
| DED-003 (delete legacy Abstractions) | Task 6 | Yes |
| DED-004 (CentsMultiplier consolidation) | Task 7 | Yes |
| DED-005 (remove CaptureEventCreated) | Task 12 | Yes |
| EDG-001 (amount overflow guard) | Task 8 | Yes |
| EDG-002 (remove redundant rollback) | Task 9 | Yes |
| EDG-003 (BogusGateway unknown) | Task 10 | Yes |
| EDG-004 (PaymentMethodId in request) | Task 11 | Yes |

### Placeholder Scan
- No TODOs, TBDs, or placeholders.
- All code steps contain concrete code.
- Task 6 has a conditional `rm -f` for a file that may not exist — acceptable.
- Task 13 Step 4 notes that CreatePaymentIntentTests needs a full test body — the pattern is clear enough.

### Type Consistency
- `GatewayConstants.Amounts.CentsMultiplier` defined in Task 7, consumed in Tasks 7, 8.
- `GatewayConstants.Amounts.MaxSafeDollarAmount` defined in Task 7, consumed in Task 8.
- `StripeGatewayResult.Errors.AmountExceedsMaximum` defined in Task 8, consumed in Task 8.
- `payment.Capture(amount)` modified in Task 2, consumed in Task 3.
- `payment.Refund(amount)` consumed in Task 4 (already exists).
- `command.PaymentMethodId` defined in Task 11, consumed in Task 11.
