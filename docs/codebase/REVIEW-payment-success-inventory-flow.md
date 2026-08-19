# Review: Payment-Success → Inventory Flow

**Date:** 2026-08-16
**Scope:** Deep end-to-end review of the payment-success → inventory flow across Cart, Ordering, Billing/Payment, Inventory, Shipping.
**Status:** Open — findings verified against current working tree (branch `feature/implement-storefront`, heavy uncommitted WIP).

> **Headline:** Inventory IS reduced on payment success, but (a) it is reduced **twice** per order due to duplicate reservations, (b) with **no stock-movement record**, and (c) the surrounding release/expiry logic can both fail to free stock *and* inflate on-hand when it does.

---

## 1. Executive Summary

> **After a successful payment, does inventory get reduced?** → **PARTIALLY — and dangerously.**

Stock **is** deducted: `ConsumeForOrderAsync` (`StockReservation.Service.Implementation.cs:339-356`) calls `stockItem.Pick(qty)` which decrements `CountOnHand`. But there is a **double-deduction bug**: reservations are created **twice** for the same cart (once in `AddToCart`, once in `CreatePaymentIntent`), and `ConsumeForOrderAsync` picks *every* `Reserved` row for the cart token, so a quantity-2 order deducts **4** from on-hand.

> **Is a stock movement created?** → **NO.**

The sale path (`ConsumeForOrderAsync`) only decrements `CountOnHand` and flips reservations to `Fulfilled`; it **never creates a `StockMovement` row**. The `"sold"` movement exists only in the legacy `StockItemService.AdjustStockAsync` path, which in production is called **only on cancellations/returns** (positive delta). So the payment-success path has **zero stock-movement audit trail**.

---

## 2. Actual End-to-End Flow (real code, not assumed architecture)

```
POST /api/storefront/cart/items                       AddToCart.cs:73      ReserveForVariantAsync (CartToken=cartId, TTL 15)
        │
        v
POST /api/storefront/cart/payment/intent              CreatePaymentIntent.cs:74-90   ReserveForVariantAsync AGAIN (CartToken=orderId)
        │                                            (also: payment capture "PAY-…", Stripe Checkout Session via CreateCheckoutSessionAsync)
        v
STRIPE HOSTED CHECKOUT  ──success──►  webhook
        │
        v
POST /api/storefront/billing/webhooks/stripe          StripeWebhook.cs:24-75  (sig-validated, deduped on stripe_event_id, persisted, Hangfire-enqueued)
        │
        v
ProcessStripeWebhookEventJob.ExecuteAsync              :47-90
  ├─ payment_intent.succeeded      HandlePaymentIntentSucceeded :132-183
  │     ├─ payment.Complete()      PaymentCapture → Completed    :149
  │     └─ CompleteCheckoutForPaymentCommand (MediatR)           :168
  └─ checkout.session.completed    HandleCheckoutSessionCompleted :306-383 (guards payment_status=paid :342)
        └─ CompleteCheckoutForPaymentCommand                      :365
        │
        v
CompleteCheckoutForPayment.cs:14-58   (idempotent: only Draft orders; refuses unless payment IsCompleted)
        │
        v
CheckoutPlacementService.PlaceAsync  CheckoutPlacementService.cs:19-72
  ├─ AdvanceCheckoutState(Confirm)     :25
  ├─ ValidateCheckoutPrerequisites     :28
  ├─ ConsumeForOrderAsync(cart.Id, lines)  :40   ← STOCK DEDUCTION (line items → StockConsumeLine)
  ├─ cart.Place(number)                :43
  ├─ SaveChangesAsync                  :46   ← ORDER COMMITTED (separate commit)
  ├─ OrderConfirmed email (best-effort):48
  └─ CreateShipmentCommand (best-effort) :52-68   ← SHIPMENT CREATED (separate SaveChanges)
        │
        v
SHIPMENT (Pending) ── admin PUT .../shipments/{id}/status──► Ready→Shipped→Delivered
        └─ ShipmentFulfillmentSyncService → RecordOrderShipmentState → Order.ShipmentState
```

**NOTE:** No `OrderPlacementReconciliationJob` exists in the codebase (verified 2026-08-16). Recovery after a mid-placement failure relies on the webhook Hangfire job's `[AutomaticRetry]` (re-sends `CompleteCheckoutForPayment`, which is idempotent via the Draft-guard) — there is no recurring job re-driving placement.

**Assumed diagram vs actual:**
- No `Order=PAID` status. Order statuses are only `Draft/Placed/Canceled/Expired`.
- No invoice entity. "Billing" = the `PaymentCapture` row + order payment timeline.
- No `ShipmentItem` entity. Shipment granularity is order-level.
- No polling — payment success is detected **only** via webhook.

---

## 3. Key Facts Verified From Code

### Entities / fields
- `StockItem.CountOnHand` — physical on-hand (`StockItem.cs:19`). **No `Reserved`/`Available`/`Sold` column** — all derived.
- `StockReservation` — `Quantity`, `State` (`Reserved/Fulfilled/Released/Expired`), `ExpiresAtUtc`, `CartToken`, `OrderId` (nullable), `RowVersion`.
- Available formula (everywhere): `Available = max(CountOnHand − Σ activeReserved, 0)` where active = `State==Reserved && ExpiresAtUtc > now`. Sources: `StockItem.Service.Implementation.cs:164,176,298,349,356,386`; `StockReservation.Service.Implementation.cs:63,142`.

### Checkout flow specifics
- Cart **is** an `Order` with `Status=Draft` (`Order.cs:45`).
- Price **snapshot** at add time: `LineItemMethod.Create(cart.Id, variantId, qty, variant.Price ?? 0)` (`AddToCart.cs:108`). Not re-read at checkout.
- Payment amount always `cart.Total` server-side (`CreatePaymentIntent.cs:105-110`) — never client-supplied.
- Stripe integration: **Checkout Session**, `Mode="payment"`, metadata `order_id`+`payment_id`, idempotency key `"shop-{payment.Number}"` (`StripeGateway.cs:188-228`).
- `ConsumeForOrderAsync` (WIP, 3-arg `(Guid orderId, IReadOnlyCollection<StockConsumeLine> lines, CancellationToken)`): loads all reservations for `CartToken==orderId` (any state), subtracts `Fulfilled` (retry idempotency) and `Reserved`, re-reserves shortfall, then **Picks every `Reserved` row** (`StockReservation.Service.Implementation.cs:285-360`). No transaction, no `StockMovement`.
- Order committed at `CheckoutPlacementService.cs:46` (separate `SaveChanges` from the consume).

### State machines
- **Order status** (`Order.Enumerate.cs`): `Draft → Placed → Canceled`, `Draft → Expired`. No PAID/SHIPPED/DELIVERED statuses.
- **Payment** (`PaymentRecordState`, `PaymentCapture.Enumerate.cs`): `Checkout → Processing → Pending → Completed | Failed | Void`, `Disputed` from most, `Invalid` from Failed/Void. Guards in `PaymentCapture.Method.State.cs` + FluentValidation table `PaymentCapture.Validation.cs:175-194`.
- **Shipment** (`Shipment.Method.State.cs`): `Pending|Backorder → Ready → Shipped → Delivered`, `Pending|Ready|Backorder → Canceled`. `Shipped` requires tracking number.
- Paid/shipping mirrors: `OrderPaymentState.Completed` via `MarkPaymentCompleted` (`Order.Method.Timestamps.cs:22-29`); `ShipmentState` via `RecordOrderShipmentState`. **`MarkPaymentAsPaid()` is dead code.**

### Transaction boundaries
Checkout commits in **four separate `SaveChanges`** — no single transaction:
```
MarkPaymentPaid (Billing)          SaveChanges #1   (MarkPaymentPaid.cs:30)
ConsumeForOrderAsync (Inventory)   SaveChanges #2   (StockReservation.Service.Implementation.cs:358)
cart.Place → order                SaveChanges #3   (CheckoutPlacementService.cs:46)
CreateShipment (Shipping)         SaveChanges #4   (CreateShipment.cs:30)
```
Explicit transactions exist only in `ReserveAsync` (Serializable), `ReserveForVariantAsync` (RepeatableRead), `VoidOrderPayments`.

### Idempotency layering (good)
- Webhook HTTP dedupe on unique `stripe_event_id` (`StripeWebhook.cs:37-47`, `WebhookEventConfiguration.cs:25`) + `23505` race catch.
- Per-payment `ProcessedStripeEventIds` jsonb set — every handler skips processed ids (`ProcessStripeWebhookEventJob.cs:144,195,227,261,286,324,395`).
- Stale-event ordering guard (`RecordStaleEventAsync` :439-450).
- `Complete()` AlreadyCompleted guard; handlers guard `State != Completed`.
- Stripe idempotency keys on all API calls.
- Order placement only for Draft orders (`CompleteCheckoutForPayment.cs:25-26`).
- Stock consume retry-safe: `Fulfilled` counts as consumed (:305-309).
- Refund webhook/admin race: monotonic `ReconcileRefunded` + `RowVersion` retry (`RefundPayment.cs:58-79`).

---

## 4. Findings / Backlog

### 🔴 CRITICAL

| # | Finding | Where | Fix |
|---|---------|-------|-----|
| 1 | **Double stock deduction on every online checkout** — reservations created in both `AddToCart.cs:73-78` and `CreatePaymentIntent.cs:74-90` with the same `CartToken`; `ReserveForVariantAsync` excludes own-cart rows from the availability sum (`StockReservation.Service.Implementation.cs:137-138`) so the second reserve always duplicates; `ConsumeForOrderAsync` then Picks **every** `Reserved` row (:334-356) ignoring `remainingByVariant`. | `AddToCart.cs`, `CreatePaymentIntent.cs`, `StockReservation.Service.Implementation.cs` | One reservation source only, OR consume only up to `remainingByVariant` per variant, OR release AddToCart set before intent-time reservation. |
| 2 | **Stock inflation on release/expiry** — `ReleaseReservationsAsync` (:202-209) and `ExpireReservationsAsync` (:265-272) do `stockItem.CountOnHand += r.Quantity`, but reservation **never decremented** `CountOnHand` (availability is derived). Releasing an abandoned cart adds back qty that was never removed. | `StockReservation.Service.Implementation.cs:202-209,265-272` | Do not mutate `CountOnHand` on release/expiry; `State` change alone frees the stock. |
| 3 | **No stock movement created on sale** — the primary "sold" ledger event is missing; audit trail only exists on cancel/return paths (`StockItem.Service.Implementation.cs:396-405` creates `"sold"` movements but is only invoked for cancellations). | `ConsumeForOrderAsync` (`StockReservation.Service.Implementation.cs:285-360`) | Create a `StockMovement` per picked reservation inside `ConsumeForOrderAsync`. |

### 🟠 HIGH

| # | Finding | Where |
|---|---------|-------|
| 4 | **Reservation expiry sweep is never scheduled** — `ReservationExpiryJobScheduler` exists but is **not registered** via `AddHostedService` (`Inventory.Extension.cs` registers only services + seeders). Expired reservations stay `Reserved` forever → abandoned-cart stock permanently unavailable. | `Inventory.Extension.cs`, `Backgrounds/ReservationExpiryJob.Scheduler.cs` |
| 5 | **`checkout.session.expired` release is a no-op** — calls `ReleaseReservationsAsync(orderId: payment.OrderId)` (`ProcessStripeWebhookEventJob.cs:418`) but cart reservations have `OrderId = null` (only `CartToken` set, `StockReservation.Service.Implementation.cs:148-149`). Filter matches nothing → expired sessions don't release stock. | `ProcessStripeWebhookEventJob.cs:418` |
| 6 | **Stock-deduction and order-placement are not atomic** — two separate `SaveChanges` (`ConsumeForOrderAsync:358` then `CheckoutPlacementService.cs:46`). Crash between them strands deducted stock on a `Draft` order; recovery depends on the webhook Hangfire retry re-sending `CompleteCheckoutForPayment` (idempotent via Draft-guard). No recurring reconciliation job exists. | `CheckoutPlacementService.cs`, `StockReservation.Service.Implementation.cs` |

### 🟡 MEDIUM

| # | Finding | Where |
|---|---------|-------|
| 7 | No invoice entity — "billing" is only the `PaymentCapture` row; `NotificationUseCase.InvoiceIssued` template unreferenced. | `Shared/Operational/Notifications/Store/NotificationStore.Template.cs:389-399` |
| 8 | Stale cart prices — price snapshot never refreshed at checkout; order/payment totals can drift from current catalog prices. | `AddToCart.cs:108` |
| 9 | Duplicate-shipment race — app-level `AnyAsync` guard with no DB unique index on `(OrderId, ShippingMethodId)`. | `CreateShipment.cs:19-23`, `Shipment.Configuration.cs:46-47` |
| 10 | The webhook job (`ProcessStripeWebhookEventJob`) and the storefront checkout (`CreateOrderFromCart`) can both call `PlaceAsync` concurrently — mitigated by Draft-guard and Fulfilled-idempotency, but no explicit `DbUpdateConcurrencyException` handling. | `ProcessStripeWebhookEventJob.cs`, `CompleteCheckoutForPayment.cs` |

### Missing validations (risks)
- Payment amount vs order total **not re-validated** at webhook time.
- Payment currency vs order currency **never cross-checked** (payment currency is always the constant `USD` default).
- No reservation ↔ line-item consistency check at consume (double reservation goes unnoticed).

### What looks good
- Layered webhook idempotency (unique event id, per-payment processed set, stale-event ordering, state guards, Stripe idempotency keys).
- Domain state machines with real transition guards (Payment, Order, Shipment).
- `ConsumeForOrderAsync` WIP retry-idempotency (`Fulfilled` counted as consumed) and re-reserve resilience are genuinely well-designed — they just get defeated by the duplicate-reservation bug.
- Reconciliation job as a safety net for the async money-moved path.
- No module leaks into Shipping for inventory; stock is consumed at placement, not at shipment.

---

## 5. Final Business Flow (simplified)

```
CUSTOMER
  │
  v
CART ─────────────── AddToCart: price snapshot + RESERVE #1
  │
  v
PAYMENT INTENT ───── CreatePaymentIntent: RESERVE #2 (duplicate) → Stripe Checkout Session
  │
  ├── FAILED/EXPIRED ──► payment Failed/Void; release is a NO-OP (orderId=null) → stock stays reserved
  │
  └── SUCCESS ──► Webhook (deduped) ──► payment Completed ──► PlaceAsync
                    │
                    ├── ConsumeForOrderAsync → CountOnHand -= 2×qty  ⚠️ DOUBLE DEDUCTION
                    │     └── NO StockMovement created                ⚠️
                    │
                    ├── Order Draft → Placed   (separate commit)
                    ├── OrderConfirmed email  (best-effort)
                    └── CreateShipment (Pending, best-effort, race-guarded)
                          └── admin marks Ready→Shipped→Delivered → Order.ShipmentState
```

---

## 6. Suggested Fix Order

1. **F1** — eliminate double reservation (single reservation source or consume-limited-to-remaining). Highest business impact.
2. **F2** — stop mutating `CountOnHand` on release/expiry.
3. **F3** — write `StockMovement` on sale.
4. **F4** — register the reservation expiry scheduler.
5. **F5** — fix `checkout.session.expired` release to filter by `CartToken`.
6. **F6** — (later) single transaction around consume+place, or explicit reconciliation handling of `DbUpdateConcurrencyException`.
7. Add a reservation ↔ line-item consistency guard so double-reservation can never silently pass.

---

## 7. Verification Commands (run after fixes)

```bash
dotnet build                                          # warnings-as-errors
dotnet test service/Api/tests/Module.UnitTests        # unit tests
bash scripts/check-feature-conventions.sh             # feature file completeness
```
