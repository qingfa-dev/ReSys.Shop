# Backend Endpoint Wiring

## Summary

Wire 5 backend endpoints that exist but aren't consumed by the new Store frontend.

## Endpoints to Wire

### 1. POST /payment/setup-intent — Save Card

**Purpose:** Allow users to save payment method for future purchases.

**File:** `app/Store/src/features/ordering/components/CheckoutStepPayment.vue`

**Changes:**
- Add "Save this card for future purchases" checkbox below Stripe card element
- When checked, call `createSetupIntent({ paymentMethodId })` after payment success
- Store returned SetupIntent client secret for Stripe confirmation

**Backend:** Already exists at `POST /api/storefront/payment/setup-intent`

### 2. GET /profiles/addresses/default — Default Address

**Purpose:** Pre-select default address in checkout.

**File:** `app/Store/src/features/ordering/components/CheckoutStepAddress.vue`

**Changes:**
- On mount, fetch addresses via `getAddresses()`
- Auto-select address where `isDefault === true`
- Fallback to first address if no default set

**Backend:** Already exists at `GET /api/store/profiles/addresses/default`

### 3. POST /passwords/change — Change Password

**Purpose:** Enable password change from account settings.

**Covered by Gap 11 (Change Password).** This wiring is part of that spec.

**Backend:** Already exists at `POST /api/store/identity/passwords/change`

### 4. GET /products/related — Related Products

**Purpose:** Show taxon-based related products on product detail.

**Covered by Gap 5 (Related Products).** This wiring is part of that spec.

**Backend:** Already exists at `GET /api/storefront/products/related`

### 5. GET /shipping/rates/{id}/delivery — Delivery Estimate

**Purpose:** Show estimated delivery date per shipping rate.

**File:** `app/Store/src/features/ordering/components/CheckoutStepDelivery.vue`

**Changes:**
- For each shipping rate, fetch delivery estimate via `getDeliveryEstimate(rateId)`
- Display "Est. delivery: Aug 15" next to each rate option
- Show loading skeleton while fetching

**Backend:** Already exists at `GET /api/storefront/shipping/rates/{id}/delivery`

## Files to Modify

| File | Endpoint |
|------|----------|
| `features/ordering/components/CheckoutStepPayment.vue` | setup-intent |
| `features/ordering/components/CheckoutStepAddress.vue` | addresses/default |
| `features/ordering/components/CheckoutStepDelivery.vue` | shipping/rates/{id}/delivery |
| `features/shipping/services/shippingApi.ts` | Add `getDeliveryEstimate` function |

## Acceptance Criteria

- [ ] "Save card" checkbox appears in payment step
- [ ] Default address auto-selected in checkout
- [ ] Delivery estimate shown per shipping rate
- [ ] All API calls handle loading and error states
- [ ] No regressions in existing checkout flow
