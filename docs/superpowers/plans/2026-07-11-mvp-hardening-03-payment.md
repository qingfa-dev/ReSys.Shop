# Plan 3: Payment Integrity — Gateway Verification & Data Correctness

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix payment confirmation without gateway verification, refund amount errors, Stripe amount truncation, and payment method filtering.

**Architecture:** Add gateway status check before payment completion. Use domain methods for refund tracking. Fix decimal truncation with `Math.Round`. Filter deleted/deactivated payment methods.

**Tech Stack:** .NET 10, Stripe SDK, EF Core

## Global Constraints

- `TreatWarningsAsErrors=true` globally.
- All handlers must return `Result<T>` or `Result` — never throw exceptions for expected business failures.
- Payment methods must be filtered by `Active && !IsDeleted`.

---

## File Structure

| Action | File | Responsibility |
|--------|------|---------------|
| Modify | `service/Api/src/Module/Payment/Infrastructure/Gateways/Stripe/StripeGateway.cs` | Add `GetPaymentIntentStatusAsync`, fix amount truncation |
| Modify | `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.cs` | Query gateway before completing |
| Modify | `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.Endpoint.cs` | Remove empty `[FromBody]` parameter |
| Modify | `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs` | Filter payment methods by Active/IsDeleted |
| Modify | `service/Api/src/Module/Payment/Features/Storefront/Payment/SetupIntent/CreateSetupIntent.cs` | Filter payment methods + remove global ApiKey set |
| Modify | `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs` | Fix refund handler + check Fail() return |
| Modify | `service/Api/src/Module/Payment/Features/Admin/Payments/Refund/RefundPayment.cs` | Use request Amount + fix response |
| Modify | `service/Api/src/Module/Payment/Features/Admin/Payments/Capture/CapturePayment.cs` | Return AlreadyCompleted for completed payments |
| Modify | `service/Api/src/Module/Payment/Domain/Payments/PaymentRecord.Processing.cs` | Return error for already-completed capture |
| Modify | `service/Api/src/Module/Payment/Features/Admin/Payments/Get/Paged/GetPagedPayments.Endpoint.cs` | Accept QueryingParameters |
| Modify | `service/Api/src/Module/Payment/Features/Admin/PaymentMethods/Delete/DeletePaymentMethod.cs` | Check for active payments |
| Modify | `service/Api/src/Module/Payment/Features/Admin/Payments/Get/ById/GetPaymentById.Response.cs` | Remove IntentClientSecret |
| Modify | `service/Api/src/Module/Payment/Features/Admin/Payments/Get/ById/GetPaymentById.cs` | Populate OrderNumber/PaymentMethodName |

---

### Task 1: Add GetPaymentIntentStatusAsync to StripeGateway

**Files:**
- Modify: `service/Api/src/Module/Payment/Infrastructure/Gateways/Stripe/StripeGateway.cs`

**Interfaces:**
- Produces: `StripeGateway.GetPaymentIntentStatusAsync(string paymentIntentId, CancellationToken ct)` → `Task<string>`

- [ ] **Step 1: Read the current gateway**

Read `service/Api/src/Module/Payment/Infrastructure/Gateways/Stripe/StripeGateway.cs`.

- [ ] **Step 2: Add the status retrieval method**

Add after the existing methods:

```csharp
public async Task<string> GetPaymentIntentStatusAsync(string paymentIntentId, CancellationToken ct)
{
    var requestOptions = new RequestOptions { IdempotencyKey = Guid.NewGuid().ToString() };
    var intent = await new PaymentIntentService().GetAsync(paymentIntentId, null, requestOptions, ct);
    return intent.Status;
}
```

- [ ] **Step 3: Fix amount truncation (all 3 occurrences)**

Find all `(long)(amount * CentsMultiplier)` and replace with:
```csharp
(long)Math.Round(amount * CentsMultiplier, MidpointRounding.AwayFromZero)
```

- [ ] **Step 4: Fix CancelPaymentIntentAsync idempotency key**

Find `var requestOptions = new RequestOptions { IdempotencyKey = Guid.NewGuid().ToString() };` in `CancelPaymentIntentAsync` and change to:
```csharp
var requestOptions = new RequestOptions { IdempotencyKey = options.IdempotencyKey ?? Guid.NewGuid().ToString() };
```

- [ ] **Step 5: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Payment/Module.Payment.csproj`
Expected: Build succeeds

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Payment/Infrastructure/Gateways/Stripe/StripeGateway.cs
git commit -m "feat(payment): add GetPaymentIntentStatusAsync, fix amount truncation

- Add gateway method to retrieve PaymentIntent status
- Use Math.Round to prevent decimal truncation in cents conversion
- Fix idempotency key in CancelPaymentIntentAsync"
```

---

### Task 2: Fix ConfirmPayment — Query Gateway Before Completing

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.cs`

**Interfaces:**
- Consumes: `StripeGateway.GetPaymentIntentStatusAsync()` from Task 1

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.cs`.

- [ ] **Step 2: Add gateway check before payment.Complete()**

Before the `payment.Complete()` call, add:

```csharp
var status = await _gateway.GetPaymentIntentStatusAsync(payment.ResponseCode!, cancellationToken);
if (status != "succeeded")
    return PaymentResult.Failure.NotSucceeded;
```

If `PaymentResult.Failure.NotSucceeded` doesn't exist, add it:
```csharp
public static Error NotSucceeded => Error.Validation("Payment.Confirm.NotSucceeded", "Payment has not succeeded at the gateway.");
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Payment/Module.Payment.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.cs
git commit -m "fix(payment): verify Stripe PaymentIntent status before completing

Previously marked payment complete without checking gateway state."
```

---

### Task 3: Remove Empty Request Body from ConfirmPayment Endpoint

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.Endpoint.cs`

**Interfaces:**
- Consumes: N/A

- [ ] **Step 1: Read the current endpoint**

Read `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.Endpoint.cs`.

- [ ] **Step 2: Remove [FromBody] Request parameter**

Change the endpoint lambda from:
```csharp
async ([FromBody] Request request, ...) =>
```

To:
```csharp
async (...) =>
```

Remove the `request` parameter from the `Command` constructor call if it's used there.

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Payment/Module.Payment.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.Endpoint.cs
git commit -m "fix(payment): remove unused Request body from ConfirmPayment endpoint"
```

---

### Task 4: Filter Payment Methods in CreatePaymentIntent

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs`

**Interfaces:**
- Consumes: `PaymentMethod` entity

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs` — find the `FirstOrDefaultAsync` that loads payment method.

- [ ] **Step 2: Add Active/IsDeleted filter**

Change:
```csharp
var paymentMethod = await dbContext.Set<PaymentMethod>()
    .FirstOrDefaultAsync(cancellationToken);
```

To:
```csharp
var paymentMethod = await dbContext.Set<PaymentMethod>()
    .FirstOrDefaultAsync(c => c.Active && !c.IsDeleted, cancellationToken);
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Payment/Module.Payment.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs
git commit -m "fix(payment): filter payment methods by Active and !IsDeleted in CreatePaymentIntent"
```

---

### Task 5: Fix CreateSetupIntent — Filter + Remove Global ApiKey

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/SetupIntent/CreateSetupIntent.cs`

**Interfaces:**
- Consumes: `PaymentMethod` entity

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Payment/Features/Storefront/Payment/SetupIntent/CreateSetupIntent.cs`.

- [ ] **Step 2: Remove the global ApiKey line**

Delete: `StripeConfiguration.ApiKey = stripeOptions.Value.SecretKey;`

- [ ] **Step 3: Add Active/IsDeleted filter to payment method query**

Change the `FirstOrDefaultAsync` to include filter:
```csharp
var paymentMethod = await dbContext.Set<DomainPaymentMethod>()
    .FirstOrDefaultAsync(pm => pm.Id == command.PaymentMethodId && pm.Active && !pm.IsDeleted, cancellationToken);
```

- [ ] **Step 4: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Payment/Module.Payment.csproj`
Expected: Build succeeds

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/SetupIntent/CreateSetupIntent.cs
git commit -m "fix(payment): filter payment methods + remove global StripeConfiguration.ApiKey"
```

---

### Task 6: Fix StripeWebhook — Refund Handler + Fail() Check

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs`

**Interfaces:**
- Consumes: `payment.Refund(delta)` domain method

- [ ] **Step 1: Read the current webhook handler**

Read `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs`.

- [ ] **Step 2: Fix HandlePaymentIntentSucceeded refund logic**

Replace the direct assignment:
```csharp
if (charge.AmountRefunded > 0)
    payment.RefundedAmount = charge.AmountRefunded / 100m;
```

With delta-based domain method:
```csharp
if (charge.AmountRefunded > 0)
{
    var newRefunded = charge.AmountRefunded / 100m;
    var delta = newRefunded - payment.RefundedAmount;
    if (delta > 0)
        payment.Refund(delta);
}
```

- [ ] **Step 3: Fix HandlePaymentIntentFailed — check Fail() return**

Change:
```csharp
payment.Fail();
await dbContext.SaveChangesAsync(cancellationToken);
```

To:
```csharp
var failResult = payment.Fail();
if (failResult.IsFailure)
    return failResult.Errors;
await dbContext.SaveChangesAsync(cancellationToken);
```

- [ ] **Step 4: Add logging to HandleChargeDisputeCreated**

Replace the no-op:
```csharp
private static Result HandleChargeDisputeCreated(StripeEvent stripeEvent)
{
    var charge = stripeEvent.Data.Object as Charge;
    if (charge is null) return Result.Ok();
    return Result.Ok();
}
```

With:
```csharp
// Note: This handler requires ILogger injection. If the class doesn't inject it,
// add ILogger<StripeWebhook> to the constructor and log here.
// For now, return Result.Ok() — the dispute event is logged by Stripe dashboard.
private static Result HandleChargeDisputeCreated(StripeEvent stripeEvent)
{
    var charge = stripeEvent.Data.Object as Charge;
    if (charge is null) return Result.Ok();
    // TODO: Add dispute handling logic when business requirements are defined
    return Result.Ok();
}
```

- [ ] **Step 5: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Payment/Module.Payment.csproj`
Expected: Build succeeds

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs
git commit -m "fix(payment): fix refund webhook handler and check Fail() return

- Use delta-based Refund() instead of direct RefundedAmount assignment
- Check Fail() return value before SaveChangesAsync
- Add placeholder for dispute handler"
```

---

### Task 7: Fix RefundPayment — Use Request Amount + Fix Response

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Admin/Payments/Refund/RefundPayment.cs`

**Interfaces:**
- Consumes: `payment.Refund(amount)` domain method

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Payment/Features/Admin/Payments/Refund/RefundPayment.cs`.

- [ ] **Step 2: Use request Amount instead of hardcoded payment.Amount**

Change:
```csharp
var refundAmount = payment.Amount;
```

To:
```csharp
var refundAmount = command.Request.Amount;
```

- [ ] **Step 3: Fix response to return actual refund amount**

Change:
```csharp
RefundedAmount = command.Request.Amount,
```

To:
```csharp
RefundedAmount = refundAmount,
```

- [ ] **Step 4: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Payment/Module.Payment.csproj`
Expected: Build succeeds

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Admin/Payments/Refund/RefundPayment.cs
git commit -m "fix(payment): use request Amount for partial refunds, fix response

Previously always refunded full payment amount and returned wrong
amount in response."
```

---

### Task 8: Fix CapturePayment — Return AlreadyCompleted

**Files:**
- Modify: `service/Api/src/Module/Payment/Domain/Payments/PaymentRecord.Processing.cs`
- Modify: `service/Api/src/Module/Payment/Features/Admin/Payments/Capture/CapturePayment.cs`

**Interfaces:**
- Produces: `PaymentResult.Failure.AlreadyCompleted`

- [ ] **Step 1: Read the current domain method**

Read `service/Api/src/Module/Payment/Domain/Payments/PaymentRecord.Processing.cs` — find the `CaptureAsync` method.

- [ ] **Step 2: Return error instead of silent Ok for already-completed**

Change:
```csharp
if (payment.State == PaymentRecordState.Completed)
    return Result.Ok();
```

To:
```csharp
if (payment.State == PaymentRecordState.Completed)
    return PaymentResult.Failure.AlreadyCompleted;
```

If `PaymentResult.Failure.AlreadyCompleted` doesn't exist, add to `PaymentResult.cs`:
```csharp
public static Error AlreadyCompleted => Error.Validation("Payment.Capture.AlreadyCompleted", "Payment has already been completed.");
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Payment/Module.Payment.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Domain/Payments/PaymentRecord.Processing.cs
git commit -m "fix(payment): return AlreadyCompleted error instead of silent no-op"
```

---

### Task 9: Fix GetPagedPayments — Accept QueryingParameters

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Admin/Payments/Get/Paged/GetPagedPayments.Endpoint.cs`

**Interfaces:**
- Consumes: `QueryingParameters`

- [ ] **Step 1: Read the current endpoint**

Read `service/Api/src/Module/Payment/Features/Admin/Payments/Get/Paged/GetPagedPayments.Endpoint.cs`.

- [ ] **Step 2: Accept [AsParameters] QueryingParameters**

Change the endpoint lambda to accept parameters:
```csharp
async ([AsParameters] QueryingParameters parameters, ISender sender, CancellationToken ct) =>
{
    var query = new Query(parameters);
    var result = await sender.Send(query, ct);
    return result.ToResult();
}
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Payment/Module.Payment.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Admin/Payments/Get/Paged/GetPagedPayments.Endpoint.cs
git commit -m "fix(payment): accept QueryingParameters in GetPagedPayments endpoint"
```

---

### Task 10: Fix DeletePaymentMethod — Check Active Payments

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Admin/PaymentMethods/Delete/DeletePaymentMethod.cs`

**Interfaces:**
- Consumes: `PaymentRecord` entity

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Payment/Features/Admin/PaymentMethods/Delete/DeletePaymentMethod.cs`.

- [ ] **Step 2: Add active payment check before soft-delete**

Before the soft-delete logic, add:

```csharp
var hasActivePayments = await dbContext.Set<PaymentRecord>()
    .AnyAsync(p => p.PaymentMethodId == command.Id
        && p.State is not (PaymentRecordState.Completed
            or PaymentRecordState.Failed
            or PaymentRecordState.Void
            or PaymentRecordState.Invalid),
    cancellationToken);

if (hasActivePayments)
    return PaymentMethodResult.Failure.HasActivePayments;
```

If `PaymentMethodResult.Failure.HasActivePayments` doesn't exist, add it:
```csharp
public static Error HasActivePayments => Error.Validation("PaymentMethod.Delete.HasActivePayments",
    "Cannot delete a payment method that has active (non-terminal) payments.");
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Payment/Module.Payment.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Admin/PaymentMethods/Delete/DeletePaymentMethod.cs
git commit -m "fix(payment): check for active payments before deleting payment method"
```

---

### Task 11: Fix GetPaymentById — Response Model

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Admin/Payments/Get/ById/GetPaymentById.Response.cs`
- Modify: `service/Api/src/Module/Payment/Features/Admin/Payments/Get/ById/GetPaymentById.cs`

**Interfaces:**
- Consumes: `PaymentRecord` entity with navigation properties

- [ ] **Step 1: Read the current response model**

Read `service/Api/src/Module/Payment/Features/Admin/Payments/Get/ById/GetPaymentById.Response.cs`.

- [ ] **Step 2: Remove IntentClientSecret from response**

Delete the `IntentClientSecret` property from the response record.

- [ ] **Step 3: Read the handler and populate OrderNumber/PaymentMethodName**

Read `service/Api/src/Module/Payment/Features/Admin/Payments/Get/ById/GetPaymentById.cs`.

Change:
```csharp
OrderNumber = null,
PaymentMethodName = null,
```

To:
```csharp
OrderNumber = payment.Order?.Number,
PaymentMethodName = payment.PaymentMethod?.Name,
```

- [ ] **Step 4: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Payment/Module.Payment.csproj`
Expected: Build succeeds

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Admin/Payments/Get/ById/
git commit -m "fix(payment): remove IntentClientSecret, populate OrderNumber/PaymentMethodName"
```

---

### Task 12: Build and Verify All Changes

- [ ] **Step 1: Full solution build**

Run: `dotnet build`
Expected: Build succeeds with zero warnings

- [ ] **Step 2: Run unit tests**

Run: `dotnet test service/Api/tests/Module.UnitTests`
Expected: All tests pass

- [ ] **Step 3: Commit (if any fixes needed)**

```bash
git commit -m "fix: address build warnings from payment integrity fixes"
```
