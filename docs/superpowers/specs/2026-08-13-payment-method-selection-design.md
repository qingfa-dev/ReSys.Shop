# Payment Method Selection & Hosted Checkout — Design

Date: 2026-08-13
Status: Approved for implementation
Modules: Billing, Ordering, Inventory (stock reservations reused), Store SPA

## Problem

The storefront checkout forces a single embedded Stripe card form and silently
picks the first active payment method. There is no user choice of payment method,
no Stripe hosted checkout, and no cash-on-delivery path. The Stripe integration is
also incomplete: `CreatePaymentIntent` builds a PaymentIntent with
`ConfirmationMethod = manual` but never confirms it, so the card path fails on
`requires_confirmation` and has no 3DS/redirect handling. Order placement
(`CreateOrderFromCart`) hard-requires `payment.State == Completed`, so an offline
method cannot place an order.

## Goal

Let the customer pick a payment method at checkout:

- **Credit Card (Stripe)** — redirect to Stripe Checkout (hosted page); Stripe
  collects details, handles 3DS/wallets, and a webhook auto-places the order.
- **Cash on Delivery** — a non-gateway method; the payment is created `Pending`
  and the order is still placed via the explicit Place Order action.

## Non-Goals

- Saved payment methods / SetupIntent flow (unchanged, not in scope).
- Stripe tax, shipping rates, and multi-currency beyond the existing `Currency`.
- Bank Transfer storefront availability (stays `DisplayOn.Backend`).
- Admin SPA changes (payment-method CRUD already supports provider key / display).

## Approach (chosen)

Approach A from brainstorming: **Stripe Checkout Session** for cards, an
**offline provider key** (`cash_on_delivery`) for COD, and **webhook-driven order
placement** via a new cross-module Ordering command.

---

## Design

### 1. Domain & data model

- Add `GatewayConstants.Providers.CashOnDelivery = "cash_on_delivery"`.
- Seed a `PaymentMethod` "Cash on Delivery" in `PaymentMethodSeeder`
  (`providerKey: cash_on_delivery`, `displayOn: Frontend`, `autoCapture: false`).
- `ListPaymentMethods` filters `DisplayOn != Backend` so the storefront list only
  exposes customer-facing methods (Credit Card, Cash on Delivery).
- Add nullable `CheckoutUrl` to `PaymentCapture` (new EF migration). This lets the
  hosted Stripe URL persist and flow through the existing `MapToStoreDetail`
  mapping that `CreatePaymentIntent` already uses as its return path.
- No new `PaymentRecordState` values; reuse the existing lifecycle:
  - **Stripe:** `Process()` (Checkout → Processing) at intent creation, then
    `Complete()` (Processing → Completed) on the webhook.
  - **COD:** `Process()` then `Pend()` (→ Pending) at intent creation, no gateway.

### 2. Gateway abstraction

- Add `CreateCheckoutSessionAsync(decimal amount, GatewayOptions options,
  CancellationToken ct)` to `IPaymentGatewayActionProvider` and the abstract
  `Gateway`. Returns `PaymentGatewayResponse` with `Authorization = session.Id`
  and a new `CheckoutUrl` field (added to `PaymentGatewayResponse`).
- `StripeGateway` implements it via `SessionService`:
  - `Mode = payment`, `CustomerEmail = options.Customer`, single line item of
    `amount` + `currency`, metadata `order_id`/`payment_id`,
    `CancelUrl = options.CancelUrl`.
  - `SuccessUrl` is `{ReturnUrl}?order={OrderId}` (the backend appends the cart
    id as a query param). Stripe appends `session_id={CHECKOUT_SESSION_ID}` on
    top of it automatically; the SPA ignores `session_id` and keys off `order`
    so it can poll our own status endpoint (see §6).
- `BogusGateway` returns a fake session (`cs_fake_...` id + a fake URL) so the
  demo/test path stays gateway-free.

### 3. CreatePaymentIntent branching

`CreatePaymentIntent.CommandHandler` branches on the loaded method's provider key.
Both branches set `payment.ProviderKey = paymentMethod.ProviderKey` (so offline
detection never relies on the `PaymentMethod` navigation, which is
`SetNull`-deletable):

- **`cash_on_delivery`:** create `PaymentCapture` (Checkout), `Process()`,
  `Pend()`, `SourceId`/`SourceType` null, no `ResponseCode`; reserve stock;
  advance cart to `Payment`; return detail (`state = Pending`).
- **`stripe`:** create Checkout Session via the new gateway method; set
  `ResponseCode = session.Id`, `CheckoutUrl = session.Url`; `Process()`; reserve
  stock; advance cart to `Payment`; return detail with `CheckoutUrl`.

`CreatePaymentIntent.Request` gains `CancelUrl`; `GatewayOptions.CancelUrl` is
populated. `StorePaymentDetailResponse` and `MapToStoreDetail` gain `CheckoutUrl`.

### 4. Webhook & auto-place order

- `StripeWebhookDispatcher.SupportedEventTypes` adds
  `checkout.session.completed` and `checkout.session.expired`.
- `ProcessStripeWebhookEventJob`:
  - `checkout.session.completed` → find `PaymentCapture` by
    `ResponseCode == session.Id`; `Complete()` (idempotent, skip if already
    Completed); then send a new Ordering command `CompleteCheckoutForPayment`
    via `ISender`.
  - `checkout.session.expired` → find payment; `Void()` and release stock
    reservations (via `IStockReservationService`), matching the existing
    compensate-on-failure pattern.
- Because `ResponseCode` now stores the Checkout Session id (`cs_...`), the
  existing `payment_intent.succeeded` handler (which looks up
  `ResponseCode == intent.Id`, i.e. `pi_...`) no longer matches Checkout
  sessions and no-ops. `checkout.session.completed` is the single completion
  source for the card flow; the legacy `payment_intent.*` handlers remain for
  the (now-unused) direct-intent path.
- New Ordering command `CompleteCheckoutForPayment { CartId, PaymentIntentId }`
  reuses the `CreateOrderFromCart` core (consume stock, advance to Confirm, place
  order, notify) without requiring `ICurrentUser`. This keeps Billing→Ordering
  communication on `ISender` (allowed), with no module reference.

### 5. Order placement gating

- **Payment identifier:** the "payment intent id" passed between the SPA and
  `CreateOrderFromCart` becomes the `PaymentCapture.Id` (Guid), not the gateway
  `ResponseCode`. `GetPaymentForCheckout` and `MarkPaymentPaid` match on
  `Id == PaymentIntentId` (with `ResponseCode` kept as a secondary OR-key for
  webhook-style lookups). This is required because COD payments have a null
  `ResponseCode`. The webhook still correlates Stripe events by `ResponseCode`.
- `PaymentForCheckoutResponse` gains `State` and `IsOffline`.
- `GetPaymentForCheckout` populates both; `IsOffline` is derived from
  `PaymentCapture.ProviderKey == GatewayConstants.Providers.CashOnDelivery`
  (set at intent creation), not from the `PaymentMethod` navigation.
- `CreateOrderFromCart` allows placement when
  `IsCompleted || (State == Pending && IsOffline)`.
- **COD stays Pending:** for an offline payment, `CreateOrderFromCart` skips the
  `MarkPaymentPaid` call so the capture remains `Pending` (cash collected later
  via the existing admin capture flow). For gateway payments the webhook already
  marked it `Completed`, so `MarkPaymentPaid` is only invoked for that path
  (effectively a no-op / skipped).

### 6. Store SPA

- `CheckoutView.vue` step 3 renders a radio list from `getPaymentMethods()`
  instead of auto-picking the first method and forcing a card form.
  - **Card selected:** "Pay with card" → `createPaymentIntent(..., { returnUrl:
    origin + '/checkout/return', cancelUrl: origin + '/checkout' })`; on success
    `window.location.href = result.value.checkoutUrl`.
  - **COD selected:** `createPaymentIntent(...)` (no token), then advance to
    Review and use the existing Place Order action.
- New route `/checkout/return` (ordering feature). The cart store is in-memory
  only, so it is empty after the Stripe redirect; the page reads `order` from the
  query string and polls the existing `GetPaymentStatus` endpoint
  (`GET api/storefront/cart/payment/intent/{orderId}`) until `IsCompleted`, then
  fetches the placed order (via `listOrders`) and shows the confirmation. A
  timeout + manual "View My Orders" link covers the delayed-webhook case; the
  `cancel` path returns to the payment step.
- `useCheckout.createPaymentIntent` sets `paymentIntentId = result.value.id`
  (always the `PaymentCapture.Id`) and returns the full detail so the view can
  read `checkoutUrl` and `state`.

---

## Data flow (summary)

```
pick method ──┬─ COD:  create-intent → PaymentCapture Pending → Place Order (explicit)
              └─ Card: create-intent → Checkout Session → redirect to Stripe
                        → pay → success_url → /checkout/return (poll)
                        → webhook checkout.session.completed
                        → PaymentCapture Completed → CompleteCheckoutForPayment
                        → order auto-placed → confirmation
```

## Error handling

- Offline path with no gateway: no gateway errors possible; stock-reservation
  failure still releases reservations (existing compensate).
- Checkout Session create failure → release reservations, no PaymentCapture
  persisted (existing `ProcessAsync` failure branch adapted).
- `checkout.session.expired` → void + release reservations.
- `checkout.session.completed` arriving for an already-placed order is idempotent
  (state guard + `ProcessedStripeEventIds`); `CompleteCheckoutForPayment` must
  also be idempotent (a placed/Placed order is a no-op).
- Return page polls `GetPaymentStatus`; the webhook may be delayed, so the SPA
  treats "not yet completed" as a retry (poll), not an error.

## Testing

- **Unit (Module.UnitTests):** offline branch creates Pending capture with no
  gateway call and sets `ProviderKey`; Stripe branch maps `CheckoutUrl`/`ResponseCode`
  and appends `order` to `SuccessUrl`; `GetPaymentForCheckout`
  exposes `State`/`IsOffline`; `CreateOrderFromCart` allows Pending+offline
  (and skips `MarkPaymentPaid` for offline) while still rejecting
  Pending+gateway; webhook job completes payment and sends the
  Ordering command idempotently; `checkout.session.expired` voids + releases stock.
- **Integration (Api.Tests):** create-intent for COD returns `Pending` without a
  gateway; card path returns a `checkoutUrl` (Bogus fake session).
- **Store SPA:** `CheckoutView` renders method list; COD path places order;
  `/checkout/return` polls to confirmation.
