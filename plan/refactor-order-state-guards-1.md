---
goal: Harden the Order state machine against lifecycle leaks found in the order-status-lifecycle audit (Completed/Expired cancellability, refund-on-completed payment-state corruption, Resume re-derivation, RecordOrderShipmentState write guards, cancel side-effect ordering, UpdateOrderStatus divergent cancel path)
version: 1.0
date_created: 2026-08-18
owner: Ordering
status: 'Completed'
tags: refactor, ordering, state-machine, guards, payment
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

A deep audit of the Order state machines (after `OrderStatus.Completed` was added) found that completion was bolted onto guards written for a 4-state world. The lifecycle `Draft → Placed → (Canceled | Completed) + Expired` hangs together at the surface, but the seams leak:

- `Order.Cancel()` guards only `Draft`/`Canceled`, so **`Completed` and `Expired` orders can be canceled** — and no handler (admin or storefront) adds a `Status == Placed` guard. A Completed order with a still-Pending shipment cancels successfully with money never refunded (`VoidOrderPayments` skips Completed captures) and stock never released (`wasPlaced=false`).
- **Refunding a Completed order corrupts `PaymentState`**: a fully-refunded Completed order has `PaymentTotal=0` → `OutstandingBalance=Total>0` → `BalanceDue` ("customer owes" despite full refund). The `Void` rule only matches `Canceled`.
- **`Resume()` never re-derives completion** — an order resumed from Canceled stays `Placed` even if all shipments are Delivered, until a new Delivered event fires.
- **`RecordOrderShipmentState` writes derived state with no Status guard** — a sync on a Canceled/Completed order can overwrite `ShipmentState` (e.g. a new shipment on a Completed order regresses it to `Partial`); `order.Complete("System")` result is discarded.
- **`CancelOrderAdmin` dispatches `VoidOrderPayments` before running all in-process guards** — if a shipment is Delivered, the handler returns error without SaveChanges but gateway payments were already voided.
- **`UpdateOrderStatus` has a third, divergent cancel path** — no payment void, no shipment cancel, no `ShipmentState=Canceled` write, and it adjusts stock directly instead of `ReturnConsumedForOrderAsync`.

Deferred (documented, not in scope): `CompletedAtUtc` timeline overload (Place vs Complete), `Delete()` no cleanup path for Canceled/Completed, `Finalize`/`Place` consolidation, dead `AllowCancel()`, legacy SPA numeric map.

## 1. Requirements & Constraints

- **REQ-001**: `Order.Cancel()` must reject `Status is Completed or Expired`; only `Placed` (and the existing Canceled/Draft rejections) may cancel.
- **REQ-002**: `CancelOrderAdmin` and `CancelOrder` handlers must guard `Status == Placed` before invoking any side effect (void payments, shipment cancel, stock return).
- **REQ-003**: `CancelOrderAdmin` must run ALL in-process guards (shipment-cancel loop) BEFORE any gateway side effect (`VoidOrderPayments`), so a failed guard leaves gateway payments untouched.
- **REQ-004**: Canceling a Completed order must be blocked at the domain layer; if a Completed order somehow reaches a cancel path, completed captures must be refunded (not voided) and stock returned — but the primary fix is to reject at the guard.
- **REQ-005**: `UpdatePaymentState` must not derive `BalanceDue` for a fully-refunded order; a Completed order with `PaymentTotal == 0` due to full refund must not show `BalanceDue` (add a full-refund rule: e.g. `PaymentTotal == 0 && RefundedAmount > 0` → `CreditOwed` or a documented neutral).
- **REQ-006**: `Resume()` (and `ResumeOrder` handler) must re-derive completion: after Resume, if `ShipmentState == Delivered`, transition to `Completed` (via `order.Complete("System")` or a shared helper).
- **REQ-007**: `RecordOrderShipmentStateCommandHandler` must guard the `ShipmentState` write (and `MarkShipped`/`MarkDelivered` timestamps) on a valid `(Status, ShipmentState)` pair — skip writes for `Canceled`/`Completed`/`Expired` orders that cannot accept the derived state; and must check the `order.Complete("System")` result (log/handle failure).
- **REQ-008**: `UpdateOrderStatus` Canceled path must be aligned with `CancelOrderAdmin` semantics (void pending payments, cancel shipments, set `ShipmentState=Canceled`, return stock via `ReturnConsumedForOrderAsync`), or be documented as intentionally divergent and gated to `Placed` with the new guard.
- **SEC-001**: No new security surface; actor attribution in `ModifiedBy` preserved (`"System"` for unattended transitions).
- **CON-001**: `TreatWarningsAsErrors=true` — all switches over `OrderStatus`/`ShipmentState` must remain exhaustive.
- **CON-002**: Result-object pattern; domain methods return `Result`, never throw for business-state transitions.
- **CON-003**: No schema change; enums persist as strings, so no migration is required for guard changes.
- **CON-004**: The 3 pre-existing `OrderStatusValueConverterTests` NRE failures are unrelated (verified) and out of scope.
- **GUD-001**: Follow the existing domain method + handler test patterns (`Order.Method.Tests.cs`, `CancelOrderAdminCascadeTests.cs`).

## 2. Implementation Steps

### Implementation Phase 1 — Domain guards + payment-state rule

- GOAL-001: Harden `Order.Cancel()`, fix `UpdatePaymentState` for full-refund, and add Resume completion re-derivation at the domain layer.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `Order.Method.StateMachine.cs` `Cancel()` (~lines 36-56): add a guard rejecting `Completed` and `Expired` before any state mutation: `if (order.Status is OrderStatus.Completed or OrderStatus.Expired) return OrderResult.Errors.InvalidStatusTransition;` (or a dedicated error). Keep the existing Canceled/Draft guards. Update the summary comment. | ✅ | 2026-08-18 |
| TASK-002 | `Order.Method.StateMachine.cs` `Resume()` (~lines 58-71): after restoring `Placed`, if `order.ShipmentState == ShipmentState.Delivered`, transition to `Completed` — call a shared helper (e.g. `order.Complete("System")` after setting Status=Placed) OR set `order.Status = OrderStatus.Completed` directly with the same timestamp/modifier fields. Prefer reusing `Complete()` so `CompletedAtUtc`/`ModifiedBy` stay consistent. Return the combined result. | ✅ | 2026-08-18 |
| TASK-003 | `Order.Method.Computation.cs` `UpdatePaymentState()` (~lines 40-52): add a full-refund rule before the `BalanceDue` check so a fully-refunded order does not derive `BalanceDue`. Decide the target: `CreditOwed` when `PaymentTotal == 0 && RefundedAmount > 0 && Status is not Canceled`. Verify `RefundedAmount` is summed across captures (check `RecomputePaymentState` computation). Update the summary comment. | ✅ | 2026-08-18 |
| TASK-004 | Add domain tests in `Order.Method.Tests.cs`: Cancel rejects `Completed` and `Expired`; Resume re-derives `Completed` when `ShipmentState==Delivered`; Resume of a non-delivered Canceled order stays `Placed`; `UpdatePaymentState` of a fully-refunded Completed order is NOT `BalanceDue`. | ✅ | 2026-08-18 |

### Implementation Phase 2 — Handler guards + side-effect ordering

- GOAL-002: Guard both cancel handlers on `Status == Placed`, reorder CancelOrderAdmin side effects, and align UpdateOrderStatus.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | `CancelOrderAdmin.cs` (~lines 38-58): before `order.Cancel(userId)`, add `if (order.Status != OrderStatus.Placed) return OrderResult.Errors.InvalidStatusTransition;`. Keep `wasPlaced`/`RecomputePaymentState`. | ✅ | 2026-08-18 |
| TASK-006 | `CancelOrderAdmin.cs`: reorder so the shipment-cancel loop (all in-process guards) runs BEFORE `sender.Send(VoidOrderPaymentsCommand)`. If any shipment cancel fails, return the error WITHOUT dispatching the void (gateway untouched). Move the void dispatch after the shipment loop succeeds. | ✅ | 2026-08-18 |
| TASK-007 | `CancelOrder.cs` (Storefront, ~lines 46-52): add the same `entity.Status != OrderStatus.Placed` guard before `entity.Cancel(userId)`. | ✅ | 2026-08-18 |
| TASK-008 | `UpdateOrderStatus.cs` Canceled path (~lines 48-67): align with `CancelOrderAdmin` semantics — after `entity.Cancel(...)`, void pending payments, cancel shipments, set `ShipmentState = ShipmentState.Canceled`, and return stock via `IStockReservationService.ReturnConsumedForOrderAsync` (replacing the direct `AdjustStockAsync` loop). Follow the exact structure in `CancelOrderAdmin.cs`. If `UpdateOrderStatus` is intentionally a status-only override, instead gate it and document; prefer alignment. | ✅ | 2026-08-18 |
| TASK-009 | `RecordOrderShipmentState.cs` (~lines 15-26): guard the `ShipmentState` write + `MarkShipped`/`MarkDelivered` timestamp mirrors on a valid order state — skip when `order.Status is Canceled or Completed or Expired` (return `Result.Ok()` or a no-op). Check the `order.Complete("System")` result: `if (completeResult.IsFailure) { /* log via injected ILogger if present, else return completeResult.Errors */ }`. Add `ILogger` to the handler ctor if it's not already there (follow `ShipmentFulfillmentSyncService`/other handler patterns). | ✅ | 2026-08-18 |
| TASK-010 | Add/update handler tests: `CancelOrderAdminCascadeTests` — cancel of a `Completed` order returns failure; shipment-cancel failure does NOT dispatch `VoidOrderPayments`; `UpdateOrderStatusTests` — Canceled path returns stock + voids + cancels shipments; `RecordOrderShipmentStateTests` — sync on Canceled/Completed order is a no-op; auto-complete failure is surfaced. | ✅ | 2026-08-18 |

### Implementation Phase 3 — Verification

- GOAL-003: Build, full test suite, and feature-convention checks.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | `dotnet build service/Api/src/Api/Api.csproj -v q --nologo` → 0 warnings / 0 errors. | ✅ | 2026-08-18 |
| TASK-012 | Run the full `Module.UnitTests` suite via `cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0 && ./Module.UnitTests` — expect ONLY the 3 pre-existing `OrderStatusValueConverterTests` failures. | ✅ | 2026-08-18 |
| TASK-013 | `bash scripts/check-feature-conventions.sh` → all pass. Update this plan's task table and set `status` to `Completed` once green. | ✅ | 2026-08-18 |

## 3. Alternatives

- **ALT-001**: Block cancel only at the handlers (not the domain). Rejected — every future cancel caller would need to remember the guard; the domain is the single choke point.
- **ALT-002**: Introduce a `Refunded` `OrderPaymentState` member. Rejected for now — `CreditOwed` already models over-payment/refund surplus; adding a new member requires enum + converter + SPA changes (deferred follow-up if needed).
- **ALT-003**: Make `UpdateOrderStatus` stay as-is and just document the divergence. Rejected — two cancel semantics (stock/void vs none) is a data-integrity trap; alignment is the safer default.

## 4. Dependencies

- **DEP-001**: `IStockReservationService.ReturnConsumedForOrderAsync` (exists — Inventory) for the aligned `UpdateOrderStatus` stock return.
- **DEP-002**: `VoidOrderPaymentsCommand` (exists — Billing) used by `CancelOrderAdmin`; reused by `UpdateOrderStatus` if aligned.
- **DEP-003**: `ShipmentMethod.ComputeFulfillmentState` (exists) — `ShipmentState == Delivered` ⟺ all shipments delivered (verified) — basis for Resume re-derivation.
- **DEP-004**: `ILogger` availability for `RecordOrderShipmentStateCommandHandler` (check ctor; follow existing handler patterns).

## 5. Files

- **FILE-001**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.StateMachine.cs` — Cancel guard, Resume re-derivation.
- **FILE-002**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Computation.cs` — UpdatePaymentState full-refund rule.
- **FILE-003**: `service/Api/src/Module/Ordering/Features/Admin/Orders/Cancel/CancelOrderAdmin.cs` — Placed guard + side-effect reorder.
- **FILE-004**: `service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs` — Placed guard.
- **FILE-005**: `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateStatus/UpdateOrderStatus.cs` — aligned Canceled path.
- **FILE-006**: `service/Api/src/Module/Ordering/Features/Storefront/RecordOrderShipmentState/RecordOrderShipmentState.cs` — Status guard + checked Complete result.
- **FILE-007**: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs` — domain guard/refund tests.
- **FILE-008**: `service/Api/tests/Module.UnitTests/Ordering/Features/Admin/Orders/Cancel/CancelOrderAdminCascadeTests.cs` + `UpdateStatus/UpdateOrderStatusTests.cs` + `Storefront/RecordOrderShipmentState/RecordOrderShipmentStateTests.cs` — handler tests.

## 6. Testing

- **TEST-001**: `Order.Method.Tests` — Cancel rejects Completed and Expired.
- **TEST-002**: `Order.Method.Tests` — Resume re-derives Completed when `ShipmentState==Delivered`; non-delivered stays Placed.
- **TEST-003**: `Order.Method.Tests` — fully-refunded Completed order `PaymentState` is NOT `BalanceDue`.
- **TEST-004**: `CancelOrderAdminCascadeTests` — Completed cancel fails; shipment-cancel failure does NOT dispatch VoidOrderPayments.
- **TEST-005**: `UpdateOrderStatusTests` — Canceled path aligns (stock via ReturnConsumedForOrderAsync, void, shipment cancel).
- **TEST-006**: `RecordOrderShipmentStateTests` — sync on Canceled/Completed is a no-op; auto-complete failure surfaced.
- **TEST-007**: Full `Module.UnitTests` suite — only the 3 pre-existing `OrderStatusValueConverterTests` failures.

## 7. Risks & Assumptions

- **RISK-001**: Aligning `UpdateOrderStatus`'s Canceled path changes its behavior — existing callers/tests may rely on the old (minimal) semantics. Mitigated by TASK-010 tests and the guard.
- **RISK-002**: The full-refund `PaymentState` rule (REQ-005) is a business decision — confirm `CreditOwed` is the desired display for a fully-refunded Completed order before merge.
- **RISK-003**: `Resume()` calling `Complete()` re-stamps `CompletedAtUtc` — acceptable (it reflects actual completion) but verify the timeline still reads sanely.
- **ASSUMPTION-001**: `VoidOrderPaymentsCommand` skips Completed captures by design (refund instead) — confirmed in its handler filter.
- **ASSUMPTION-002**: `ShipmentState == Delivered` is the authoritative completion trigger (verified via `ComputeFulfillmentState`).
- **ASSUMPTION-003**: The 3 `OrderStatusValueConverterTests` NREs remain pre-existing and unrelated (verified by the audit).

## 8. Related Specifications / Further Reading

- [feature-order-status-lifecycle-1.md](./feature-order-status-lifecycle-1.md) — the plan that added `OrderStatus.Completed`; this plan hardens its seams.
- [docs/codebase/ARCHITECTURE.md](../docs/codebase/ARCHITECTURE.md) — Ordering domain state machine and layer responsibilities.
- [refactor-api-status-enums-1.md](./refactor-api-status-enums-1.md) — enum typing and `ActivityStatus.Completed`.