# Wire Disconnected Backend Design Spec

## Summary

Hook up 6 dead API functions to frontend components. Connect inventory, shipping, and error handling to actual UI.

## Findings to Address

### 1. Inventory Availability

**File:** `features/inventory/services/availabilityApi.ts:8`

`checkAvailability()` — shows per-variant stock across locations.

**Wire to:** `ProductDetailView.vue`

**Design:**
- On product load, call `checkAvailability(variantId)` for selected variant
- Display stock per location below stock badge
- Update when variant changes

### 2. Cart Stock Reservation

**File:** `features/inventory/services/cartReservationApi.ts:9`

`reserveStock()` — reserves inventory when item added to cart.

**Wire to:** `cartStore.ts`

**Design:**
- After `addItem()` succeeds, call `reserveStock({ variantId, quantity, cartToken })`
- Store reservation ID on cart line item
- On page load, call `getCartReservations()` to sync state

### 3. Release Reservation

**File:** `features/inventory/services/cartReservationApi.ts:16`

`releaseReservation()` — frees reserved stock when item removed.

**Wire to:** `cartStore.ts`

**Design:**
- Before `removeItem()` API call, call `releaseReservation(reservationId)`
- On `clearCart()`, release all reservations

### 4. Get Cart Reservations

**File:** `features/inventory/services/cartReservationApi.ts:22`

`getCartReservations()` — fetches active reservation status.

**Wire to:** `cartStore.ts`

**Design:**
- On `fetchCart()` success, call `getCartReservations()` to sync reservation state

### 5. Shipping Rates with Delivery Range

**File:** `features/shipping/services/shippingApi.ts:23`

`getShippingRates()` — returns rates with `deliveryRange` field.

**Wire to:** `CheckoutStepDelivery.vue`

**Design:**
- After selecting shipping method, call `getShippingRates()` filtered by method
- Display `deliveryRange` below each rate option
- Fixes the previously removed `as any` cast

### 6. Centralized Error Handler

**File:** `shared/composables/useApiErrorHandler.ts:11`

`useApiErrorHandler` — handles errors with toast notifications.

**Wire to:** All views with ad-hoc error handling.

**Design:**
- Import in each view
- Replace manual `try/catch` + `notify.error()` with `handleError(error)`

## Verification

- [ ] Stock availability shown on product detail
- [ ] Cart reservations created on add, released on remove
- [ ] Shipping rates show delivery range
- [ ] Error toasts consistent across all views
- [ ] All 257 unit tests pass
