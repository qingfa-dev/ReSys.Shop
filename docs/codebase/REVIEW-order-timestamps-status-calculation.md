# Review: Order Timestamps, Status, and Calculation Audit

- Date: 2026-08-16
- Scope: Ordering `Order` aggregate — timestamps, status transitions, money calculation, and the data surfaced to the order-detail pages.

## 1. Timestamp properties

All on `Order` (`service/Api/src/Module/Ordering/Domain/Orders/Order.cs:56-84`):

| Property | Meaning |
|---|---|
| `CreatedAtUtc` | Order created (factory). |
| `ModifiedAtUtc` / `ModifiedBy` | Last mutation (most transitions set it). |
| `PaymentProcessingAtUtc` | First-write-only (first payment processing transition). |
| `PaymentCompletedAtUtc` | Monotonic (never regresses). |
| `PaymentFailedAtUtc` | Monotonic. |
| `ShipmentShippedAtUtc` | First-write-only. |
| `ShipmentDeliveredAtUtc` | First-write-only. |
| `CompletedAtUtc` | Set at placement (`Place`/`Finalize`) and by `Complete` — doubles as "placed at". |
| `CanceledAtUtc` / `CanceledById` | Set by `Cancel`; cleared by `Resume`. |
| `ApprovedAtUtc` / `ApprovedById` | Set by `Approve`. |
| `DeletedAtUtc` / `DeletedBy` | Soft delete. |

## 2. Status enums

- `OrderStatus { Draft, Placed, Canceled, Expired }` (`Order.Enumerate.cs:5-11`)
- `CheckoutState { Address, PickDeliveryMethod, PickPaymentMethod, Confirm, Complete }` (`:14-21`)
- `OrderPaymentState { Completed, Failed, Void, BalanceDue, CreditOwed, Paid, Pending, Checkout, Invalid }` (`:24-35`)
- `ShipmentState { None, Pending, Partial, Shipped, Delivered, Canceled }` (order-level, derived; `Shared/Application/Domain/Orders/OrderFulfillmentState.cs`)
- Per-shipment `ShipmentStatus { Pending, Ready, Shipped, Delivered, Backorder, Canceled }` (`Shipment.Enumerate.cs`)

## 3. Transition → timestamp matrix

Verified in `Order.Method.Timestamps.cs` and `Order.Method.StateMachine.cs`:

| Method | Effect |
|---|---|
| `MarkPaymentProcessing` | `PaymentProcessingAtUtc` (first-write), `ModifiedAtUtc` |
| `MarkPaymentCompleted` | `PaymentCompletedAtUtc` (monotonic), `PaymentState=Completed` |
| `MarkPaymentFailed` | `PaymentFailedAtUtc` (monotonic), `PaymentState=Failed` |
| `MarkShipped` | `ShipmentShippedAtUtc` (first-write), `ShipmentState=Shipped` |
| `MarkDelivered` | `ShipmentDeliveredAtUtc` (first-write), `ShipmentState=Delivered` |
| `Place` / `Finalize` | `Status=Placed`, `CheckoutState=Complete`, `CompletedAtUtc`, `Number`, recalc |
| `Cancel` | `Status=Canceled`, `CanceledAtUtc`, `CanceledById` |
| `Resume` | `Status=Placed`, clears `CanceledAtUtc`/`CanceledById` (does NOT re-set `CompletedAtUtc`) |
| `Approve` | `ApprovedAtUtc`, `ApprovedById` |
| `Complete` | `CheckoutState=Complete`, `CompletedAtUtc` |
| `Delete` | `IsDeleted`, `DeletedAtUtc`, `DeletedBy` |
| `MarkPaymentAsPaid` | `PaymentState=Paid` — **no timestamp** |
| `Empty` | zeroes totals, clears items/adjustments |
| `Create` | `Status=Draft`, `CheckoutState=Address`, totals 0, `Number="DRAFT-…"` |

## 4. Money invariants

`Order.Method.Computation.cs`:

```
ItemCount          = sum(LineItem.Quantity)
ItemTotal          = sum(LineItem.Total)
AdjustmentTotal    = sum(LineItem.AdjustmentTotal)   // currently always 0 (nothing sets it)
                     + sum(eligible non-shipping Adjustments)
ShipmentTotal      = sum(eligible shipping-source Adjustments)
Total              = ItemTotal + ShipmentTotal + AdjustmentTotal
OutstandingBalance = Total - PaymentTotal
UpdatePaymentState : Canceled && PaymentTotal==0 → Void; OutstandingBalance>0 → BalanceDue; <0 → CreditOwed; else Paid
```

All consistent with the `@CAT-10` invariant `Total = ItemTotal + AdjustmentTotal + ShipmentTotal`.

## 5. Gaps found

1. **`OrderHistory` is dead code** — `OrderHistory.cs` declares an audit entry but there is no `DbSet`, no EF configuration, no table, no write, and no API surface. Only a legacy npm-Admin TS echo remains.
2. **`GetOrderTracking` loads an unused `Shipment`** (`GetOrderTracking.cs:30-32`) — the first shipment by `OrderId` is fetched but never read; all tracking timestamps come from the Order.
3. **`EstimatedDeliveryAt` / `DeliveryExceptionAt` never populated** — `OrderTrackingParameters` has both, always null; the `Shipment` entity carries `EstimatedDeliveryAtUtc` but it is never copied.
4. **`OrderDetailResponse` omits data that exists** — only `ApprovedAtUtc/CompletedAtUtc/CanceledAtUtc/CreatedAtUtc/ModifiedAtUtc` are exposed; the five payment/shipment timestamps, `PaymentCaptures`, and `Shipments` are excluded. Both get-order handlers `.Include` only `LineItems` + `Adjustments`.
5. **Admin SPA TS type omits `lineItems`** — the backend already returns them in `OrderDetailResponse`, but `app/Admin` does not declare them and instead lazy-fetches items via a separate endpoint (that endpoint also powers add/edit/remove management actions, so it stays).
6. **No timeline on Admin order detail** — the Storefront has one only inside the Track dialog (from the separate tracking endpoint).

## 6. Recommended (implemented as the enrichment feature)

- Derive a timeline from the existing order timestamps (no persistence).
- Embed `Payments`, `Shipments`, and the payment/shipment timestamps in `OrderDetailResponse` (both Admin + Storefront get-order handlers).
- Keep `OrderHistory` deferred (derived timeline chosen over a persisted audit log).
