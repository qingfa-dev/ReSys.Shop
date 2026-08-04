# Stripe.js Frontend Integration Handoff

> **Purpose:** Capture every API contract, token flow, state machine, and known gap the future Stripe.js + Storefront UI will need.
> **Date:** 2026-08-04
> **Status:** Backend-ready, Stripe.js integration pending

---

## 1. Storefront Payment Endpoint Contracts

### 1.1 Create Payment Intent

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `api/storefront/payment/create-intent` |
| **Auth** | Bearer JWT required |
| **Rate Limit** | `payment` policy (see §6) |

**Request body** (`CreatePaymentIntent.Request`):

```json
{
  "orderId": "guid (required)",
  "paymentMethodId": "guid | null (selects the PaymentMethod entity row)",
  "paymentMethodToken": "string | null (Stripe pm_... token)",
  "returnUrl": "string | null (redirect URL for 3DS)",
  "cardNumber": "string | null (Bogus test card, e.g. 4111111111111111)"
}
```

**Response** (`StorePaymentDetailResponse`):

```json
{
  "id": "guid",
  "amount": 29.99,
  "currency": "USD",
  "orderId": "guid",
  "paymentMethodId": "guid",
  "state": "Checkout | Processing | Pending | Completed | Failed | Void | Disputed | Invalid",
  "paymentStatus": "string | null (Stripe intent status, e.g. requires_action)",
  "clientSecret": "string | null (Stripe pi_..._secret_...)",
  "responseCode": "string | null (Stripe PaymentIntent ID)",
  "createdAtUtc": "datetimeoffset",
  "modifiedAtUtc": "datetimeoffset | null"
}
```

**Error codes:**

| Status | Code | Meaning |
|---|---|---|
| 400 | `PaymentCapture.NotFound` | No active payment method found |
| 400 | `PaymentCapture.ProviderNotRegistered` | Gateway not registered |
| 400 | `Order.NotFound` | Order not found or user doesn't own it |
| 400 | Various Stripe errors | Gateway processing failure |

---

### 1.2 Confirm Payment

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `api/storefront/payment/confirm/{paymentId}` |
| **Auth** | Bearer JWT required |
| **Rate Limit** | `payment` policy |

**Request body:** None (paymentId is a route parameter).

**Response** (`StorePaymentDetailResponse` + `Message`):

```json
{
  "id": "guid",
  "amount": 29.99,
  "currency": "USD",
  "orderId": "guid",
  "paymentMethodId": "guid",
  "state": "Completed",
  "clientSecret": "string | null",
  "responseCode": "string | null",
  "createdAtUtc": "datetimeoffset",
  "modifiedAtUtc": "datetimeoffset | null",
  "message": "Payment confirmed."
}
```

**Behavior:** Checks local `PaymentCapture` state. If already `Completed` (webhook beat us), returns immediately. If `Processing` or `Pending`, attempts `Complete()`. Race-safe with webhook.

**Error codes:**

| Status | Code | Meaning |
|---|---|---|
| 400 | `PaymentCapture.NotFound` | Payment or order not found |
| 400 | `PaymentCapture.InvalidStateTransition` | Payment not in `Processing`/`Pending` state |

---

### 1.3 List Payment Methods

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `api/storefront/payment/methods` |
| **Auth** | Bearer JWT required |

**Query parameters:** Standard `QueryingParameters` (pagination, sorting, filtering).

**Response:** Paged list of `StorePaymentMethodListItemResponse` with `Id`, `Amount`, `Currency`, `OrderId`, `PaymentMethodId`, `State`.

---

### 1.4 Create Setup Intent

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `api/storefront/payment/setup-intent` |
| **Auth** | Bearer JWT required |
| **Rate Limit** | None (no `.RequireRateLimiting()` on endpoint) |

**Request body** (`CreateSetupIntent.Request`):

```json
{
  "paymentMethodId": "guid (required)"
}
```

**Response** (`StorePaymentDetailResponse`):

```json
{
  "id": "guid",
  "clientSecret": "string (pi_setup_..._secret_...)",
  "state": "...",
  ...
}
```

**Status:** Currently unused in active flow. Exists for future "save payment method" functionality.

---

### 1.5 Stripe Webhook

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `api/payments/stripe/webhook` |
| **Auth** | HMAC-SHA256 signature validation (no JWT) |
| **Rate Limit** | `webhook` policy |

**Request body:** Raw Stripe event JSON (max 64KB).

**Required header:** `Stripe-Signature` (Stripe-generated HMAC-SHA256 signature).

**Response:** `200 OK` with `"Webhook accepted and queued for processing."` on success.

**Handled events:**
- `payment_intent.succeeded` → marks payment `Completed`
- `payment_intent.payment_failed` → marks payment `Failed`
- `payment_intent.requires_action` → sets `PaymentStatus = "requires_action"`
- `payment_intent.processing` → sets `PaymentStatus = "processing"`
- `payment_intent.canceled` → marks payment `Void`
- `charge.refunded` → records refund
- `charge.dispute.created` → marks payment `Disputed`

**Behavior:** Validates signature, then enqueues a Hangfire background job (`ProcessStripeWebhookEventJob`) for async processing. The webhook endpoint returns immediately — actual state updates happen asynchronously.

---

## 2. Source-Token Semantics

The system supports two gateway modes for payment source identification:

### Stripe Gateway (`paymentMethodToken`)

When the active `PaymentMethod.ProviderKey == "stripe"`:

- **`paymentMethodToken`**: The Stripe PaymentMethod token (`pm_...`) obtained from Stripe.js after `stripe.createPaymentMethod()`.
- **`sourceType`**: Set to `"payment_method"`.
- This token is attached to the PaymentIntent via Stripe's API to charge the card.

### Bogus/Test Gateway (`cardNumber`)

When the active `PaymentMethod.ProviderKey == "bogus"`:

- **`cardNumber`**: Plain text card number for simulation (e.g. `"4111111111111111"` for success, `"4000000000000002"` for decline).
- **`sourceType`**: Set to `"card"`.
- No Stripe.js involvement — purely server-side test flow.

### Selection Logic

The backend selects which field to use based on `PaymentMethod.ProviderKey`:

```
if ProviderKey == "bogus"
    source = cardNumber, sourceType = "card"
else (stripe)
    source = paymentMethodToken, sourceType = "payment_method"
```

The `paymentMethodId` field (a GUID) selects the entity row in the `PaymentMethods` table — it is separate from the Stripe `pm_...` token.

---

## 3. `clientSecret` Flow

### How it works

1. Frontend calls `POST api/storefront/payment/create-intent` with `orderId` and `paymentMethodToken` (Stripe pm_ token).
2. Backend creates a Stripe PaymentIntent via the Stripe API and stores the `clientSecret` on the `PaymentCapture` entity.
3. Backend returns `clientSecret` (format: `pi_..._secret_...`) in the response.
4. Frontend uses this secret with Stripe.js:

```javascript
const stripe = await loadStripe(publishableKey);

const { error, paymentIntent } = await stripe.confirmCardPayment(clientSecret, {
  payment_method: {
    card: cardElement,  // Stripe Elements card input
    billing_details: { name: '...', email: '...' }
  },
  return_url: returnUrl  // for 3DS redirects
});
```

5. If `paymentIntent.status === 'succeeded'`, payment is complete.
6. If `paymentIntent.status === 'requires_action'`, 3DS is needed (see §4).

### Response field mapping

| Field | Source | Description |
|---|---|---|
| `clientSecret` | `PaymentCapture.IntentClientSecret` | Used by Stripe.js `confirmCardPayment()` |
| `responseCode` | `PaymentCapture.ResponseCode` | Stripe PaymentIntent ID (`pi_...`) |
| `paymentStatus` | `PaymentCapture.PaymentStatus` | Stripe intent status string |
| `state` | `PaymentCapture.State` | Local state machine value |

---

## 4. 3DS Redirect Flow

### Trigger

When Stripe requires 3D Secure authentication, the PaymentIntent status becomes `requires_action`.

### Backend fields

- `PaymentCapture.PaymentStatus` is set to `"requires_action"` via webhook (`payment_intent.requires_action` event).
- `ReturnUrl` is passed as `SuccessUrl` in gateway options during intent creation.
- `PaymentCapture.IntentClientSecret` contains the full client secret needed for redirect.

### Frontend flow

1. After `stripe.confirmCardPayment()`, check `paymentIntent.status`:
   - `"succeeded"` → payment complete, proceed to checkout.
   - `"requires_action"` → 3DS redirect needed.

2. Redirect the user to the 3DS authentication URL (provided by Stripe via `paymentIntent.next_action.redirect_to_url.url`).

3. After 3DS completion, Stripe redirects back to `return_url`.

4. On resume, **poll the order status** by calling the order detail endpoint or the confirm endpoint:

```javascript
// After 3DS redirect resume
const response = await fetch(`/api/storefront/payment/confirm/${paymentId}`, {
  method: 'POST',
  headers: { Authorization: `Bearer ${token}` }
});
const { state, paymentStatus } = await response.json();
```

5. **Important:** Capture is asynchronous via webhook. Even after 3DS redirect, the payment may still be `Processing`. The client should poll or use SSE/WebSocket to detect when `state` becomes `Completed`.

### `PaymentRecordState` state machine

```
Checkout → Processing → Pending → Completed
                  ↓         ↓
                Failed    Failed → Invalid
                  ↓
                 Void → Invalid
```

The `requires_action` state is represented via `PaymentStatus` (Stripe's status string), not via `PaymentRecordState`. The local state remains `Processing` while Stripe waits for 3DS.

---

## 5. Checkout Flow (After Phase 2B)

### `CheckoutState` enum

```csharp
public enum CheckoutState
{
    Address,    // 0 — Initial state
    Delivery,   // 1 — Addresses set
    Payment,    // 2 — Shipping method selected
    Confirm,    // 3 — Payment processed
    Complete    // 4 — Order placed
}
```

### State machine rules

Transitions are **strictly forward-only**:

```
Address → Delivery → Payment → Confirm → Complete
```

- Out-of-order transitions return **409 Conflict**.
- Canceled orders cannot advance.
- `Complete` is terminal.

### Transition guards

| From → To | Guard |
|---|---|
| `Address → Delivery` | `BillAddressId != null && ShipAddressId != null` |
| `Delivery → Payment` | `ShippingMethodId != null` |
| `Payment → Confirm` | Payment required (Total > 0) |
| `Confirm → Complete` | Always allowed |
| Any → Any backward | 409 `InvalidCheckoutTransition` |

### Expired reservations

Stock reservations have a 30-minute TTL (`StockReservationExpiryMinutes = 30`). If a reservation expires:

- The order remains in `Draft` status.
- The payment step must be re-executed (a new `CreatePaymentIntent` call is needed).
- The old `PaymentCapture` may be in a terminal state (`Failed`/`Void`) — the frontend should create a fresh intent.

### Checkout prerequisites (validated at `POST api/storefront/cart/checkout`)

```
- Order must be Draft
- CheckoutState >= Confirm
- BillAddressId set
- ShipAddressId set
- ShippingMethodId set
- Email set
- At least one line item
- No discontinued variants
```

---

## 6. Webhook-Driven State Transitions

### Architecture

Stripe webhooks are the **source of truth** for payment state updates. The confirm endpoint (`POST /api/storefront/payment/confirm/{paymentId}`) is a **polling/fallback** mechanism.

### Flow

1. Stripe processes the payment asynchronously (especially 3DS, bank transfers).
2. Stripe sends webhook to `POST api/payments/stripe/webhook`.
3. Backend validates HMAC-SHA256 signature.
4. Backend enqueues `ProcessStripeWebhookEventJob` via Hangfire.
5. Background job processes the event and updates `PaymentCapture.State`.

### Client behavior after 3DS

After a 3DS redirect, the client should **not assume the payment is complete**. Instead:

1. Call `POST /api/storefront/payment/confirm/{paymentId}` to check current state.
2. If `state` is still `Processing` or `Pending`, wait and retry (exponential backoff recommended).
3. If `state` is `Completed`, proceed to `POST api/storefront/cart/checkout`.
4. If `state` is `Failed`, show error and allow retry.

### Duplicate event prevention

`PaymentCapture.ProcessedStripeEventIds` tracks processed Stripe event IDs to prevent duplicate handling of the same webhook.

---

## 7. Rate Limiting

### Payment endpoints

Both `CreatePaymentIntent` and `ConfirmPayment` endpoints use the `payment` rate limiting policy:

```csharp
.RequireRateLimiting("payment")
```

### Configuration

The `payment` policy uses a **fixed window limiter** with IP + user partitioning:

- **Partition key:** IP address + user identity
- **Default config:** `PermitLimit = 1000`, `WindowSeconds = 60` (disabled by default)
- **Production target:** 30 requests per minute per user (configurable via `appsettings.json`)

```json
{
  "RateLimit": {
    "Enabled": true,
    "Policies": {
      "payment": {
        "PermitLimit": 30,
        "WindowSeconds": 60
      }
    }
  }
}
```

### Client behavior on 429

When the client receives HTTP 429:

1. Read the `Retry-After` header (if present).
2. Back off exponentially: `1s → 2s → 4s → 8s → 16s` (max).
3. Do not retry immediately — the fixed window resets at the window boundary.
4. Show a user-friendly "Too many requests, please try again" message after 3+ failures.

### Webhook rate limiting

The webhook endpoint uses a separate `webhook` policy (IP-only partitioning, higher limits).

---

## 8. Known Gaps

### No Stripe.js in the Storefront today

The frontend has **no Stripe.js integration**. The `clientSecret` is returned by the backend but never consumed by the UI. The storefront currently bypasses the Stripe.js flow entirely — payments are either mocked (Bogus) or processed server-side without client-side confirmation.

**Impact:** Real Stripe payments with 3DS cannot work until Stripe.js is integrated in the Store SPA.

### `confirm` endpoint unused in active flow

The `POST api/storefront/payment/confirm/{paymentId}` endpoint exists but is **not called** in the current storefront flow. The `CreateOrderFromCart` handler validates payment by checking `PaymentCapture.ResponseCode` and `State` directly, without going through the confirm endpoint.

**Impact:** No functional issue today, but the confirm endpoint becomes important for 3DS polling once Stripe.js is integrated.

### `setup-intent` endpoint unused

The `POST api/storefront/payment/setup-intent` endpoint is fully implemented but has **no callers**. It creates a Stripe SetupIntent for saving payment methods for future use.

**Impact:** "Save this card for next time" feature requires this endpoint + Stripe.js `stripe.confirmCardSetup()`.

### Save-payment-method UI is stubbed

The storefront has no UI for saving or managing payment methods. The `ListPaymentMethods` endpoint exists but there's no create/delete/replace UI.

**Impact:** All payment methods must be pre-configured in the database. Users cannot add cards at checkout.

### Transaction repository is dead code

No `TransactionRepository` or `ITransactionRepository` was found in the Payment module. Payment state is managed directly through the `PaymentCapture` entity and EF Core `DbContext`. If any transaction repository pattern was planned, it was never implemented.

**Impact:** None — the current direct EF Core approach is simpler and works correctly.

### Other gaps

- **No SSE/WebSocket for payment status updates** — client must poll after 3DS.
- **No payment retry flow** — if payment fails, user must restart checkout.
- **No idempotency key on client side** — backend uses `shop-{paymentNumber}` as idempotency key, but client has no way to pass its own.
- **`PaymentMethodToken` validation is minimal** — no format validation for `pm_...` prefix on the Stripe path.

---

## Appendix: Quick Reference for Frontend Developers

### Stripe.js initialization

```javascript
import { loadStripe } from '@stripe/stripe-js';
const stripe = await loadStripe('pk_live_...');
```

### Payment flow sequence

```
1. GET  /api/storefront/payment/methods       → pick payment method
2. POST /api/storefront/payment/create-intent  → get clientSecret
3. stripe.confirmCardPayment(clientSecret)     → handle 3DS if needed
4. POST /api/storefront/payment/confirm/{id}   → poll until Completed
5. POST /api/storefront/cart/checkout           → place order
```

### Stripe test cards

| Card | Behavior |
|---|---|
| `4242424242424242` | Success (Bogus gateway) |
| `4000000000000002` | Decline (Bogus gateway) |
| `4000000000009995` | Insufficient funds (Bogus gateway) |

For real Stripe test cards, see [Stripe testing docs](https://docs.stripe.com/testing).
