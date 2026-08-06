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

**Backend:** Endpoint does NOT exist yet. Need to create it.

**New endpoint:** `GET /api/store/profiles/addresses/default`

**Handler:** Query user's addresses, return the one with `IsDefault == true`. Fallback to first address if no default set.

**Backend files to create:**
- `Module/Profile/Features/Storefront/Addresses/GetDefault/GetDefaultAddress.cs`
- `Module/Profile/Features/Storefront/Addresses/GetDefault/GetDefaultAddress.Endpoint.cs`
- `Module/Profile/Features/Storefront/Addresses/GetDefault/GetDefaultAddress.Response.cs`

**Frontend file:** `app/Store/src/features/ordering/components/CheckoutStepAddress.vue`

**Changes:**
- On mount, call `GET /api/store/profiles/addresses/default`
- Auto-select returned address
- Fallback to first address from list if endpoint returns empty

### 3. POST /passwords/change — Change Password

**Purpose:** Enable password change from account settings.

**Covered by Gap 11 (Change Password).** This wiring is part of that spec.

**Backend:** Already exists at `POST /api/store/identity/passwords/change`

### 4. GET /products/related — Related Products

**Purpose:** Show taxon-based related products on product detail.

**Covered by Gap 5 (Related Products).** This wiring is part of that spec.

**Backend:** Already exists at `GET /api/storefront/products/related`

### 5. GET /shipping/rates — Delivery Estimate

**Purpose:** Show estimated delivery date per shipping rate.

**Backend:** No separate endpoint needed. The existing `GET /api/storefront/shipping/rates` response already includes `DeliveryRange` per rate.

**File:** `app/Store/src/features/ordering/components/CheckoutStepDelivery.vue`

**Changes:**
- Read `deliveryRange` from each shipping rate in the existing list response
- Display "Est. delivery: Aug 15-17" next to each rate option
- No additional API call needed

## Files to Create/Modify

| File | Endpoint |
|------|----------|
| `Module/Profile/Features/Storefront/Addresses/GetDefault/` | CREATE — new default address endpoint |
| `features/ordering/components/CheckoutStepPayment.vue` | setup-intent |
| `features/ordering/components/CheckoutStepAddress.vue` | addresses/default |
| `features/ordering/components/CheckoutStepDelivery.vue` | Read deliveryRange from existing rates response |

## Acceptance Criteria

- [ ] "Save card" checkbox appears in payment step
- [ ] Default address auto-selected in checkout
- [ ] Delivery estimate shown per shipping rate
- [ ] All API calls handle loading and error states
- [ ] No regressions in existing checkout flow
