# Storefront Type/Service/Store Audit Fix — Design Spec

## Problem
The Storefront Vue SPA has 2 critical bugs, 10 type hygiene issues, 2 store defects, 3 convention violations, 12 unused endpoint constants, and missing frontend services for existing backend endpoints. A DELETE reservation endpoint is also missing from the backend.

## Scope
Full-stack fix: frontend types/services/stores/views + new backend endpoints.

---

## Phase 1: Types & Constants

### 1.1 Fix `shared/constants/api.ts`
- **Add** `availability: (variantId: string) => \`${API_STOREFRONT}/availability/${variantId}\``
- **Rename** `search` → `searchByImage` (value: `api/storefront/products/images/search`)
- **Add** `addressDefault` constant: `api/store/profiles/addresses/default`
- **Keep** unused constants (`emailsChange`, `emailsConfirm`, `emailsResend`, `shippingCalculate`, `countryById`, `countryByIso`, `stateById`, `stateByIso`, `authLoginExternal`, `sessionsRefresh`, `images`) — they will be consumed by new services in Phase 2

### 1.2 Move inline types to type files
- `VisualSearchModel` from `catalog/services/searchByImageApi.ts` → `catalog/types/searchByImage.ts`
- `OrderTrackingResponse` from `ordering/services/orderApi.ts` → `ordering/types/order.ts`

### 1.3 Fix `Result<unknown>` return types
All 8 occurrences → `Result<void>`:
- `identity/services/authApi.ts`: `register()`, `forgotPassword()`, `resetPassword()`
- `ordering/services/checkoutApi.ts`: `updateCheckout()`, `selectShippingRate()`, `validateCheckout()`
- `identity/services/sessionApi.ts`: `revokeCurrentDevice()`, `revokeAll()`

### 1.4 Fix broken return type
- `profile/services/addressApi.ts`: `getDefaultAddress()` → `Promise<Result<Address | null>>`

---

## Phase 2: Services

### 2.1 Fix broken endpoint references
- `inventory/services/availabilityApi.ts`: `ENDPOINTS.availability(variantId)`
- `catalog/services/searchByImageApi.ts`: `ENDPOINTS.searchByImage`
- `shared/api/interceptors/refresh.ts`: use `ENDPOINTS.sessionsRefresh` instead of hardcoded string

### 2.2 New service files
- `identity/services/emailApi.ts`: `changeEmail()`, `confirmEmail()`, `resendVerification()`
- `profile/services/accountApi.ts`: `deleteProfile()`

### 2.3 Route views through service layer
- `catalog/views/HomeView.vue`: replace raw `getPaged()` with `productApi.getPagedProducts()`
- `catalog/views/ShopView.vue`: use `productApi.getPagedProducts` as URL provider for `usePagedQuery`

---

## Phase 3: Stores

### 3.1 Fix `checkoutStore.confirmPayment()`
Wrap in try/catch, set `error.value` on failure, match pattern of other actions.

### 3.2 Fix wishlist N+1
`fetchWishlistedIds()` and `toggleWishlist()` currently fetch each wishlist detail individually. Fix by using the paged list response which already includes variant IDs, or batch the requests.

### 3.3 New store actions
- `authStore`: `changeEmail(email)`, `confirmEmail(token)`, `resendVerification()`
- `profileStore`: `deleteProfile()`

---

## Phase 4: Backend

### 4.1 New endpoint: DELETE reservation
`DELETE /api/storefront/cart/reserve/{reservationId}` — release a single stock reservation.
- Feature folder: `Module/Inventory/Features/Storefront/ReleaseSingleReservation/`
- Command: `ReleaseSingleReservationCommand`
- Handler: releases one reservation by ID (vs `ReleaseCartStockReservations` which releases all for a cart)

Note: The email endpoints (`POST /api/store/identity/emails/change`, `/confirm`, `/resend`) and `DELETE /api/store/profiles` already exist on the backend. No new backend work needed for those.

---

## File Change Summary

| Category | Files Changed | Files Added |
|----------|--------------|-------------|
| Constants | 1 | 0 |
| Types | 2 | 0 |
| Services | 6 | 2 |
| Stores | 3 | 0 |
| Views | 2 | 0 |
| Backend | 0 | 3-4 |
| **Total** | **14** | **5-6** |
