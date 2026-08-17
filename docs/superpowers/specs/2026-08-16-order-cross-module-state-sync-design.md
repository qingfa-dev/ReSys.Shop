# Design: Order Cross-Module State Synchronization

- Date: 2026-08-16
- Status: Approved (derived payment state; 3 phases)
- Scope: Ordering, Billing, Shipping, Inventory — cross-module state propagation

## Problem

The Order aggregate's derived state (`PaymentState`, `PaymentTotal`,
`OutstandingBalance`, `ShipmentState`) is out of sync with its related entities
(payments, shipments, stock) because the runtime never recomputes it and some
cascades are missing. See `docs/codebase/REVIEW-order-timestamps-status-calculation.md`
and the cross-module audit:

1. `UpdatePaymentState()` and `PaymentTotal` writes exist only in the seeder.
   Runtime paths hardcode `PaymentState = Completed/Failed` via
   `MarkPaymentCompleted/Failed`; `PaymentTotal` stays 0, `OutstandingBalance`
   stays `= Total`.
2. `CreateShipment` never syncs `ShipmentState` (stays `None` after placement).
3. Order cancel never cancels shipments / sets `ShipmentState`.
4. Cancel returns stock via `AdjustStockAsync` ("returned", single-location)
   instead of the reservation lifecycle.

## Decision

Derive the Order's payment state from its captures at runtime, sync fulfillment
state on shipment creation/cancellation, and return consumed stock through the
reservation service (multi-location).

## Phase A — Payment → Order

- New `OrderMethod.RecomputePaymentState()`:
  `PaymentTotal = Σ CapturedAmount − Σ RefundedAmount` (idempotent, partial- and
  refund-aware), then `OutstandingBalance = Total − PaymentTotal`, then
  `UpdatePaymentState()` (`Paid`/`BalanceDue`/`CreditOwed`/`Void`).
- `MarkPaymentCompleted`/`MarkPaymentFailed` become timestamp-only (drop the
  `PaymentState` assignment). `PaymentState` is now derived-only;
  `Completed`/`Failed`/`Pending`/`Checkout` are no longer set (timestamps record
  the events).
- `RecordOrderPaymentStateCommandHandler` includes `PaymentCaptures` and
  recomputes on Completed/Failed. `CompleteCheckoutForPayment` includes captures
  and recomputes after `MarkPaymentCompleted`.
- Admin `CapturePayment` dispatches `Completed` only when the capture is fully
  `Completed`, else `Processing`.
- New `RecomputeOrderPaymentStateCommand` (Ordering), dispatched from admin
  `RefundPayment` and the `charge.refunded` webhook. Void/dispute/canceled/expired
  move no money → no mirror (correct as-is).

## Phase B — Shipment → Order

- `CreateShipment` injects `ShipmentFulfillmentSyncService` and calls
  `SyncOrderFulfillmentAsync(orderId)` after save → placed order gets
  `ShipmentState = Pending`.
- `CancelOrder`/`CancelOrderAdmin` load `Shipments`, `shipment.Cancel()` each
  (Pending/Ready/Backorder), and set `ShipmentState = Canceled`.

## Phase C — Cancel → Stock

- New `IStockReservationService.ReturnConsumedForOrderAsync(Guid orderId)`:
  load `Fulfilled` reservations (`CartToken == orderId`), `stockItem.Restock(qty)`
  at each reservation's `StockLocationId`, create a `canceled` `StockMovement`,
  and mark the reservation `Released` (new `StockReservation.Return()` transition).
- `CancelOrder`/`CancelOrderAdmin` call it instead of the `AdjustStockAsync` loop.

## Out of Scope

- Persisting `OrderHistory`; `GetOrderTracking` fixes; enum cleanup (keep unused
  values for compatibility).

## Testing

- Backend unit tests for: `RecomputePaymentState` (paid/balance-due/credit-owed/
  void + partial + refund), timestamp-only `MarkPayment*`, recompute handlers,
  partial-capture, `ReturnConsumedForOrderAsync` (multi-location), shipment
  cancel cascade. Existing `RecordOrderPaymentState`/`CompleteCheckoutForPayment`/
  `CapturePayment`/`RefundPayment`/`CancelOrder`/`CreateShipment` tests updated.
