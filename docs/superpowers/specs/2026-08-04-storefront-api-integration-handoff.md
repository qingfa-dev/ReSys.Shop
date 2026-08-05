# Storefront API Integration Handoff

Document for the Store SPA (`app/Store/`) and legacy Storefront (`app/legacy/Storefront/`)
teams. Covers API contract changes introduced by the Stripe enablement and storefront
correction plan (2026-08-04).

## What Changed

### Phase 1 — Stripe Payment Gateway

**SourceId type migration** (`string?` from `Guid?`)
Any frontend code that reads/writes `SourceId` on a payment record must treat it as a
nullable string, not a GUID. Stripe IDs are strings (`pi_...`, `pm_...`); legacy GUIDs
were Bogus test data.

**CreatePaymentIntent endpoint**
- `POST /api/storefront/payment/create-intent`
- Request body now includes `CardNumber` (string?) for Bogus test card path.
- Response includes `ClientSecret` for Stripe.js Elements or Payments (`pi_...`/`seti_...`).
- Gateway selection is automatic — no `GatewayName` field needed in request.
- Stripe is co-enabled with Bogus in Development; Bogus uses `ProviderKey=CashOnDelivery`
  and path `Bogus`, Stripe uses `ProviderKey=Stripe` and path `Stripe`.

**ConfirmPayment endpoint**
- `POST /api/storefront/payment/confirm/{paymentId:guid}`
- Requires `PaymentIntentId` in request body (returned from `create-intent`).

**Currency honoring**
- Gateway currency field is now an instance property defaulting to `usd`.
- No action needed from frontend — the backend reads its own config.

**Webhook**
- `POST /api/storefront/webhooks/stripe`
- HMAC signature verified; idempotent processing (event ID dedup).
- Frontend should NOT call this — Stripe sends webhooks directly.

### Phase 2A — Mechanical Conformance

No user-facing API changes. `CartReservations.Release` endpoint added
(`POST /api/storefront/cart/release`) for cart idle timeout — called by
backend timers, not frontend.

### Phase 2B — Checkout Flow Restructure (BREAKING)

**CheckoutState machine: Address → Delivery → Payment → Confirm → Complete**

The order of operations is now enforced server-side:

1. **Address** — call `PUT /api/storefront/cart` with `CheckoutState=Address` + email, billing/shipping address IDs.
2. **Delivery** — call `POST /api/storefront/cart/shipping-rate` with `CheckoutState=Delivery` + selected `ShippingRateId`.
3. **Payment** — call `POST /api/storefront/payment/create-intent` with `CheckoutState=Payment` + `CartToken`.
4. **Confirm** — call `POST /api/storefront/payment/confirm/{id}` OR `POST /api/storefront/cart/checkout` with `CheckoutState=Confirm`.
5. **Complete** — order placed; no further client action.

Skipping steps returns `405 InvalidCheckoutTransition`. The frontend must track
the current `CheckoutState` from the cart response and advance step-by-step.

**Stock reservation is coupled with payment intent creation**
- Creating a payment intent (`create-intent`) now reserves stock via
  `POST /api/storefront/cart/reserve` internally (backend-only).
- If stock is insufficient, the payment intent is voided (E3 flow) and an error is returned.
- Frontend should handle `OutOfStock` errors on `create-intent`.

**Cart validation**
- `POST /api/storefront/cart/validate` validates the current checkout state.
- Use before proceeding to the next step to catch stale/broken state.

### Phase 3A — Profile Module Correction

**Route prefix: `api/store/profiles`** (unchanged)

**Directory rename:** Backend feature files moved from `Features/Store/` to
`Features/Storefront/` (AGENTS rule 3). No frontend impact — route prefixes
did not change.

**Route defect fixed:** Profiling endpoints previously served under
`/api/store/profiles/profiles/...`. Now correctly served under
`/api/store/profiles/...`. Frontend code that targets the doubled-segment
path will break and must be updated.

**CreateProfile endpoint added:**
- `POST /api/store/profiles`
- Body: `{ FirstName, LastName, DateOfBirth?, Gender?, PhoneNumber?, AvatarUrl? }`
- Previously, profiles were only created implicitly during registration.
  This endpoint enables explicit profile creation for existing accounts.

**Address CountryCode/StateCode FK validation:**
- `POST/PUT /api/store/profiles/addresses`
- `CountryCode` and `StateCode` are now validated against the Location database
  via MediatR queries. Invalid codes return `404 LocationNotFound`.
- Frontend should use Location endpoints to look up valid ISO codes:
  - `GET /api/store/locations/countries/by-iso/{isoCode}`
  - `GET /api/store/locations/states/by-iso/{isoCode}`

### Cross-Module Reference Cleanup

All cross-module `using Module.X.Domain.*` references that bridged Payment ↔
Ordering ↔ Inventory are replaced with `Shared.Application.Contracts` MediatR
commands/queries. This is a backend-only refactor with no API impact but
ensures the modules remain decoupled for future microservice extraction.

## Endpoint Reference

All 73 storefront endpoints are documented in the demo `.http` files under
`ApiTests/`:

| Module | .http file |
|--------|-----------|
| Catalog | `ApiTests/Catalog/` |
| Identity | `ApiTests/Identity/` |
| Inventory | `ApiTests/Inventory/` |
| Location | `ApiTests/Location/` |
| Ordering | `ApiTests/Ordering/demo-flow.http` (9-step UC-STR-CHK) |
| Payment | `ApiTests/Payment/` (5 files: methods, create-intent, confirm, setup-intent, webhook) |
| Profile | `ApiTests/Profile/` |
| Shipping | `ApiTests/Shipping/` |

### Key Route Prefixes

| Module | Prefix |
|--------|--------|
| Catalog | `api/storefront` |
| Identity | `api/store/identity` |
| Inventory | `api/storefront` |
| Location | `api/store/locations` |
| Ordering | `api/storefront` |
| Payment | `api/storefront` |
| Profile | `api/store/profiles` |
| Shipping | `api/storefront` |

### Breaking Changes for Frontend

1. **CheckoutState step ordering** — frontend must respect the state machine
   (Address→Delivery→Payment→Confirm→Complete). Skipping steps is rejected.
2. **Profile route** — `/api/store/profiles/profiles/...` no longer works;
   use `/api/store/profiles/...` (single `profiles` segment).
3. **SourceId is now a string** — any Payment display logic that parses SourceId
   as GUID must handle string values.
4. **CreatePaymentIntent request** — now includes `CardNumber` for Bogus demo;
   Stripe flow ignores it. Old request shape without `CardNumber` still works
   (it's optional).

## Frontend Integration Steps (Store SPA)

1. Update profile API route in the Store SPA HTTP client
   (`/api/store/profiles/profiles` → `/api/store/profiles`).
2. Wire the checkout state machine: track `CheckoutState` from cart response,
   advance through Address→Delivery→Payment→Confirm steps.
3. Update `create-intent` call to include `CartToken` from cart state +
   `CheckoutState=Payment`.
4. Handle `OutOfStock` error from `create-intent` (show "insufficient stock"
   toast, send user back to cart to adjust quantities).
5. After `confirm` returns success, transition to order confirmation page.
6. Use Location endpoints for address country/state ISO code lookup before
   creating addresses.

## Verification

Run all `ApiTests/**/*.http` files against a running instance. The
`ApiTests/Ordering/demo-flow.http` covers the full UC-STR-CHK flow:
create cart → add item → set address → select shipping → create intent
→ confirm → webhook simulation → order creation.
