# Storefront API Fixes + Checkout Wiring

**Date**: 2026-08-05
**Scope**: Fix API route mismatches, wire checkout to real backend services, add Terms/Privacy routes
**Depends on**: Nothing (independent of Spec A and Spec B)
**Status**: Approved

## Goal

Fix 4 known API route mismatches in `shared/constants/api.ts`. Replace
checkout placeholder data with real API calls to address, shipping, and
payment services. Add Terms and Privacy route entries. Verify
request/response shapes match backend types.

## Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Route fix approach | Split single-param functions into typed dual functions | Backend has separate routes for Guid vs ISO code params |
| Profile route fix | Remove double `profiles` path segment | Backend route is bare `/api/store/profiles` |
| Checkout wiring strategy | Direct API calls in step components, orchestrated by checkoutStore | Follows existing pattern; step components fetch data, store manages state transitions |
| Stripe integration | Reuse existing `usePayment` composable | Already has Stripe.js init, Elements mount/unmount, error handling |
| Address step | Use user's real address book from addressApi | Backend endpoint exists, SPA types already defined |

## Batch 1 — Route Mismatch Fixes

### File: `src/shared/constants/api.ts`

All fixes are surgical changes to specific lines. Other 79 constants unchanged.

### Fix 1: Profile double segment (HIGH severity)

**Line 69**:
```
profiles: `${API_STORE}/profiles/profiles`,
```
Change to:
```
profiles: `${API_STORE}/profiles`,
```

Backend route: `GET /api/store/profiles` (not `/api/store/profiles/profiles`).
The existing comment on line 67-68 incorrectly states the double path is
correct. Remove that comment.

**Impact**: `profileApi.ts` services use `ENDPOINTS.profiles` for GET and PUT.
After fix, these calls will reach the correct backend route instead of 404.

### Fix 2: Location country/states idOrIso (HIGH severity)

**Lines 80-82**:
```
countryByIdOrIso: (idOrIso: string) => `${API_STORE}/locations/countries/${idOrIso}`,
states: `${API_STORE}/locations/states`,
stateByIdOrIso: (idOrIso: string) => `${API_STORE}/locations/states/${idOrIso}`,
```
Change to:
```
countryById: (id: string) => `${API_STORE}/locations/countries/${id}`,
countryByIso: (iso: string) => `${API_STORE}/locations/countries/by-iso/${iso}`,
states: `${API_STORE}/locations/states`,
stateById: (id: string) => `${API_STORE}/locations/states/${id}`,
stateByIso: (iso: string) => `${API_STORE}/locations/states/by-iso/${iso}`,
```

Backend has separate routes:
- `GET /api/store/locations/countries/{id:guid}` — Guid-only constraint
- `GET /api/store/locations/countries/by-iso/{isoCode}` — string ISO code
- Same pattern for states

**Call site update**: `countryApi.ts` functions like `getCountryByIdOrIso`
must be updated to use the new names. Currently only used in:
- `src/features/location/services/countryApi.ts`
- `src/features/location/composables/useLocationCascade.ts`

Search for all call sites and update before commit.

### Fix 3: Remove dead sessionById route (LOW severity)

**Line 40**:
```
sessionById: (id: string) => `${API_STORE}/identity/auth/sessions/${id}`,
```
Change to:
```
// sessionById: Backend route not yet available — use GET /sessions for full list
```

No backend route exists for `GET /api/store/identity/auth/sessions/{id}`.
Single session info is not exposed by the API.

### Fix 4: Remove dead taxonomies list route (LOW severity)

**Line 12**:
```
taxonomies: `${API_STOREFRONT}/taxonomies`,
```
Change to:
```
// taxonomies: No backend GET /api/storefront/taxonomies list endpoint
```

Only `GET /api/storefront/taxonomies/{id:guid}` exists (for tree loading).

## Batch 2 — Checkout Real API Wiring

### File 1: `src/features/ordering/components/CheckoutStepAddress.vue`

**Current**: 2 hardcoded GUID addresses in `addressOptions` array.
**Target**: User's real address book from `GET /api/store/profiles/addresses`.

**New dependencies**:
```ts
import { getAddresses } from '@/features/profile/services/addressApi'
import type { Address } from '@/features/profile/types/address'
import AddressCard from '@/features/profile/components/AddressCard.vue'
import AddressForm from '@/features/profile/components/AddressForm.vue'
```

**Flow**:
1. `onMounted`: call `getAddresses()` -> populate reactive `addresses` ref
2. Render address list with `<AddressCard>` for each address + radio selection
3. Selected address ID stored in local `selectedAddressId` ref
4. "Continue" button -> `checkoutStore.saveAddress(selectedAddressId, email)` -> on success -> `goToStep(2)`
5. "Add New Address" button -> shows inline `<AddressForm>`. On save -> refresh list, auto-select new

**States**:
- Loading: `<Skeleton>` rows (3 cards), skeleton lines for address text
- Error: `<Message severity="error">` with retry button
- Empty: `<EmptyState>` with "No saved addresses. Add one to continue." + link to `/account/addresses`

**Rendering each address**: Reuse the profile module's `AddressCard` component.
Wrap each card in a radio-button selectable container. Selected state shown
with teal border + teal-50 background.

### File 2: `src/features/ordering/components/CheckoutStepDelivery.vue`

**Current**: 2 hardcoded GUID shipping methods in `shippingOptions` array.
**Target**: Real shipping methods from `GET /api/storefront/shipping/methods`.

**New dependencies**:
```ts
import { getShippingMethods } from '@/features/shipping/services/shippingApi'
import type { ShippingMethod } from '@/features/shipping/types/shipping'
```

**Flow**:
1. `onMounted`: call `getShippingMethods()` -> populate options
2. Auto-select if single method returned
3. Each option: radio + method name + carrier + estimated delivery days + formatted price
4. "Continue" button -> `checkoutStore.calculateShipping(methodId)` -> on success -> `goToStep(3)`

**States**: Loading (3 skeleton rows), error (Message + retry), empty (Message "No shipping methods available for your location").

**Type verification**: The `ShippingMethod` type in `features/shipping/types/shipping.ts`
may not have `carrier` or `estimatedDays`. Verify against actual API response
shape before wiring. If type missing fields, extend the interface.

### File 3: `src/features/ordering/components/CheckoutStepPayment.vue`

**Current**: 2 hardcoded payment method IDs + no Stripe Elements.
**Target**: Real payment methods + Stripe Card Element + payment confirmation.

**New dependencies**:
```ts
import { getPaymentMethods } from '@/features/payment/services/paymentApi'
import { usePayment } from '@/features/payment/composables/usePayment'
import type { PaymentMethod } from '@/features/payment/types/payment'
```

**Flow**:
1. `onMounted`: call `getPaymentMethods()` -> populate method list
2. User selects method -> call `checkoutStore.createPaymentIntent(methodId, cart.subtotal)` -> receives `clientSecret`
3. Mount Stripe Card Element: `usePayment().mount(clientSecret, containerRef.value)`
4. "Pay" button -> `stripe.confirmCardPayment(clientSecret)` -> on confirmed -> `paymentApi.confirmPayment(paymentIntentId)` -> on success -> `goToStep(4)`

**Stripe Card Element container**:
```vue
<div ref="cardContainer" class="mt-4 p-4 border border-stone-200 rounded-lg min-h-[40px]" />
```

Mounted by `usePayment().mount()` after clientSecret received. Unmounted on
component unmount via `onUnmounted(() => usePayment().unmount())`.

**Error handling**: Card declined -> show error message below card field.
3D Secure -> Stripe handles redirect automatically. Network failure ->
show "Payment processing failed. Please try again."

**Stripe publishable key**: Read from `import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY`.
Add to `.env.development` if missing:
```
VITE_STRIPE_PUBLISHABLE_KEY=pk_test_placeholder
```

Fallback: if key not configured, show error "Payment processing is not
configured" instead of card element.

### File 4: `src/features/ordering/stores/checkoutStore.ts`

**Current**: `saveAddress`, `calculateShipping`, `createPaymentIntent` already
call the correct backend endpoints via `put(ENDPOINTS.cart, ...)`,
`post(ENDPOINTS.cartShippingRate, ...)`, `post(ENDPOINTS.paymentCreateIntent, ...)`.

**Changes needed**:
1. Verify `saveAddress` body shape matches backend's `OrderParameters`:
   - Current sends: `{ shipAddressId, currency, email }`
   - Backend expects: `{ currency, email?, specialInstructions?, billAddressId?, shipAddressId?, shippingMethodId? }`
   - All fields except `currency` are optional -> current body is valid. No change needed.

2. Verify `calculateShipping` body shape matches `SelectShippingRate.Request`:
   - Current sends: `{ shippingMethodId }`
   - Backend expects: `{ shippingMethodId }` -> matches. No change needed.

3. Verify `createPaymentIntent` body shape matches `CreatePaymentIntent.Request`:
   - Current sends: `{ amount, currency, paymentMethodId }`
   - Backend expects `StorePaymentRequest`: `{ amount, currency, orderId?, paymentMethodId, state?, paymentStatus?, returnUrl?, paymentMethodToken?, cardNumber? }`
   - `orderId` may be required by the backend handler. If so, add `orderId: cart.id` where cart is imported from `useCartStore`.

4. Add error state propagation: each action already sets `error.value` on
   failure. Ensure step components display this error via:
   ```vue
   <Message v-if="checkout.error" severity="error" class="mb-4">{{ checkout.error }}</Message>
   ```

### File 5: `src/features/ordering/types/checkout.ts`

Verify that `CreatePaymentIntent.Request` type matches backend response shape.
Expected response: `{ id: string, clientSecret: string, responseCode: string }`.
If the current type is a generic `Result<unknown>`, add the proper interface.

## Batch 3 — Legal Pages Routing (same files as Spec B Batch 6)

### File: `src/features/catalog/routes/index.ts`

Add route entries for Terms and Privacy views:
```ts
{
  path: '/terms',
  name: 'terms',
  component: () => import('../views/TermsView.vue'),
  meta: { title: 'Terms of Service' },
},
{
  path: '/privacy',
  name: 'privacy',
  component: () => import('../views/PrivacyView.vue'),
  meta: { title: 'Privacy Policy' },
},
```

These routes are registered under `DefaultLayout` (the `catalogRoutes`
array is used as children of the DefaultLayout shell).

**Note**: The view files (`TermsView.vue`, `PrivacyView.vue`) are created by
Spec B Batch 6. Spec C only adds the route entries. If Spec C runs first,
create placeholder view files:
```vue
<!-- TermsView.vue -->
<template>
  <div class="max-w-3xl mx-auto px-4 py-16">
    <h1>Terms of Service</h1>
    <!-- Placeholder legal content -->
  </div>
</template>
```

## Batch 4 — Request/Response Shape Verification

No file changes. Manual verification against backend:

1. Send HTTP request to `GET /api/store/profiles` -> verify response matches `ProfileDetailResponse`
2. Send `GET /api/store/locations/countries/by-iso/VN` -> verify ISO route returns country
3. Send `GET /api/storefront/shipping/methods` -> verify `ShippingMethod[]` shape with carrier/estimatedDays fields
4. Send `GET /api/storefront/payment/methods` -> verify `PaymentMethod[]` shape
5. Send `POST /api/storefront/payment/create-intent` -> verify `clientSecret` field in response
6. Send `GET /api/store/profiles/addresses` -> verify `Address[]` shape with all fields used in AddressCard

For each mismatch between API response and TypeScript type:
- If type is missing fields API returns: add to type interface
- If type has fields API doesn't return: mark as optional in type
- If type has wrong names: rename to match backend (camelCase transform handles snake_case)

## File Inventory

### Modified Files (8)

| File | Change |
|------|--------|
| `src/shared/constants/api.ts` | Fix 4 route mismatches (lines 12, 40, 69, 80-82) |
| `src/features/ordering/components/CheckoutStepAddress.vue` | Rewrite to use real addressApi |
| `src/features/ordering/components/CheckoutStepDelivery.vue` | Rewrite to use real shippingApi |
| `src/features/ordering/components/CheckoutStepPayment.vue` | Rewrite to use real paymentApi + Stripe Elements |
| `src/features/ordering/stores/checkoutStore.ts` | Verify body shapes, add error propagation |
| `src/features/location/services/countryApi.ts` | Update imports to use new `countryById`/`countryByIso` names |
| `src/features/location/composables/useLocationCascade.ts` | Update imports to use new names |
| `src/features/catalog/routes/index.ts` | Add /terms and /privacy routes |

### New Files (2 — only if Spec B hasn't run)

| File | Change |
|------|--------|
| `src/features/catalog/views/TermsView.vue` | Placeholder if Spec B not yet done |
| `src/features/catalog/views/PrivacyView.vue` | Placeholder if Spec B not yet done |

## Risk Matrix

| Risk | Impact | Mitigation |
|------|--------|------------|
| Payment intent requires `orderId` field not currently sent | High — checkout payment broken | Verify via ApiTests HTTP file before wiring; add `orderId: cart.id` if needed |
| ShippingMethod type doesn't match API response shape | Medium | Verify type against actual API response; extend interface |
| Stripe publishable key not configured in env | Medium — payment step shows error | Add to `.env.development` + fallback error message |
| `countryByIdOrIso` call sites not all found -> type-check failure | Medium | Run `rg "countryByIdOrIso|stateByIdOrIso" src/` before commit |
| Profile double-segment fix changes behavior -> other code relied on broken URL | Low | Only `profileApi.ts` uses `ENDPOINTS.profiles` |
| Address API returns 401 in checkout (requires specific auth) | Low | Checkout route already `requiresAuth: true` |
| Stripe Elements mount fails on mobile | Low | PrimeVue Dialog handles mobile viewport |
| Placeholder Terms/Privacy views show low-quality placeholder text | Low | Acceptable — legal content comes from business team |

## Verification

1. `pnpm run type-check` — 0 errors after all fixes
2. `rg "countryByIdOrIso|stateByIdOrIso" src/` -> 0 results (all call sites updated)
3. `rg "profiles/profiles" src/` -> 0 results (double segment removed)
4. `pnpm run lint` — 0 violations
5. `GET /api/store/profiles` -> returns profile data (200, not 404)
6. `GET /api/store/locations/countries/by-iso/VN` -> returns Vietnam country data
7. Checkout flow:
   - Address step shows real addresses from user's address book
   - Delivery step shows real shipping methods with carrier + days
   - Payment step shows Stripe card field after method selection
   - Enter test card `4242 4242 4242 4242` with future expiry + any CVC -> payment succeeds -> advances to Confirm step
8. `/terms` -> renders TermsView content (not 404)
9. `/privacy` -> renders PrivacyView content (not 404)
10. `pnpm run test:unit -- --run` -> existing tests still pass

## Out of Scope

- Backend changes (Stripe webhook HMAC, payment capture, order state transitions — covered in `2026-08-04-stripe-enablement-storefront-correction-design.md`)
- Creating new backend endpoints (sessionById, taxonomies list, country list without paging)
- Admin dashboard checkout management
- Order fulfillment / shipment tracking
- Payment method CRUD (saving cards for future use — `setup-intent` exists in API but not wired in UI)

## Related Specs

- **Spec A**: `2026-08-05-storefront-design-system-design.md` — design tokens foundation
- **Spec B**: `2026-08-05-storefront-feature-restoration-design.md` — feature restoration (creates TermsView + PrivacyView)
- **Backend**: `2026-08-04-stripe-enablement-storefront-correction-design.md` — Stripe enablement, CheckoutState, cross-module fixes
- **API contract**: `2026-08-04-storefront-api-integration-handoff.md` — API response shapes, route prefixes
