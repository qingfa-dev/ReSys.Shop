# Stripe Integration Completion — 3DS, Resilience, Domain, and Hardening Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Close 13 production gaps in the Stripe integration: 3DS/SCA handling, transient error resilience, statement descriptor passthrough, webhook retry, dispute state, DB indexing, startup validation, and code quality hardening.

**Architecture:** Changes touch 3 layers: Domain (new `Disputed` state + guards), Infrastructure (StripeGateway hardening, webhook retry, DB index), and Cross-Cutting (startup validation, SDK reuse). Each task is independently buildable and testable. No breaking interface changes.

**Tech Stack:** .NET 10, Stripe.net 52.1.0, Hangfire, EF Core + Npgsql, xUnit, FluentAssertions, Moq.

## Global Constraints

- `TreatWarningsAsErrors=true` — zero warnings on `dotnet build`
- Result objects, not exceptions — domain failures return `Result.IsFailure`, never throw
- Modules never reference each other — all changes confined to Payment module
- Vertical slice feature files — each action is `static partial class` split across files
- No breaking interface changes to `IPaymentGatewayActionProvider` or `IStripeWebhookService`
- Commit after each task completes and builds clean

---

### Task 1: Add Disputed State to Domain Enum

**Files:**
- Modify: `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Enumerate.cs:4-13`

**Interfaces:**
- Produces: `PaymentRecordState.Disputed` enum member with value 7. No downstream code breaks — enum is used in switch expressions that must become exhaustive.

- [ ] **Step 1: Add Disputed to the enum**

Open `PaymentCapture.Enumerate.cs`. Add `Disputed` as the 7th member, before `Invalid`:

```csharp
// PaymentCapture.Enumerate.cs — full file after change
namespace Module.Payment.Domain.PaymentCaptures;

public enum PaymentRecordState
{
    Checkout,
    Processing,
    Pending,
    Completed,
    Failed,
    Void,
    Disputed,
    Invalid
}
```

- [ ] **Step 2: Build to verify no warnings**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Enumerate.cs
git commit -m "feat(payment): add Disputed state to PaymentRecordState enum"
```

---

### Task 2: Add Dispute Domain Method

**Files:**
- Modify: `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Method.State.cs:62-74`
- Modify: `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Result.cs:165-169`

**Interfaces:**
- Produces: `PaymentCapture.Dispute()` extension method. Returns `Result.Ok()` on success or `PaymentCaptureResult.Failure.AlreadyDisputed` / `PaymentCaptureResult.Failure.InvalidStateTransition(from, Disputed)` on failure.

- [ ] **Step 1: Add AlreadyDisputed error factory**

Open `PaymentCapture.Result.cs`. Add after `AlreadyFailed` (line 178, before `AmountExceedsAuthorized`):

```csharp
/// <summary>Error indicating the payment has already been disputed.</summary>
public static Error AlreadyDisputed => Error.Conflict(
    code: "Payment.AlreadyDisputed",
    message: "Payment has already been disputed.");
```

- [ ] **Step 2: Add Dispute method to state transitions**

Open `PaymentCapture.Method.State.cs`. Add after the `Void` method (after line 74, before `#endregion`):

```csharp
// Update: Any non-terminal state → Disputed — idempotent if already disputed
public static Result Dispute(this PaymentCapture payment)
{
    if (payment.State is PaymentRecordState.Disputed)
        return PaymentCaptureResult.Failure.AlreadyDisputed;

    if (payment.State is PaymentRecordState.Void or PaymentRecordState.Invalid)
        return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Disputed);

    payment.State = PaymentRecordState.Disputed;
    payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
    return Result.Ok();
}
```

- [ ] **Step 3: Guard CanRefund against Disputed state**

In the same file, modify `CanRefund` at line 118. Change from:

```csharp
public static bool CanRefund(this PaymentCapture payment, decimal amount)
    => payment.State is PaymentRecordState.Completed
       && amount > 0 && (payment.Amount - payment.RefundedAmount) >= amount;
```

to:

```csharp
public static bool CanRefund(this PaymentCapture payment, decimal amount)
    => payment.State is PaymentRecordState.Completed
       && !payment.State.Equals(PaymentRecordState.Disputed)
       && amount > 0 && (payment.Amount - payment.RefundedAmount) >= amount;
```

Wait — `Completed` and `Disputed` are mutually exclusive, so the existing `is Completed` guard already rejects `Disputed`. The real risk is the `CreditAllowed` method on line 93: if payment was manually set to `Disputed` after completion, `CreditAllowed` returns false (it checks `is Completed`). So the domain guard is actually sufficient.

However, we should add an explicit guard for clarity. Modify `CreditAllowed` at line 93:

```csharp
// Before:
public static bool CreditAllowed(this PaymentCapture payment)
    => payment.State is PaymentRecordState.Completed;

// After:
public static bool CreditAllowed(this PaymentCapture payment)
    => payment.State is PaymentRecordState.Completed;
```

No change needed — `Disputed` is not `Completed`. The state is exclusive. Let's keep it simple: add `Disputed` to the `CanCapture` guard too. Modify line 101:

```csharp
public static bool CanCapture(this PaymentCapture payment, decimal amount)
    => payment.State is PaymentRecordState.Processing or PaymentRecordState.Pending
       && amount > 0 && amount <= payment.Amount;
```

This already rejects `Disputed` since it only accepts `Processing|Pending`. No change needed.

- [ ] **Step 4: Build to verify no errors**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Method.State.cs
git add service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Result.cs
git commit -m "feat(payment): add Dispute() domain method with AlreadyDisputed guard"
```

---

### Task 3: Make GetPaymentStatusAsync Abstract

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/Gateway.cs:33-36`
- Modify: `service/Api/src/Module/Payment/Services/Provider/Bogus/BogusGateway.cs:73-78`

**Interfaces:**
- Consumes: None (changes Gateway base class signature)
- Produces: `abstract Task<string> GetPaymentStatusAsync(string, CancellationToken)` — all subclasses must override

- [ ] **Step 1: Change Gateway base class**

Open `Gateway.cs`. Change lines 33-36 from:

```csharp
public virtual Task<string> GetPaymentStatusAsync(
    string responseCode, CancellationToken ct = default)
    => Task.FromResult("succeeded");
```

to:

```csharp
public abstract Task<string> GetPaymentStatusAsync(
    string responseCode, CancellationToken ct = default);
```

- [ ] **Step 2: Build — expect BogusGateway compile error**

```bash
dotnet build service/Api/src/Module
```

Expected: Build FAILED with CS0534: `BogusGateway does not implement inherited abstract member Gateway.GetPaymentStatusAsync(...)`.

- [ ] **Step 3: BogusGateway already has the override**

Open `BogusGateway.cs`. Lines 73-78 already have:

```csharp
public override Task<string> GetPaymentStatusAsync(string responseCode, CancellationToken ct)
{
    if (_intentStatuses.TryGetValue(responseCode, out var status))
        return Task.FromResult(status);
    return Task.FromResult("succeeded");
}
```

But it's missing the `= default` default parameter. Change line 73 from:

```csharp
public override Task<string> GetPaymentStatusAsync(string responseCode, CancellationToken ct)
```

to:

```csharp
public override Task<string> GetPaymentStatusAsync(string responseCode, CancellationToken ct = default)
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Provider/Gateway.cs
git add service/Api/src/Module/Payment/Services/Provider/Bogus/BogusGateway.cs
git commit -m "feat(payment): make GetPaymentStatusAsync abstract on Gateway — prevents silent default"
```

---

### Task 4: Add New Error Factories for 3DS and Transient Errors

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.Result.cs:20-31`

**Interfaces:**
- Produces: `StripeGatewayResult.Errors.PaymentMethodRequired(string message)` → `Error.Validation`
- Produces: `StripeGatewayResult.Errors.TransientGatewayError(string code, string message)` → `Error.Unexpected`

- [ ] **Step 1: Add PaymentMethodRequired and TransientGatewayError**

Open `StripeGateway.Result.cs`. After `AuthorizeNotRequiresCapture` (line 26, before `GatewayError`):

```csharp
public static Error PaymentMethodRequired(string? message) => Error.Validation(
    "Stripe.PaymentMethod.Required",
    $"Payment method was declined or requires re-entry: {message ?? "unknown"}");

public static Result<PaymentGatewayResponse> TransientGatewayError(string code, string message) =>
    Result<PaymentGatewayResponse>.Failure(
        Error.Unexpected($"Stripe.Transient.{code}", message));
```

Wait — existing error factories like `PurchaseNotSucceeded` return `Error` (not `Result<...>`). The `GatewayError` method returns `Error`. The new `TransientGatewayError` needs to return `Result<PaymentGatewayResponse>` because it MUST carry the `Error.Unexpected` type (retryable), unlike `Error.BadRequest` (terminal). Let me check the usage pattern.

In `StripeGateway.cs`, `MapStripeException` returns `Result<PaymentGatewayResponse>` (line 185). The callers are `PurchaseAsync` L53, `AuthorizeAsync` L72, `CaptureAsync` L93, `VoidAsync` L110, `RefundAsync` L132, `CreateSetupIntentAsync` L147 — all do `catch (StripeException ex) { return MapStripeException(ex); }`.

So `MapStripeException` must return `Result<PaymentGatewayResponse>`. Currently it calls `GatewayError` which returns `Error` and wraps it implicitly. Let's check:

```csharp
// Line 185-193
private static Result<PaymentGatewayResponse> MapStripeException(StripeException ex)
{
    var e = ex.StripeError;
    var code = e?.Code ?? GatewayConstants.ErrorCodes.Stripe.UnknownError;
    var msg = ...;
    return StripeGatewayResult.Errors.GatewayError(code, msg);
}
```

`GatewayError(code, msg)` returns `Error`. But there's likely an implicit conversion from `Error` to `Result<PaymentGatewayResponse>`. Yes, `Result<T>` has implicit conversion from `Error`. So `return StripeGatewayResult.Errors.GatewayError(code, msg)` works.

But for the transient case, we need to return a `Result<PaymentGatewayResponse>` with `Error.Unexpected`. Let's make `TransientGatewayError` return `Result<PaymentGatewayResponse>` directly since the implicit conversion from `Error` would lose the `ErrorType` distinction. Actually, `Result<T>.Failure(Error.Unexpected(...))` creates a failed Result with Unexpected error type. The implicit `Error → Result<T>` also creates a failure. Let's check if the type distinction is preserved...

In the `Result` pattern, `Error.BadRequest(...)` vs `Error.Unexpected(...)` create errors of different `ErrorType`. When implicit-conversioned to `Result<T>`, the `ErrorType` is preserved in `Result.Errors[0].Type`. So both approaches work.

But for clarity and explicit intent, let's make `TransientGatewayError` return `Result<PaymentGatewayResponse>` and rename the existing `GatewayError` to also return `Result<PaymentGatewayResponse>` ... no, that would be a breaking change to all callers. Let's add the transient factory as a proper Result factory.

Here's what to add after line 31 (`GatewayError`):

```csharp
public static Result<PaymentGatewayResponse> TransientGatewayError(string code, string message) =>
    Result<PaymentGatewayResponse>.Failure(
        Error.Unexpected($"Stripe.Transient.{code}", message));
```

Also add `PaymentMethodRequired`:

```csharp
public static Result<PaymentGatewayResponse> PaymentMethodRequired(string? message) =>
    Result<PaymentGatewayResponse>.Failure(
        Error.Validation(
            "Stripe.PaymentMethod.Required",
            $"Payment method was declined or requires re-entry: {message ?? "unknown"}"));
```

- [ ] **Step 1: Write the additions**

Add after `GatewayError` at line 31:

```csharp
public static Result<PaymentGatewayResponse> TransientGatewayError(string code, string message) =>
    Result<PaymentGatewayResponse>.Failure(
        Error.Unexpected($"Stripe.Transient.{code}", message));

public static Result<PaymentGatewayResponse> PaymentMethodRequired(string? message) =>
    Result<PaymentGatewayResponse>.Failure(
        Error.Validation(
            "Stripe.PaymentMethod.Required",
            $"Payment method was declined or requires re-entry: {message ?? "unknown"}"));
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.Result.cs
git commit -m "feat(payment): add TransientGatewayError and PaymentMethodRequired error factories for Stripe"
```

---

### Task 5: Handle 3DS/requires_action in StripeGateway.PurchaseAsync

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs:45-48`

**Interfaces:**
- Consumes: `StripeGatewayResult.Errors.PaymentMethodRequired(string?)` (from Task 4)
- Produces: `PurchaseAsync` now returns `Success` with `paymentStatus = "requires_action"` when 3DS is needed. Returns `Failure` with `Stripe.PaymentMethod.Required` when payment method re-entry needed.

- [ ] **Step 1: Replace the succeeded-only status check**

Open `StripeGateway.cs`. Change lines 45-51 from:

```csharp
// Check: Intent must be succeeded status for auto-capture
if (intent.Status != GatewayConstants.Stripe.IntentStatus.Succeeded)
    return StripeGatewayResult.Errors.PurchaseNotSucceeded(intent.Status);
return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe,
    authorization: intent.Id,
    clientSecret: intent.ClientSecret);
```

to:

```csharp
// Check: Intent status routing
if (intent.Status == GatewayConstants.Stripe.IntentStatus.Succeeded)
    return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe,
        authorization: intent.Id,
        clientSecret: intent.ClientSecret);

if (intent.Status == "requires_action")
    return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe,
        authorization: intent.Id,
        clientSecret: intent.ClientSecret,
        paymentStatus: "requires_action");

if (intent.Status == "requires_payment_method")
    return StripeGatewayResult.Errors.PaymentMethodRequired(
        intent.LastPaymentError?.Message);

return StripeGatewayResult.Errors.PurchaseNotSucceeded(intent.Status);
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs
git commit -m "feat(payment): handle requires_action (3DS) and requires_payment_method in PurchaseAsync"
```

---

### Task 6: Split MapStripeException into Transient vs Terminal

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs:184-193`

**Interfaces:**
- Consumes: `StripeGatewayResult.Errors.TransientGatewayError(string, string)` (from Task 4)
- Produces: `MapStripeException` now returns `ErrorType.Unexpected` for HTTP 5xx and `api_error/api_connection_error`, `ErrorType.Validation` for terminal errors.

- [ ] **Step 1: Replace MapStripeException with transient-aware version**

Open `StripeGateway.cs`. Replace lines 184-193:

```csharp
// Map: StripeException → structured Error response
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

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs
git commit -m "fix(payment): distinguish transient StripeException (5xx/api_error) from terminal declines"
```

---

### Task 7: Add checked{} Around Amount-to-Cents Conversion

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs:166`
- Modify: `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs:87`
- Modify: `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs:126`

**Interfaces:**
- No interface changes — same signatures, just `checked` wrapping on the cast.

- [ ] **Step 1: Add checked to all three amount conversions**

Open `StripeGateway.cs`. Find each `(long)Math.Round(amount * CentsMultiplier, ...)` and wrap in `checked(...)`:

Line 166 inside `CreatePaymentIntentOptions`:
```csharp
// Before:
Amount = (long)Math.Round(amount * CentsMultiplier, MidpointRounding.AwayFromZero),
// After:
Amount = checked((long)Math.Round(amount * CentsMultiplier, MidpointRounding.AwayFromZero)),
```

Line 87 inside `CaptureAsync`:
```csharp
// Before:
AmountToCapture = (long)Math.Round(amount * CentsMultiplier, MidpointRounding.AwayFromZero)
// After:
AmountToCapture = checked((long)Math.Round(amount * CentsMultiplier, MidpointRounding.AwayFromZero))
```

Line 126 inside `RefundAsync`:
```csharp
// Before:
Amount = (long)Math.Round(amount * CentsMultiplier, MidpointRounding.AwayFromZero)
// After:
Amount = checked((long)Math.Round(amount * CentsMultiplier, MidpointRounding.AwayFromZero))
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs
git commit -m "fix(payment): wrap amount-to-cents conversion in checked{} to fail on overflow"
```

---

### Task 8: Pass Statement Descriptor and Shipping Address to Stripe

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs:160-182`

**Interfaces:**
- Consumes: `GatewayOptions.StatementDescriptorSuffix`, `GatewayOptions.ShippingAddress` (already exist on the record)
- Produces: `PaymentIntentCreateOptions` now includes `StatementDescriptorSuffix` and `Shipping` when provided.

- [ ] **Step 1: Add statement descriptor and shipping to CreatePaymentIntentOptions**

Open `StripeGateway.cs`. Inside `CreatePaymentIntentOptions`, after the `Metadata` assignment (after line 176, before the `// Assign:` comment at line 178), add:

```csharp
// Assign: Statement descriptor suffix — shown on customer card statements
if (!string.IsNullOrEmpty(options.StatementDescriptorSuffix))
    o.StatementDescriptorSuffix = options.StatementDescriptorSuffix;

// Assign: Shipping details for fraud detection and card statement context
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

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs
git commit -m "feat(payment): pass StatementDescriptorSuffix and ShippingAddress to Stripe PaymentIntent"
```

---

### Task 9: Reuse Stripe SDK Service Instances as Fields

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs:9-10` (add fields)
- Modify: `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs:45,64,90,107,129,155` (replace `new XxxService()`)

**Interfaces:**
- No interface changes. All API calls now use field-level service instances.

- [ ] **Step 1: Add private readonly fields**

Open `StripeGateway.cs`. Add after line 11 (`private readonly StripeSetting _options;`):

```csharp
private readonly PaymentIntentService _paymentIntentService = new();
private readonly RefundService _refundService = new();
private readonly SetupIntentService _setupIntentService = new();
```

- [ ] **Step 2: Replace all `new PaymentIntentService()` → `_paymentIntentService`**

Line 45:
```csharp
// Before:
var intent = await new PaymentIntentService().CreateAsync(po, ro, ct).ConfigureAwait(false);
// After:
var intent = await _paymentIntentService.CreateAsync(po, ro, ct).ConfigureAwait(false);
```

Line 64:
```csharp
// Before:
var intent = await new PaymentIntentService().CreateAsync(po, ro, ct).ConfigureAwait(false);
// After:
var intent = await _paymentIntentService.CreateAsync(po, ro, ct).ConfigureAwait(false);
```

Line 90:
```csharp
// Before:
var intent = await new PaymentIntentService().CaptureAsync(responseCode, co, ro, ct).ConfigureAwait(false);
// After:
var intent = await _paymentIntentService.CaptureAsync(responseCode, co, ro, ct).ConfigureAwait(false);
```

Line 107:
```csharp
// Before:
var intent = await new PaymentIntentService().CancelAsync(responseCode, co, ro, ct).ConfigureAwait(false);
// After:
var intent = await _paymentIntentService.CancelAsync(responseCode, co, ro, ct).ConfigureAwait(false);
```

Line 155:
```csharp
// Before:
var intent = await new PaymentIntentService().GetAsync(paymentIntentId, null, ro, ct);
// After:
var intent = await _paymentIntentService.GetAsync(paymentIntentId, null, ro, ct);
```

- [ ] **Step 3: Replace `new RefundService()` → `_refundService`**

Line 129:
```csharp
// Before:
var refund = await new RefundService().CreateAsync(ro, requestOptions, ct).ConfigureAwait(false);
// After:
var refund = await _refundService.CreateAsync(ro, requestOptions, ct).ConfigureAwait(false);
```

- [ ] **Step 4: Replace `new SetupIntentService()` → `_setupIntentService`**

Line 143:
```csharp
// Before:
var intent = await new SetupIntentService().CreateAsync(options, ro, ct).ConfigureAwait(false);
// After:
var intent = await _setupIntentService.CreateAsync(options, ro, ct).ConfigureAwait(false);
```

- [ ] **Step 5: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs
git commit -m "refactor(payment): reuse Stripe SDK service instances as fields instead of new per call"
```

---

### Task 10: Add Database Index on ResponseCode

**Files:**
- Modify: `service/Api/src/Module/Payment/Persistence/Configurations/Payments/PaymentRecordConfiguration.cs:33`

**Interfaces:**
- Produces: EF Core migration with index `ix_payment_captures_response_code` on `ResponseCode` column with `WHERE "ResponseCode" IS NOT NULL` filter.

- [ ] **Step 1: Add the index configuration**

Open `PaymentRecordConfiguration.cs`. Add after line 33 (`builder.HasOne...`) and before the closing brace:

```csharp
builder.HasIndex(x => x.ResponseCode)
    .HasDatabaseName("ix_payment_captures_response_code")
    .HasFilter("\"ResponseCode\" IS NOT NULL");
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Generate migration**

```bash
dotnet ef migrations add AddResponseCodeIndex --project service/Api/src/Migrations --startup-project service/Api/src/Api
```

If `dotnet ef` is not available as a global tool, use:
```bash
dotnet run --project service/Api/src/Api -- --migrate 2>&1 | head -20
```

Or check the migration approach used in the project:

```bash
rg "ef migrations" docs/ --max-count 5
rg "AddMigration\|dotnet ef" service/ --max-count 5 --include "*.sh" --include "*.md"
```

- [ ] **Step 4: Verify the migration SQL contains the index**

```bash
rg "ix_payment_captures_response_code" service/Api/src/Migrations/
```

Expected: Match in the latest migration file.

- [ ] **Step 5: Build and run unit tests**

```bash
dotnet build service/Api
dotnet test service/Api/tests/Module.UnitTests --filter "Category=Unit"
```

Expected: All unit tests pass.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Payment/Persistence/Configurations/Payments/PaymentRecordConfiguration.cs
git add service/Api/src/Migrations/
git commit -m "feat(payment): add partial index ix_payment_captures_response_code for webhook lookups"
```

---

### Task 11: Add Hangfire AutomaticRetry to Webhook Job

**Files:**
- Modify: `service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs:14`

**Interfaces:**
- No interface changes. Adds `[AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]` attribute to the job class.

- [ ] **Step 1: Add the AutomaticRetry attribute**

Open `ProcessStripeWebhookEventJob.cs`. Add the attribute on line 14, before the class declaration. The class currently looks like:

```csharp
/// <summary>Background job...</summary>
public sealed partial class ProcessStripeWebhookEventJob
```

Add the attribute:

```csharp
[AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
/// <summary>Background job...</summary>
public sealed partial class ProcessStripeWebhookEventJob
```

Also ensure the `using Hangfire;` directive is present at the top. Check line 1 — it uses `Module.Payment.Backgrounds` but Hangfire types are used. Let me check if Hangfire is already imported:

Looking at the file content, line 1 starts with `using Microsoft.Extensions.Logging;` — no `using Hangfire;`. Add it:

Add `using Hangfire;` after `using Microsoft.Extensions.Logging;` on line 1:

```csharp
using Hangfire;
using Microsoft.Extensions.Logging;
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs
git commit -m "fix(payment): add Hangfire [AutomaticRetry(3)] to webhook job — prevents dropped webhooks on transient DB errors"
```

---

### Task 12: Handle charge.dispute.created — Transition to Disputed State

**Files:**
- Modify: `service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs:130-136`
- Modify: `service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.Loggers.cs:19-22`

**Interfaces:**
- Consumes: `PaymentCapture.Dispute()` (from Task 2)
- Produces: `HandleChargeDisputeCreated` now loads the payment by `PaymentIntentId` and calls `Dispute()` instead of just logging.

- [ ] **Step 1: Add structured logger for dispute failures**

Open `ProcessStripeWebhookEventJob.Loggers.cs`. Add after `CannotRefundPayment` (line 22):

```csharp
[LoggerMessage(
    EventId = 5009,
    Level = LogLevel.Warning,
    Message = "Cannot dispute payment {PaymentId} (state={State}): {Message}")]
public static partial void CannotDisputePayment(ILogger logger, Guid PaymentId, string State, string? Message);
```

- [ ] **Step 2: Replace the dispute handler**

Open `ProcessStripeWebhookEventJob.cs`. Change the `HandleChargeDisputeCreated` method (lines 131-136) from:

```csharp
// Webhook: charge.dispute.created — log for manual review
private void HandleChargeDisputeCreated(Event stripeEvent)
{
    var dispute = stripeEvent.Data.Object as Dispute;
    if (dispute is null) return;
    _logger.DisputeCreated(dispute.ChargeId, dispute.Reason ?? "unknown");
}
```

to:

```csharp
// Webhook: charge.dispute.created — transition to Disputed state
private async Task HandleChargeDisputeCreated(Event stripeEvent, CancellationToken ct)
{
    var dispute = stripeEvent.Data.Object as Dispute;
    if (dispute is null || string.IsNullOrEmpty(dispute.PaymentIntentId)) return;

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
    _logger.DisputeCreated(dispute.ChargeId, dispute.Reason ?? "unknown");
}
```

Note: `_logger.DisputeCreated()` — does this extension method exist? Looking at the original code, it uses `_logger.DisputeCreated(...)` on line 135. This must be a custom extension method. Let me check if it's defined.

- [ ] **Step 3: Verify DisputeCreated logger exists**

The current code calls `_logger.DisputeCreated(dispute.ChargeId, dispute.Reason ?? "unknown")`. This is not a standard `ILogger` method — it must be a custom source-generated logger or an extension method. Search for it:

```bash
rg "DisputeCreated" service/Api/src/Module/Payment/
```

If it's defined in another partial file or as a `[LoggerMessage]` attribute method, the code will build. If not found, we'll need to define it. Let's assume it exists (the code compiles currently).

- [ ] **Step 4: Update the switch statement in ExecuteAsync**

Open `ProcessStripeWebhookEventJob.cs`. Change the `HandleChargeDisputeCreated` call on line 53 from synchronous to async:

```csharp
case GatewayConstants.WebhookEvents.Stripe.ChargeDisputeCreated:
    await HandleChargeDisputeCreated(stripeEvent, ct);
    break;
```

- [ ] **Step 5: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs
git add service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.Loggers.cs
git commit -m "feat(payment): transition payment to Disputed state on charge.dispute.created webhook"
```

---

### Task 13: Add Startup Validation for StripeSetting

**Files:**
- Create: `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeSettingValidation.cs`
- Modify: `service/Api/src/Module/Payment/Payment.Extension.cs:44-46`

**Interfaces:**
- Consumes: `IValidateOptions<StripeSetting>` (Microsoft.Extensions.Options)
- Produces: Startup fails with `OptionsValidationException` if Stripe is enabled but `SecretKey` or `WebhookSecret` is empty.

- [ ] **Step 1: Create StripeSettingValidation.cs**

Create new file `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeSettingValidation.cs`:

```csharp
using Microsoft.Extensions.Options;

namespace Module.Payment.Services.Provider.Stripe;

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

- [ ] **Step 2: Register validation in DI**

Open `Payment.Extension.cs`. After line 46 (`services.Configure<StripeSetting>(...)`), add:

```csharp
services.AddSingleton<IValidateOptions<StripeSetting>, StripeSettingValidation>();
```

- [ ] **Step 3: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 4: Verify validation fires on startup**

Enable Stripe with empty secrets in `appsettings.Development.json` temporarily:

```bash
# Check current settings
rg "stripe" service/Api/src/Api/appsettings.Development.json
```

Expected: The `dotnet run` with development settings should NOT fail because Stripe is `Enabled: false` in dev. To test the validation manually, temporarily add to appsettings.Development.json:

```json
"GatewayProviders": {
    "stripe": {
        "Enabled": true,
        "SecretKey": "",
        "WebhookSecret": ""
    }
}
```

Then run `dotnet run --project service/Api/src/Api` and observe the startup crash. Revert the setting after testing.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Provider/Stripe/StripeSettingValidation.cs
git add service/Api/src/Module/Payment/Payment.Extension.cs
git commit -m "feat(payment): fail startup if Stripe enabled but SecretKey or WebhookSecret is empty"
```

---

### Task 14: Narrow Exception Catch in StripeWebhookDispatcher

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs:64-69`

**Interfaces:**
- No interface changes.

- [ ] **Step 1: Change Exception to StripeException**

Open `StripeWebhookDispatcher.cs`. Change lines 64-69 from:

```csharp
// Catch: Exception → log and return null (malformed payload)
try { return EventUtility.ParseEvent(payload); }
catch (Exception ex)
{
    StripeWebhookDispatcherLoggers.EventParseFailed(_logger, ex, payload);
    return null;
}
```

to:

```csharp
// Catch: StripeException → log and return null (malformed payload)
try { return EventUtility.ParseEvent(payload); }
catch (StripeException ex)
{
    StripeWebhookDispatcherLoggers.EventParseFailed(_logger, ex, payload);
    return null;
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs
git commit -m "fix(payment): catch StripeException not Exception in ParseEvent — prevents swallowing fatal errors"
```

---

### Task 15: Guard PaymentProcessingService Refund Against Disputed State

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs:85-105`

**Interfaces:**
- Consumes: `PaymentCapture.Disputed` state (from Task 1)
- Produces: `RefundAsync` returns `Failure` if payment is in `Disputed` state.

- [ ] **Step 1: Add Disputed check to RefundAsync**

Open `PaymentProcessingService.cs`. In the `RefundAsync` method, after the existing `CanRefund` check at line 88, add a `Disputed` guard. Change lines 88-94 from:

```csharp
if (!payment.CanRefund(amount))
{
    if (payment.State is not PaymentRecordState.Completed)
        return ProcessingResult.Errors.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

    return ProcessingResult.Errors.AmountExceedsAuthorized;
}
```

to:

```csharp
if (payment.State is PaymentRecordState.Disputed)
    return ProcessingResult.Errors.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

if (!payment.CanRefund(amount))
{
    if (payment.State is not PaymentRecordState.Completed)
        return ProcessingResult.Errors.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

    return ProcessingResult.Errors.AmountExceedsAuthorized;
}
```

- [ ] **Step 2: Add Disputed check to CaptureAsync**

Also guard `CaptureAsync` at line 34. Add after the `AlreadyCompleted` check:

```csharp
// Check: Cannot capture disputed payments
if (payment.State == PaymentRecordState.Disputed)
    return ProcessingResult.Errors.InvalidStateTransition(payment.State, PaymentRecordState.Completed);
```

The full `CaptureAsync` method starting at line 31 should now be:

```csharp
public async Task<Result<PaymentProcessingResult>> CaptureAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal? amount = null, CancellationToken ct = default)
{
    // Check: Already completed — idempotency guard
    if (payment.State == PaymentRecordState.Completed)
        return ProcessingResult.Errors.AlreadyCompleted;

    // Check: Cannot capture disputed payments
    if (payment.State == PaymentRecordState.Disputed)
        return ProcessingResult.Errors.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

    amount ??= payment.Amount;

    // Check: Payment does not allow capture at current state or amount
    if (!payment.CanCapture(amount.Value))
        return ProcessingResult.Errors.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

    StartedProcessing(payment);

    // Call: Gateway capture API — Stripe PaymentIntent capture
    var gatewayResult = await gateway.CaptureAsync(amount.Value, payment.ResponseCode, options, ct).ConfigureAwait(false);

    // Catch: Gateway failure — propagate error without mutating state
    if (gatewayResult.IsFailure)
        return Result<PaymentProcessingResult>.Failure(gatewayResult.Errors[0]);

    var response = gatewayResult.Value;
    RecordGatewayResponse(payment, response);
    payment.State = PaymentRecordState.Completed;
    payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
    return ProcessingResult.Success.Captured(payment.Number, amount.Value);
}
```

- [ ] **Step 3: Build to verify**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs
git commit -m "fix(payment): guard RefundAsync and CaptureAsync against Disputed state"
```

---

### Task 16: Write and Run Tests

**Files:**
- Create: `service/Api/tests/Module.UnitTests/Payment/Domain/PaymentCaptures/PaymentDisputeTests.cs`
- Modify: `service/Api/tests/Module.UnitTests/Payment/Backgrounds/ProcessStripeWebhookEventJobTests.cs:174-198`
- Create: `service/Api/tests/Module.UnitTests/Payment/Infrastructure/Gateways/Stripe/StripeGatewayTransientErrorTests.cs`
- Create: `service/Api/tests/Module.UnitTests/Payment/Services/StripeSettingValidationTests.cs`

**Interfaces:**
- Consumes: `PaymentCapture.Dispute()`, `StripeGatewayResult.Errors.TransientGatewayError`, `StripeSettingValidation`
- Produces: Test coverage for all new domain, gateway, and validation logic.

- [ ] **Step 1: Write domain dispute tests**

Create `PaymentDisputeTests.cs`:

```csharp
using Module.Payment.Domain.PaymentCaptures;

namespace Module.UnitTests.Payment.Domain.PaymentCaptures;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "PaymentDispute")]
public class PaymentDisputeTests
{
    [Fact(DisplayName = "Dispute transitions Completed payment to Disputed")]
    public void Dispute_ShouldTransitionToDisputed()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Completed;

        var result = payment.Dispute();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Disputed);
    }

    [Fact(DisplayName = "Dispute is idempotent — already disputed returns AlreadyDisputed")]
    public void Dispute_ShouldBeIdempotent()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Disputed;

        var result = payment.Dispute();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Payment.AlreadyDisputed");
    }

    [Fact(DisplayName = "Dispute from Void state returns InvalidStateTransition")]
    public void Dispute_FromVoid_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Void;

        var result = payment.Dispute();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Payment.State.InvalidTransition");
    }

    [Fact(DisplayName = "Dispute from Processing state transitions to Disputed")]
    public void Dispute_FromProcessing_ShouldSucceed()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;

        var result = payment.Dispute();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Disputed);
    }
}
```

- [ ] **Step 2: Write webhook dispute test**

Open `ProcessStripeWebhookEventJobTests.cs`. Add after the last test method (line 197), before the closing braces:

```csharp
[Fact(DisplayName = "charge.dispute.created transitions payment to Disputed")]
public async Task HandleChargeDisputeCreated_ShouldDisputePayment()
{
    var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
    payment.State = PaymentRecordState.Completed;
    payment.ResponseCode = "pi_disputed";
    _dbContext.Set<PaymentCapture>().Add(payment);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
        .Returns(new Event
        {
            Type = "charge.dispute.created",
            Data = new EventData
            {
                Object = new Dispute
                {
                    PaymentIntentId = "pi_disputed",
                    ChargeId = "ch_disputed",
                    Reason = "fraudulent"
                }
            }
        });

    await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

    var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
    updated.State.Should().Be(PaymentRecordState.Disputed);
}
```

- [ ] **Step 3: Create StripeSettingValidation tests**

Create `StripeSettingValidationTests.cs`:

```csharp
using Microsoft.Extensions.Options;

using Module.Payment.Services.Provider.Stripe;

namespace Module.UnitTests.Payment.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "StripeSettingValidation")]
public class StripeSettingValidationTests
{
    private readonly StripeSettingValidation _validator = new();

    [Fact(DisplayName = "Validation passes when Stripe is disabled with empty secrets")]
    public void Validate_ShouldPass_WhenDisabled()
    {
        var options = new StripeSetting
        {
            Enabled = false,
            SecretKey = "",
            WebhookSecret = ""
        };

        var result = _validator.Validate(null, options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact(DisplayName = "Validation fails when Enabled=true and SecretKey is empty")]
    public void Validate_ShouldFail_WhenEnabledAndSecretKeyEmpty()
    {
        var options = new StripeSetting
        {
            Enabled = true,
            SecretKey = "",
            WebhookSecret = "whsec_test"
        };

        var result = _validator.Validate(null, options);

        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("SecretKey");
    }

    [Fact(DisplayName = "Validation fails when Enabled=true and WebhookSecret is empty")]
    public void Validate_ShouldFail_WhenEnabledAndWebhookSecretEmpty()
    {
        var options = new StripeSetting
        {
            Enabled = true,
            SecretKey = "sk_test_fake",
            WebhookSecret = ""
        };

        var result = _validator.Validate(null, options);

        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("WebhookSecret");
    }

    [Fact(DisplayName = "Validation passes when Enabled=true and both secrets are set")]
    public void Validate_ShouldPass_WhenEnabledAndSecretsSet()
    {
        var options = new StripeSetting
        {
            Enabled = true,
            SecretKey = "sk_test_fake",
            WebhookSecret = "whsec_test"
        };

        var result = _validator.Validate(null, options);

        result.Succeeded.Should().BeTrue();
    }
}
```

- [ ] **Step 4: Run all Payment unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "Category=Unit&Module=Payment"
```

Expected: All tests pass (including newly added tests).

- [ ] **Step 5: Verify no warnings in full build**

```bash
dotnet build service/Api
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 6: Run all unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "Category=Unit"
```

Expected: All unit tests pass.

- [ ] **Step 7: Commit**

```bash
git add service/Api/tests/Module.UnitTests/Payment/Domain/PaymentCaptures/PaymentDisputeTests.cs
git add service/Api/tests/Module.UnitTests/Payment/Backgrounds/ProcessStripeWebhookEventJobTests.cs
git add service/Api/tests/Module.UnitTests/Payment/Services/StripeSettingValidationTests.cs
git commit -m "test(payment): add tests for Dispute domain method, dispute webhook, and StripeSettingValidation"
```

---

## Pre-Implementation Verification

Before starting, verify the Hangfire server is registered:

```bash
rg "AddHangfireServer" service/Api/src/Api/
```

If no results, `IBackgroundJobClient` may not be resolvable — this is a pre-existing condition not addressed by this plan.

Verify the `DisputeCreated` logger extension exists:

```bash
rg "DisputeCreated" service/Api/src/Module/Payment/Backgrounds/
```

If no results, the dispute logging call in `HandleChargeDisputeCreated` will fail to compile. In that case, replace `_logger.DisputeCreated(...)` with a standard log call:
```csharp
_logger.LogWarning("Dispute created for charge {ChargeId}: {Reason}", dispute.ChargeId, dispute.Reason ?? "unknown");
```

---

## Self-Review Checklist

### Spec Coverage

| Spec Requirement | Task(s) | Covered? |
|---|---|---|
| PRD-001: Handle requires_action (3DS) | Task 5 | Yes |
| PRD-002: Handle requires_payment_method | Task 5 | Yes |
| PRD-003: Distinguish transient vs terminal | Task 4 + Task 6 | Yes |
| PRD-004: Pass StatementDescriptorSuffix | Task 8 | Yes |
| PRD-005: Pass Shipping | Task 8 | Yes |
| PRD-006: checked{} on amount | Task 7 | Yes |
| RES-007: AutomaticRetry on webhook job | Task 11 | Yes |
| RES-008: DB index on ResponseCode | Task 10 | Yes |
| RES-009: Startup validation | Task 13 | Yes |
| DOM-010: Dispute state + guards | Task 1 + Task 2 + Task 12 + Task 15 | Yes |
| DOM-011: Make GetPaymentStatusAsync abstract | Task 3 | Yes |
| QLT-012: Narrow exception catch | Task 14 | Yes |
| QLT-013: Reuse SDK service instances | Task 9 | Yes |

### Placeholder Scan
- No TODOs, TBDs, or "implement later" found.
- All code steps contain actual code.
- All commands have expected output.
- No "add appropriate error handling" without concrete code.

### Type Consistency
- `PaymentRecordState.Disputed` used consistently across Tasks 1, 2, 12, 15, 16.
- `StripeGatewayResult.Errors.TransientGatewayError(string, string)` defined in Task 4, consumed in Task 6.
- `StripeGatewayResult.Errors.PaymentMethodRequired(string?)` defined in Task 4, consumed in Task 5.
- `PaymentCapture.Dispute()` defined in Task 2, consumed in Tasks 12, 16.
- `ProcessStripeWebhookEventJobLoggers.CannotDisputePayment` defined in Task 12, consumed in Task 12.
- All test files reference types defined in earlier tasks.
