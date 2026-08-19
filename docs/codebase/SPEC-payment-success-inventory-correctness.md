# Spec: Payment-Success → Inventory Correctness

**Date:** 2026-08-16
**Status:** Approved for planning (companion plan: `plan/fix-payment-inventory-correctness-1.md`)
**Related review:** `docs/codebase/REVIEW-payment-success-inventory-flow.md`

---

## Problem Statement

On a successful payment, the platform reduces inventory by **twice** the ordered quantity. The root cause is that stock is reserved twice for the same cart — once in `AddToCart` (`cartToken = cart.Id`) and again in `CreatePaymentIntent` (`cartToken = orderId`, the same value) — and `ConsumeForOrderAsync` then picks **every** `Reserved` row for that token rather than only the ordered quantity. Separately, releasing or expiring a reservation *adds back* to `CountOnHand` a quantity that was never removed, inflating physical stock. Compounding the damage: no `StockMovement` record is written for a sale, the reservation-expiry sweep is never scheduled, and expired Stripe sessions fail to release their reservations (wrong key).

These defects silently corrupt `StockItem.CountOnHand` — the single source of truth for physical stock — causing lost inventory, fabricated inventory, and a missing audit trail, each with direct fulfillment and financial consequences.

## Goals

- **G1** — A successful payment deducts exactly the ordered quantity per line item (no double deduction).
- **G2** — Every sale writes an audit `StockMovement` (`originatorType: "Order"`, `reason: "sold"`, negative quantity), matching the existing cancellation/return audit pattern.
- **G3** — Releasing or expiring a reservation never mutates `CountOnHand` (availability is derived, never stored).
- **G4** — At most one reservation set exists per cart at consume time.
- **G5** — Expired reservations are actually swept (job registered), and expired Stripe sessions release their reservations (correct key).

## Non-Goals

- **NG1** — Do **not** wrap consume + place in a single cross-module transaction (deferred; see Open Questions).
- **NG2** — Do **not** introduce an invoice entity or change the "billing = PaymentCapture row" model.
- **NG3** — Do **not** change the reservation accounting model. Availability stays derived: `Available = max(CountOnHand − Σ activeReserved, 0)`. Only remove the erroneous `CountOnHand` mutations.
- **NG4** — No DB unique constraint on shipments in this effort (separate concern).
- **NG5** — No price re-read at checkout, no payment↔order amount/currency cross-check at webhook time (separate concern).

## Requirements

### P0 (must ship)

- **R1** — `ConsumeForOrderAsync` consumes **at most** the ordered quantity per variant. Any reservation row beyond the needed quantity is released, never picked. (defense-in-depth against duplicate reservations)
- **R2** — `CreatePaymentIntent` releases prior cart reservations before reserving, so exactly one reservation set exists at consume time. (single source)
- **R3** — `ReleaseReservationsAsync` and `ExpireReservationsAsync` stop mutating `CountOnHand`.
- **R4** — `ConsumeForOrderAsync` creates a `StockMovement` per picked reservation (`quantity: -picked`, `previousCountOnHand`, `originatorType: "Order"`, `originatorId: orderId`, `reason: "sold"`).
- **R5** — `ReservationExpiryJobScheduler` is registered as a hosted service.
- **R6** — `HandleCheckoutSessionExpired` releases reservations by `cartToken`, not `orderId`.

### P1 (nice-to-have)

- **R7** — Reservation ↔ line-item consistency guard at consume: a `Reserved` row whose variant is absent from the order (or whose total exceeds ordered) is released, not picked. (folded into R1's capping)

### P2 (future)

- **R8** — Single transaction across consume + place with explicit `DbUpdateConcurrencyException` handling.

## Acceptance Criteria

- **AC1** — Given `CountOnHand=10` and a cart of qty 2 (one variant), after add-to-cart → create-intent → payment success, `CountOnHand==8` and there is exactly one `StockMovement` with `Quantity==-2`, `Reason=="sold"`, `OriginatorType=="Order"`, `OriginatorId==orderId`.
- **AC2** — Given a cart reserved qty 2 then released, `CountOnHand` is unchanged.
- **AC3** — Given a `payment_intent.succeeded` webhook delivered twice, `CountOnHand` is reduced exactly once.
- **AC4** — Given duplicate reservation rows (simulating the pre-fix state), `ConsumeForOrderAsync` picks only the ordered quantity and releases the surplus.
- **AC5** — `dotnet build` clean (warnings-as-errors); `bash scripts/check-feature-conventions.sh` and `bash scripts/check-cross-module-refs.sh` green.

## Success Metrics

- **Leading:** Zero `CountOnHand` deltas on release/expiry (verified by unit tests); exactly one `sold` movement per consumed quantity.
- **Lagging:** No "double deduction" or "stock inflation" reports in production; on-hand reconciliation (physical vs system) stays within tolerance.

## Open Questions

- **O1 (deferred)** — Cross-module transaction abstraction for R8. Owner: architecture. Non-blocking.
- **O2** — Sale `StockMovement.PreviousCountOnHand`: the value *before the current pick* (per-row), matching `DecrementInternalAsync` (`StockItem.Service.Implementation.cs:396-405`). Resolved: per-row value before `Pick`. Non-blocking.

## Evidence Map (verified against current working tree)

| Finding | Location | Confirmed |
|---|---|---|
| Double reservation (AddToCart + CreatePaymentIntent, same `CartToken`) | `AddToCart.cs:73-78`, `CreatePaymentIntent.cs:74-90`, `StockReservation.Service.Implementation.cs:137-138` | ✅ |
| Release-before-reserve only on retry branch, not first-time | `CreatePaymentIntent.cs:48-71` (guarded by `PickPaymentMethod`) | ✅ |
| Consume picks every `Reserved` row, ignoring ordered qty | `StockReservation.Service.Implementation.cs:333-356` | ✅ |
| Release/expire inflate `CountOnHand` (`+=`) | `StockReservation.Service.Implementation.cs:202-209, 265-272` | ✅ |
| `ReleaseCartReservationsAsync` correctly does NOT inflate | `StockReservation.Service.Implementation.cs:221-249` + test `:332-353` | ✅ |
| Existing tests encode the buggy "restore stock" behavior | `StockReservationServiceTests.cs:255, 437, 470` | ✅ |
| No `StockMovement` written on sale | `StockReservation.Service.Implementation.cs:285-360` | ✅ |
| `"sold"` movement only on cancel/return path | `StockItem.Service.Implementation.cs:396-405` | ✅ |
| Expiry scheduler unregistered | `Inventory.Extension.cs:11-22` | ✅ |
| Session-expiry release uses wrong key (`OrderId`) | `ProcessStripeWebhookEventJob.cs:418`; reservations store `OrderId=null` (`:148-149`) | ✅ |
