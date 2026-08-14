# Orders + Payments + Fulfillment — Before / After

Date: 2026-08-14
Scope: checkout state model, status typing, shipment aggregate, migration strategy,
and the reliability findings from the architecture review.
Authoritative spec: `spec/spec-checkout-state-enum-alignment.md`

## 1. Checkout state machine

| | Before | After |
|---|---|---|
| Enum | `Address, Delivery, Payment, Confirm, Complete` | `Address, PickDeliveryMethod, PickPaymentMethod, Confirm, Complete` |
| `Payment` meaning | mislabeled — implied "processing" | `PickPaymentMethod` = select Card vs COD |
| Processing signal | `AdvanceCheckoutState` stamped `PaymentProcessingAt` on selection | stamped on `PaymentCapture.Process()` via `RecordOrderPaymentState{Processing}` |
| Dead code | `OrderConstant.CheckoutStep` string helpers | deleted |

## 2. Status strings → enums

| | Before | After |
|---|---|---|
| `TargetState` (Advance/Regress) | `string` + `Enum.TryParse` | `CheckoutState` |
| `CartResponseBase.CheckoutState` / `GetCartForCheckoutResponse.State` | `string` (`.ToString()`) | `CheckoutState` |
| `Order.PaymentState` / `Order.ShipmentState` | `string?` | `OrderPaymentState?` / `OrderFulfillmentState?` |
| `RecordOrderPaymentState.PaymentState` | `OrderPaymentState` static string class | `PaymentTimelineState` enum |
| `PaymentForCheckoutResponse.State` | `string` (`p.State == "Pending"`) | `bool IsPending` |
| SPA | untyped `string` / `z.string()` | typed unions + `z.enum` |

## 3. Shipment / fulfillment

| | Before | After |
|---|---|---|
| Model | free-floating `Order.ShipmentState` enum (admin-set) | `Shipment` aggregate in Shipping (tracking number, method, status, timestamps) |
| Card relationship | none | `Shipment` → `OrderId` (Guid), `ShippingMethodId` (FK) |
| Lifecycle | `Pending/Delivered/Partial/Ready/Backorder/Canceled` on Order | `ShipmentStatus { Pending, Ready, Shipped, Delivered, Backorder, Canceled }` |
| Order status | `OrderShipmentState` | derived `OrderFulfillmentState { None, Pending, Partial, Shipped, Delivered, Canceled }` via `RecordOrderShipmentStateCommand` |
| Creation | admin `UpdateOrderShipmentState` endpoint | auto-create Pending shipment on placement |
| Timestamps | `Order.ShippedAt/DeliveredAt` set by admin | authoritative on `Shipment`; Order keeps derived mirrors |

## 4. Migration

| | Before | After |
|---|---|---|
| Strategy | SQL `UPDATE` backfill | EF Core bidirectional value converters (read legacy + canonical, write canonical) |
| In-progress migrations | two uncommitted migrations + dirty snapshot | dropped; snapshot reset; new migrations from corrected model |

## 5. Reliability (from architecture review)

| | Before | After (recommended) |
|---|---|---|
| Webhook ingest | validate → Hangfire enqueue raw payload | validate → persist `WebhookEvent` (unique Stripe id) → 200 → claim/process |
| Dedup | `ProcessedStripeEventIds` jsonb list (unbounded) | `WebhookEvent` idempotency store |
| Payment→Order commit | separate commits/contexts | outbox (single tx + async dispatch) |
| Business timestamps | `UtcNow` processing time | Stripe `event.Created` + separate `ProcessedAtUtc` |
| Refund/capture | `Amount`/`RefundedAmount`, invariant broken | `CapturedAmount`, enforce `Refunded ≤ Captured ≤ Amount` |
| Correlation | `ResponseCode` overloaded (cs_→pi_) | `StripeSessionId` + `StripePaymentIntentId` |
| State transitions | 4 call sites | single `PaymentService` choke point |
| Failed events | lost after 3 retries | DLQ / reconciliation job |

## 6. Priority summary

- **P0**: lost events · non-atomic completion→placement · timestamp conflation ·
  refund invariant · `ResponseCode` mutation.
- **P1**: idempotency store · job concurrency handling · duplicated state logic ·
  `succeeded` vs `session.completed` race.
- **P2**: anemic `Payment`/naming · dead code · `Resume` payment consequence.
- **P3**: correlation/tracing/metrics.
