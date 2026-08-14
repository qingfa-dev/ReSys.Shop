---
goal: Enforce the payment capture/refund amount invariants — add CapturedAmount, cap RefundedAmount ≤ CapturedAmount ≤ Amount, and make Capture/Refund/ReconcileRefunded honor them.
version: 1.0
date_created: 2026-08-14
last_updated: 2026-08-14
owner: Billing
status: 'Planned'
tags: [refactor, billing, payment, invariant, refund, capture]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The `Payment` entity's declared invariant (`CapturedTotal <= Amount;
RefundedTotal <= CapturedTotal`) is not backed by code: there is no `CapturedAmount`
field, `Refund` caps against `Amount` (authorized) not captured, `ReconcileRefunded`
has no upper bound, and `Capture` sets `Completed` even for partial amounts. This
plan adds `CapturedAmount` and enforces `RefundedAmount <= CapturedAmount <= Amount`.

**Spec:** `spec/spec-checkout-state-enum-alignment.md` FND-004

## 1. Requirements & Constraints

- **REQ-001**: `Payment.CapturedAmount` (decimal) added; `Capture(amount)` records it and enforces `amount <= Amount - CapturedAmount`.
- **REQ-002**: `Refund(amount)` enforces `amount <= CapturedAmount - RefundedAmount`; `ReconcileRefunded(total)` caps `total <= CapturedAmount`.
- **REQ-003**: Partial capture is either supported (CapturedAmount accumulates) or rejected with a clear error — decision: **support partial capture**, `Completed` only when `CapturedAmount == Amount`.
- **CON-001**: Result objects, not exceptions; zero-warning build.
- **PAT-001**: Domain methods in `PaymentCapture.Method.State.cs`; gateway call stays in `PaymentProcessingService`.

## 2. Implementation Steps

### Implementation Phase 1 — Model + invariants

- GOAL-001: Add `CapturedAmount` and enforce the amount invariants.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add `CapturedAmount` to `Payment` + EF config + migration. | | |
| TASK-002 | Rework `Capture`/`CanCapture`/`CanRefund`/`Refund`/`ReconcileRefunded` to enforce invariants. | | |
| TASK-003 | Update `PaymentProcessingService.CaptureAsync` to pass the capture amount through and handle partial capture. | | |
| TASK-004 | Unit tests for partial capture, over-refund rejection, reconcile cap. | | |

#### TASK-001: Field + migration

`PaymentCapture.cs`: `public decimal CapturedAmount { get; set; }`; `PaymentRecordConfiguration.cs`: `builder.Property(x => x.CapturedAmount).HasPrecision(...)`. Migration `AddPaymentCapturedAmount`.

#### TASK-002: Domain invariants

`PaymentCapture.Method.State.cs`:

```csharp
public static bool CanCapture(this Payment payment, decimal amount)
    => payment.State is PaymentRecordState.Processing or PaymentRecordState.Pending
       && amount > 0
       && amount <= payment.Amount - payment.CapturedAmount;

public static Result Capture(this Payment payment, decimal amount)
{
    if (!payment.CanCapture(amount))
        return PaymentCaptureResult.Failure.AmountExceedsAuthorized;
    payment.CapturedAmount += amount;
    payment.CompletedAtUtc = DateTimeOffset.UtcNow;
    payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
    if (payment.CapturedAmount >= payment.Amount)
        payment.State = PaymentRecordState.Completed; // fully captured
    return Result.Ok(PaymentCaptureResult.Success.Captured(payment.Number, amount));
}

public static bool CanRefund(this Payment payment, decimal amount)
    => payment.State is PaymentRecordState.Completed or PaymentRecordState.Disputed
       && amount > 0
       && amount <= payment.CapturedAmount - payment.RefundedAmount;

public static Result ReconcileRefunded(this Payment payment, decimal totalRefunded)
{
    if (payment.State is not (PaymentRecordState.Completed or PaymentRecordState.Disputed))
        return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);
    if (totalRefunded <= payment.RefundedAmount) return Result.Ok();
    if (totalRefunded > payment.CapturedAmount)
        return PaymentCaptureResult.Failure.AmountExceedsAuthorized;
    payment.RefundedAmount = totalRefunded;
    payment.RefundedAtUtc = DateTimeOffset.UtcNow;
    return Result.Ok();
}
```

`UncapturedAmount()` → `Amount - CapturedAmount` (0 when `CapturedAmount >= Amount`).

#### TASK-003: Service

`PaymentProcessingService.CaptureAsync` currently defaults `amount ??= payment.Amount` (`PaymentProcessingService.cs:41`) — change to `amount ??= payment.UncapturedAmount()`. The admin `CapturePayment.cs:47` already uses `payment.UncapturedAmount()`. Update the response to return `CapturedAmount`. `RefundAsync` uses `CanRefund` (updated).

#### TASK-004: Tests

Add/extend the existing domain tests `service/Api/tests/Module.UnitTests/Billing/Domain/PaymentCaptures/PaymentTransitionTests.cs` and `Payment.Extensions.Tests.cs`:
- **Update** `Capture_ShouldSetStateToCompleted` (`PaymentTransitionTests.cs:56-67`) — it currently asserts `Capture(50m)` on a 100m payment sets `State == Completed`, which conflicts with REQ-003 (partial capture); change it to assert partial capture leaves `State == Processing` and a full capture completes.
- Add: over-capture rejected; over-refund rejected; `ReconcileRefunded` > `CapturedAmount` rejected; monotonic reconcile.

## 3. Alternatives

- **ALT-001**: Reject partial capture outright. Rejected — less flexible; accumulate is standard.

## 4. Dependencies

- **DEP-001**: `PaymentProcessingService`, `CapturePayment`, `RefundPayment` (existing).

## 5. Files

- **FILE-001**: `service/Api/src/Module/Billing/Domain/PaymentCaptures/PaymentCapture.cs`.
- **FILE-002**: `service/Api/src/Module/Billing/Domain/PaymentCaptures/PaymentCapture.Method.State.cs`.
- **FILE-003**: `service/Api/src/Module/Billing/Persistence/Configurations/Payments/PaymentRecordConfiguration.cs` (class `PaymentConfiguration`).
- **FILE-004**: `service/Api/src/Module/Billing/Services/Processing/PaymentProcessingService.cs`.
- **FILE-005**: migration.

## 6. Testing

- **TEST-001**: TASK-004 unit tests green.
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests` green.

## 7. Risks & Assumptions

- **RISK-001**: Existing `ReconcileRefunded` callers (webhook `charge.refunded`) may now fail when Stripe's reported total exceeds `CapturedAmount`; treat as a data anomaly and log loudly.

## 8. Related Specifications / Further Reading

- [spec-checkout-state-enum-alignment.md](../spec/spec-checkout-state-enum-alignment.md) FND-004
- [refactor-webhook-reliability-1.md](./refactor-webhook-reliability-1.md)
