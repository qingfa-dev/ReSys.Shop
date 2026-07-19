---
title: Payment System Critical Bug Fixes and Risk Remediation
version: 1.0
date_created: 2026-07-19
owner: Platform Team
tags: design, payment, bugfix, remediation
---

# Introduction

Fix 6 defects discovered in the Payment module during code review: 3 bugs that cause data inconsistency or schema-creation failure, and 3 risks that harm test fidelity, observability, and code maintainability. All fixes comply with the codebase rules: Result objects, not exceptions; forward-only dependency between `Shared` and `Module`; warnings-as-errors.

## 1. Purpose & Scope

**Purpose**: Define exact, verifiable changes to eliminate the identified defects in the Payment module (`service/Api/src/Module/Payment/`).

**Scope**: Payment module only. Affects domain entities (`PaymentCapture`), persistence configuration (`PaymentRecordConfiguration`), background jobs (`ProcessStripeWebhookEventJob`), feature handlers (`VoidOrderPayments`), gateway providers (`BogusGateway`, `Gateway` base), and processing service (`PaymentProcessingService`).

**Out of scope**: Stripe webhook dispatch flow (no runtime bug found), Integration test updates (follow-up task), Migration generation (follow-up).

**Audience**: Agents and developers implementing these fixes.

**Assumptions**: `IApplicationDbContext.BeginTransactionAsync(IsolationLevel, CancellationToken)` returns `IDatabaseTransaction` with `CommitAsync`/`RollbackAsync`. `PaymentMethod` soft-delete interceptor already converts `Remove()` to `IsDeleted = true` updates.

## 2. Definitions

| Term | Definition |
|---|---|
| Result | The `readonly partial record struct Result` from `Shared.Application.Models.Results` — carries `IsSuccess`, `Message`, `Errors`, and an implicit conversion from `Error`. |
| `Result<T>` | Generic variant wrapping a `T Value` plus the same error plumbing. |
| Domain method | A `static` extension method in `PaymentCaptureMethod` that validates preconditions and mutates entity state, returning `Result` or `Result<T>`. |
| Hangfire job | A background job enqueued via `IBackgroundJobClient.Enqueue<TJob>()`, executed outside the HTTP request scope. Auto-retries 10 times on exception. |
| Soft-delete | The entity implements `ISoftDeletable`; an EF Core interceptor converts `Remove(entity)` to setting `IsDeleted = true` instead of issuing SQL `DELETE`. |
| Idempotency key | A unique string (`shop-PAY-YYYYMMDD-XXXXXXXX`) passed to Stripe's `Idempotency-Key` header, causing Stripe to return a cached response for duplicate requests instead of re-executing. |
| Transaction scope | `await using var tx = await dbContext.BeginTransactionAsync(...)` wrapping all EF Core operations in the handler — DB changes are all-or-nothing. |
| Fail-fast | Stop processing immediately on the first unrecoverable error; do not attempt remaining items. |

## 3. Requirements, Constraints & Guidelines

### FW-001: Background job must check domain Result before SaveChangesAsync

Every call to a domain method (`Complete()`, `Fail()`, `Refund()`) in `ProcessStripeWebhookEventJob.ExecuteAsync` and its private helpers MUST check `result.IsFailure` before calling `SaveChangesAsync`. On failure, the job MUST log a structured warning with payment ID and current state, then return without saving.

**Constraint**: Do NOT throw exceptions from domain Result failures. Hangfire auto-retries on exception, but domain failures (AlreadyCompleted, InvalidStateTransition) are non-transient — retry produces identical failure.

**Pattern to follow** (from `ConfirmPayment.cs:65-66`):
```csharp
var completeResult = payment.Complete();
if (completeResult.IsFailure) return completeResult.Errors;
await dbContext.SaveChangesAsync(cancellationToken);
```

### FK-002: PaymentMethodId foreign key must be nullable

`PaymentCapture.PaymentMethodId` MUST change from `Guid` to `Guid?`. The EF Core configuration in `PaymentRecordConfiguration` MUST keep `OnDelete(DeleteBehavior.SetNull)` — which is now valid because the column is nullable.

**Constraint**: `PaymentCapture.Validation.ApplyPaymentMethodIdRules` uses `.NotEmpty()` on `Guid`, which correctly handles `Guid?` (default `Guid.Empty` is considered empty). No validation change needed.

**Rationale**: `SetNull` on a non-nullable column is rejected by PostgreSQL at migration time (`SqlException`). The soft-delete interceptor masks this today (DELETE never fires), but any raw SQL or `ExecuteDelete` against PaymentMethod would hit the same error. Nullable `Guid?` correctly models "a PaymentCapture that may no longer reference an active PaymentMethod after soft-delete."

### TX-003: VoidOrderPayments must use transaction scope and fail-fast

The `VoidOrderPaymentsCommandHandler.Handle` method MUST:
1. Wrap the foreach loop in `await using var transaction = await dbContext.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct)`
2. Rollback and return error on the first unregistered gateway (`gatewayResult.IsFailure`) instead of `continue`
3. Rollback and return error on the first failed `VoidTransactionAsync` call
4. Commit only after `SaveChangesAsync` succeeds
5. Rollback in a catch block for unexpected exceptions

**Constraint**: Do NOT aggregate errors across payments (partial void = no). Fail-fast is correct for 1-3 payment batches with idempotency-safe retries.

### GW-004: BogusGateway must override GetPaymentStatusAsync

`BogusGateway` MUST override `GetPaymentStatusAsync(string responseCode, CancellationToken ct)` to return a status that reflects the simulated payment result. The base class default (`Gateway.GetPaymentStatusAsync`) always returns `"succeeded"`, which masks test/debug failures when `ConfirmPayment` checks the gateway status.

**Guideline**: Minimal implementation: return `"succeeded"` for the success test card (`4242 4242 4242 4242`) stored in a concurrent dictionary keyed by `responseCode`; return `"requires_payment_method"` for declined/insufficient-funds test cards. If storing per-intent state is deemed over-engineering, at minimum override with a configurable result.

### GW-005: CancelAsync must accept GatewayOptions parameter

`PaymentProcessingService.CancelAsync` MUST accept `GatewayOptions options` as a parameter instead of constructing `new GatewayOptions { Email = string.Empty, Customer = string.Empty }` internally. The two callers (`VoidAsync` line 75, elsewhere line 189) already construct `GatewayOptions` and can pass it through.

**Constraint**: Do NOT change the public API of `IPaymentProcessingService` — `CancelAsync` is private.

### DOC-006: Complete() comment must not claim idempotency

The XML doc comment on `PaymentCaptureMethod.Complete()` MUST change from `"idempotent if already completed"` to `"returns AlreadyCompleted error if already completed"`. The method returns `Failure.AlreadyCompleted`, which is NOT idempotent behavior (idempotent would return `Result.Ok()` with a success message).

**Constraint**: Do NOT change the method behavior. Callers (`ConfirmPayment.cs:45`) check for `AlreadyCompleted` as an error. Making the method truly idempotent would require auditing all callers.

## 4. Interfaces & Data Contracts

### 4.1 PaymentCapture entity — property change

```csharp
// PaymentCapture.cs:27 — before
public Guid PaymentMethodId { get; set; }

// PaymentCapture.cs:27 — after
public Guid? PaymentMethodId { get; set; }
```

### 4.2 ProcessStripeWebhookEventJob — handler pattern

```csharp
// Before (line 69-72)
if (payment.State == PaymentRecordState.Completed) return;
payment.Complete();
await _dbContext.SaveChangesAsync(ct);

// After
if (payment.State == PaymentRecordState.Completed) return;
var result = payment.Complete();
if (result.IsFailure)
{
    _logger.LogWarning("Cannot complete payment {PaymentId} (state={State}): {Message}",
        payment.Id, payment.State, result.Message);
    return;
}
await _dbContext.SaveChangesAsync(ct);
```

Same pattern for `Fail()` on line 85 and `Refund(delta)` on line 104.

### 4.3 VoidOrderPayments — transaction-scoped handler

```csharp
public async Task<Result> Handle(VoidOrderPaymentsCommand command, CancellationToken ct)
{
    var payments = await dbContext.Set<PaymentCapture>()
        .Where(p => p.OrderId == command.OrderId)
        .ToListAsync(ct);

    await using var transaction = await dbContext.BeginTransactionAsync(
        IsolationLevel.ReadCommitted, ct);
    try
    {
        foreach (var payment in payments)
        {
            var gatewayResult = gatewayRegistry.GetGateway(payment.ProviderKey);
            if (gatewayResult.IsFailure)
            {
                await transaction.RollbackAsync(ct);
                return PaymentCaptureResult.Failure.ProviderNotRegistered(payment.ProviderKey);
            }
            var options = new GatewayOptions { /* ... same as current ... */ };
            var voidResult = await processingService.VoidTransactionAsync(
                payment, gatewayResult.Value, options, null, ct);
            if (voidResult.IsFailure)
            {
                await transaction.RollbackAsync(ct);
                return voidResult.Errors;
            }
        }
        await dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Result.Ok();
    }
    catch
    {
        await transaction.RollbackAsync(ct);
        throw;
    }
}
```

### 4.4 BogusGateway — status override

```csharp
public override Task<string> GetPaymentStatusAsync(string responseCode, CancellationToken ct)
{
    // Minimal: match test card results
    return Task.FromResult("succeeded");
}
```

### 4.5 CancelAsync — parameter injection

```csharp
// Before
private async Task<Result<PaymentProcessingResult>> CancelAsync(
    PaymentCapture payment, IPaymentGatewayActionProvider gateway, CancellationToken ct = default)

// After
private async Task<Result<PaymentProcessingResult>> CancelAsync(
    PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken ct = default)
```

Remove internal `new GatewayOptions { ... }` construction; use passed `options`.

### 4.6 Complete() — comment fix

```csharp
// Before
// Update: Processing/Pending → Completed — idempotent if already completed

// After
// Update: Processing/Pending → Completed — returns AlreadyCompleted error if already completed
```

## 5. Acceptance Criteria

- **AC-001**: Given a Stripe `payment_intent.succeeded` webhook for a payment already in `Completed` state, When the background job calls `payment.Complete()`, Then `result.IsFailure` is true, the job logs a warning and does NOT call `SaveChangesAsync`.
- **AC-002**: Given a Stripe `payment_intent.payment_failed` webhook for a payment in `Void` state, When the background job calls `payment.Fail()`, Then `result.IsFailure` is true, the job logs a warning and returns.
- **AC-003**: Given a `charge.refunded` webhook with a positive `AmountRefunded` on a non-Completed payment, When `payment.Refund(delta)` returns failure, Then the job logs a warning and does NOT call `SaveChangesAsync`.
- **AC-004**: Given `PaymentMethodId` is `Guid?` and `DeleteBehavior.SetNull`, When a migration is generated (`dotnet ef migrations add`), Then no `SqlException` occurs and the FK constraint is created with `ON DELETE SET NULL` on a nullable column.
- **AC-005**: Given Payment 1 and Payment 2 in an order, and Payment 2's gateway fails, When `VoidOrderPayments` calls `RollbackAsync`, Then Payment 1's in-memory `State = Void` mutation is discarded and no `SaveChangesAsync` runs.
- **AC-006**: Given Payment 1 has an unregistered `ProviderKey`, When `VoidOrderPayments` calls `GetGateway`, Then the handler calls `RollbackAsync` and returns `ProviderNotRegistered` error — NOT `Result.Ok()`.
- **AC-007**: Given a bogus payment with the declined test card, When `ConfirmPayment` calls `GetPaymentStatusAsync()`, Then the BogusGateway returns a non-succeeded status so `ConfirmPayment` returns `NotSucceeded`.
- **AC-008**: Given `CancelAsync` receives a `GatewayOptions` with non-empty Email, When the gateway void is called, Then the Stripe metadata includes the customer email and order ID.
- **AC-009**: The XML doc comment on `Complete()` no longer contains the word "idempotent".
- **AC-010**: `dotnet build` succeeds with warnings-as-errors after all changes. `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Payment"` passes.

## 6. Test Automation Strategy

- **Test Levels**: Unit tests for domain state transitions (existing `Payment.Validation.Tests.cs`, `Payment.Extensions.Tests.cs`); Unit tests for background job handler logic (existing `ProcessStripeWebhookEventJobTests.cs`); Integration tests for transaction rollback in VoidOrderPayments (existing `Api.Tests/Scenarios/Payment/`).
- **Frameworks**: xUnit (implicit from `Module.UnitTests`), FluentAssertions, Moq.
- **Test updates required**:
  - `ProcessStripeWebhookEventJobTests.cs`: Add cases for `Complete()` and `Fail()` returning failure — verify no `SaveChangesAsync` call and log entry.
  - `PaymentMethod.Validation.Tests.cs`: No change needed (`.NotEmpty()` on `Guid?` is handled).
  - `VoidOrderPaymentsTests.cs` (new or existing): Verify rollback on gateway failure, rollback on unregistered provider.
- **CI/CD Integration**: `dotnet test` runs in GitHub Actions on PR/push — no new pipeline required.
- **Coverage Requirements**: No change from existing project defaults (opt-in via `/p:CollectCoverage=true`).

## 7. Rationale & Context

### FW-001: Why check Result in background jobs

Hangfire has zero visibility into custom `Result` types — it only detects unhandled exceptions. A job that silently ignores a failed state transition is marked "succeeded" and removed from the queue. The current pre-checks (`if (payment.State == Completed) return`) duplicate domain logic and are fragile: any new failure condition added to `Complete()` will not be caught.

### FK-002: Why nullable + SetNull not Restrict

`Restrict`/`NoAction` would preserve the non-nullable FK but throws `DbUpdateException` on any hard-delete. Since `PaymentMethod` uses soft-delete, this won't fire in normal operations, but it's semantically inaccurate: the business intent is that PaymentCaptures survive PaymentMethod deletion. `Guid?` + `SetNull` models this intent at the schema level.

### TX-003: Why transaction scope and fail-fast

Batch voiding without a transaction means an early gateway failure leaves in-memory mutations stranded. While `SaveChangesAsync` isn't currently called inside the loop, a future refactor could introduce it, creating a distributed consistency gap. Fail-fast is appropriate because orders have few payments, idempotency keys make retries safe, and partial void is semantically wrong (an order's payments should all be voided or none).

### GW-004: Why Bogus needs status override

`ConfirmPayment.cs:60-61` checks `status != "succeeded"` against the gateway. Without an override, any payment confirmed via BogusGateway always appears to have succeeded. This makes integration testing of the confirm flow unreliable — a bug in the confirm logic would be masked because every simulated payment "succeeds" at the gateway.

### GW-005: Why CancelAsync needs GatewayOptions

`CancelAsync` constructs `GatewayOptions` internally with empty `Email` and `Customer`. But its caller already has a `GatewayOptions` with populated fields. Passing the options through avoids metadata loss on Stripe PaymentIntent cancellation records.

### DOC-006: Why not make Complete() truly idempotent

Changing `Complete()` to return `Result.Ok()` when already completed would break `ConfirmPayment.cs:45`, which specifically checks for `PaymentCaptureResult.Failure.AlreadyCompleted` as a terminal state. A behavioral change would require auditing all callers, updating response semantics, and potentially changing API contracts. The simpler fix is correcting the misleading comment.

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: Stripe API (payment gateway) — idempotency keys must be present on all void/refund calls for safe batch retry.

### Third-Party Services
- **SVC-001**: Hangfire (background job processing) — auto-retry on exception only; no awareness of `Result` objects.

### Infrastructure Dependencies
- **INF-001**: PostgreSQL (Npgsql via EF Core) — `ON DELETE SET NULL` requires nullable column; `BeginTransactionAsync` provides DB-level atomicity.

### Data Dependencies
- **DAT-001**: PaymentCapture table — schema change from `NOT NULL` to nullable for `PaymentMethodId` column requires a migration.

### Technology Platform Dependencies
- **PLT-001**: .NET 10, EF Core 10 — `ExecuteUpdate`/`ExecuteDelete` (EF Core 7+) available but not applicable for this fix set. `IApplicationDbContext.BeginTransactionAsync` required.

### Compliance Dependencies
- None.

## 9. Examples & Edge Cases

### FW-001: Webhook delivered out of order

```
Given: Stripe sends payment_intent.succeeded before charge.refunded
And: charge.refunded arrives first (payment not yet completed)
When: HandleChargeRefunded calls payment.Refund(delta)
Then: Refund() returns failure (state not Completed)
And: Job logs warning and returns without SaveChangesAsync
```

### FK-002: Existing data migration

```
Given: 100 PaymentCapture rows all have valid PaymentMethodId values
When: Migration changes PaymentMethodId from Guid NOT NULL to Guid NULL
Then: ALTER COLUMN DROP NOT NULL succeeds (no data migration needed)
And: All existing rows retain their PaymentMethodId values
```

### TX-003: Gateway voids, DB commit fails

```
Given: Payment voided successfully on Stripe
And: SaveChangesAsync throws DbUpdateException
When: catch block calls RollbackAsync
Then: In-memory entity changes are discarded
And: Stripe is voided but DB is not (distributed gap)
And: Retry picks up the same payment — VoidTransactionAsync at line 111 detects Void state and returns success
```

### GW-004: ConfirmPayment with Bogus declined card

```
Given: BogusGateway simulated a declined payment (state != Completed)
When: ConfirmPayment calls gatewat.GetPaymentStatusAsync(responseCode)
Then: BogusGateway returns "requires_payment_method" (not "succeeded")
And: ConfirmPayment returns PaymentCaptureResult.Failure.NotSucceeded
```

## 10. Validation Criteria

- **VC-001**: `dotnet build` passes with 0 warnings across the entire solution.
- **VC-002**: EF Core migration generates without `SqlException` after `PaymentMethodId` → `Guid?`.
- **VC-003**: `await payment.Complete()` followed by `result.IsFailure` check runs before `SaveChangesAsync` at all 3 call sites in `ProcessStripeWebhookEventJob`.
- **VC-004**: `VoidOrderPayments` handler contains `BeginTransactionAsync`, `RollbackAsync` in both error paths, `CommitAsync` in the success path.
- **VC-005**: `BogusGateway` contains an `override` of `GetPaymentStatusAsync`.
- **VC-006**: `CancelAsync` signature includes `GatewayOptions options` parameter.
- **VC-007**: Grep for `idempotent if already completed` in `PaymentCapture.Method.State.cs` returns 0 matches; `AlreadyCompleted` appears instead.

## 11. Related Specifications / Further Reading

- [spec-design-feature-conventions-remediation.md](/spec/spec-design-feature-conventions-remediation.md) — Command/Query/Response conventions
- [Microsoft Learn: Cascade Delete](https://learn.microsoft.com/en-us/ef/core/saving/cascade-delete) — EF Core DeleteBehavior behavior tables
- [Hangfire: Dealing with Exceptions](https://docs.hangfire.io/en/latest/background-processing/dealing-with-exceptions.html)
- [EF Core Transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions)
- [research_payment_fixes/findings_efcore_setnull.md](/research_payment_fixes/findings_efcore_setnull.md)
- [research_payment_fixes/findings_hangfire_result.md](/research_payment_fixes/findings_hangfire_result.md)
- [research_payment_fixes/findings_batch_void.md](/research_payment_fixes/findings_batch_void.md)
