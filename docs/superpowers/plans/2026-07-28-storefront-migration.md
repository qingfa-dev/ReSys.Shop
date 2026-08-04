# Storefront Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Migrate `app/legacy/shop/` into `app/Storefront/` with API paths adapted to new `/api/storefront/*` and `/api/store/*` endpoints, preserving PrimeVue components, styling, mock data, and tests.

**Architecture:** Feature modules copied 1:1 from legacy into `app/Storefront/src/features/`, with core infrastructure (Axios client, Result models, repositories) ported to `core/`. Each feature gets its API paths updated to new endpoints. Cart token generated via `crypto.randomUUID()` for `X-Cart-Token` header. Vite proxy routes `/api` to backend on port 5035.

**Tech Stack:** Vue 3, Pinia, Axios, PrimeVue, Zod, Vitest, TypeScript

## Global Constraints

- All legacy API paths must be mapped to new endpoints per the mapping table in the spec
- Axios interceptors for auth (Bearer token) and cart token (`X-Cart-Token`) must be operational before any feature task
- `USE_MOCK` toggle preserved in every service for offline development
- Cart token generated via `crypto.randomUUID()` on first add-to-cart
- WishlistButton component removed from catalog
- MFA removed from identity
- Coupon/promotion endpoints removed
- Build must pass (`vue-tsc --noEmit && vitest run`)
- **ZERO visual changes to any .vue file.** Every component's `<template>`, `<style>`, PrimeVue props, SCSS classes, and PrimeVue theme config must be copied verbatim. Only `.ts` data-layer files (stores, repositories, services) may be modified. If a file contains both `<template>`/`<style>` and `<script>` sections, edit only the `<script>` — never the template or style.
- PrimeVue version must match legacy — do not upgrade as part of this migration
- All global style imports and asset references must be ported identically

---

### Task 0: Core Infrastructure — Port core/, app/, router, delete stubs

**Files:**
- Create: `app/Storefront/src/core/services/api.ts`
- Create: `app/Storefront/src/core/services/toast.ts`
- Create: `app/Storefront/src/core/http/axios/axios.client.ts`
- Create: `app/Storefront/src/core/interceptors/request.interceptor.ts`
- Create: `app/Storefront/src/core/interceptors/response.interceptor.ts`
- Create: `app/Storefront/src/core/models/result.ts`
- Create: `app/Storefront/src/core/models/resultHelpers.ts`
- Create: `app/Storefront/src/core/repositories/IRepository.ts`
- Create: `app/Storefront/src/core/repositories/BaseRepository.ts`
- Create: `app/Storefront/src/core/mappers/response-mapper.ts`
- Create: `app/Storefront/src/core/helpers/query.builder.ts`
- Create: `app/Storefront/src/core/helpers/mock-query.helper.ts`
- Create: `app/Storefront/src/core/utils/result.ts`
- Create: `app/Storefront/src/app/router/index.ts`
- Create: `app/Storefront/src/app/stores/ui.ts`
- Create: `app/Storefront/src/app/stores/preferences.ts`
- Create: `app/Storefront/src/app/layouts/DefaultLayout.vue`
- Create: `app/Storefront/src/app/layouts/components/AppHeader.vue`
- Create: `app/Storefront/src/app/layouts/components/AppFooter.vue`
- Create: `app/Storefront/src/app/layouts/components/MobileNav.vue`
- Modify: `app/Storefront/vite.config.ts`
- Modify: `app/Storefront/src/main.ts`
- Modify: `app/Storefront/src/App.vue`
- Delete: `app/Storefront/src/api.ts`
- Delete: `app/Storefront/src/views/` (all 5)
- Delete: `app/Storefront/src/stores/` (both)

- [ ] **Step 1: Copy core/ files from legacy verbatim**

Do NOT edit any file during the copy. The `api.ts` edits come in Step 2.

```bash
# Copy core infrastructure files
cp app/legacy/shop/src/core/services/api.ts app/Storefront/src/core/services/api.ts
cp app/legacy/shop/src/core/services/toast.ts app/Storefront/src/core/services/toast.ts
cp -r app/legacy/shop/src/core/http/* app/Storefront/src/core/http/
cp app/legacy/shop/src/core/models/result.ts app/Storefront/src/core/models/result.ts
cp app/legacy/shop/src/core/models/resultHelpers.ts app/Storefront/src/core/models/
cp -r app/legacy/shop/src/core/repositories/* app/Storefront/src/core/repositories/
cp app/legacy/shop/src/core/mappers/response-mapper.ts app/Storefront/src/core/mappers/
cp app/legacy/shop/src/core/helpers/query.builder.ts app/Storefront/src/core/helpers/
cp app/legacy/shop/src/core/helpers/mock-query.helper.ts app/Storefront/src/core/helpers/
cp app/legacy/shop/src/core/utils/result.ts app/Storefront/src/core/utils/
```

Create any missing directories first with `mkdir -p`.

- [ ] **Step 2: Update api.ts — auth refresh endpoint + cart token**

Edit `app/Storefront/src/core/services/api.ts`:

```diff
- const refreshUrl = `${API_BASE_URL}/identity/auth/refresh`
+ const refreshUrl = `${API_BASE_URL}/api/store/identity/auth/sessions/refresh`
```

Add cart token injection in the request interceptor, after the Bearer token block:

```typescript
// X-Cart-Token for guest cart identification
const cartToken = localStorage.getItem('cartToken')
if (cartToken && config.headers) {
  config.headers['X-Cart-Token'] = cartToken
}
```

Also update the refresh endpoint in the interceptor (the `axios.post` call inside the 401 handler):

```diff
- const { data } = await axios.post(`${API_BASE_URL}/identity/auth/refresh`, {
+ const { data } = await axios.post(`${API_BASE_URL}/api/store/identity/auth/sessions/refresh`, {
```

- [ ] **Step 3: Copy app/ files from legacy verbatim**

Layout `.vue` files and components must be copied as-is — no template or style edits.

```bash
# Copy router
mkdir -p app/Storefront/src/app
cp -r app/legacy/shop/src/app/router app/Storefront/src/app/router
cp -r app/legacy/shop/src/app/stores app/Storefront/src/app/stores
cp -r app/legacy/shop/src/app/layouts app/Storefront/src/app/layouts
cp -r app/legacy/shop/src/app/composables app/Storefront/src/app/composables
```

- [ ] **Step 4: Update router lazy imports**

The legacy router uses `@/features/...` paths which will resolve once features are ported. No changes needed now if `@` alias points to `src/`.

Verify `app/Storefront/src/main.ts` imports the router from the correct path.

- [ ] **Step 5: Update vite.config.ts**

Ensure these exist in `vite.config.ts`:

```typescript
resolve: {
  alias: {
    '@': fileURLToPath(new URL('./src', import.meta.url)),
  },
},
server: {
  proxy: {
    '/api': {
      target: process.env.VITE_API_URL || 'http://localhost:5035',
      changeOrigin: true,
    },
  },
},
```

- [ ] **Step 6: Update main.ts — preserve PrimeVue theme exactly**

Port the bootstrap logic from `app/legacy/shop/src/main.ts`. The PrimeVue theme preset, plugins, and global style imports must match the legacy version exactly:

- Import PrimeVue + theme + ripple config (copy verbatim from legacy)
- Import ToastService and any other PrimeVue plugins
- Import Pinia
- Import and use router
- Import global styles (SCSS files, asset paths — same relative paths as legacy)

**Do not change the PrimeVue import pattern, theme preset, or plugin registration order.**

- [ ] **Step 7: Update App.vue**

Replace `app/Storefront/src/App.vue` content with ported `app/legacy/shop/src/App.vue` (the one that wraps `<router-view>` in `DefaultLayout`).

- [ ] **Step 8: Delete stale files**

```bash
rm -f app/Storefront/src/api.ts
rm -rf app/Storefront/src/views/
rm -rf app/Storefront/src/stores/
```

- [ ] **Step 9: Update package.json — add dependencies from legacy**

Merge the legacy `package.json` dependencies into `app/Storefront/package.json`. Required additions:
- `axios`
- `primevue`, `@primevue/themes`, `@primevue/auto-import-resolver`
- `unplugin-vue-components`
- `@iconify/vue` or similar icon library used by legacy
- Any PrimeVue dependencies from legacy

- [ ] **Step 10: Install dependencies and verify build**

```bash
cd app/Storefront && pnpm install
npx vue-tsc --noEmit
```

Build may fail because feature files don't exist yet — that's expected at this point.

- [ ] **Step 11: Commit**

```bash
git add -A app/Storefront/
git commit -m "feat(storefront): port core infrastructure, router, layouts from legacy shop"
```

---

### Task 1: Catalog Module

**Files:**
- Create: All files under `app/Storefront/src/features/catalog/` from legacy `app/legacy/shop/src/features/catalog/`
- Modify: API repository paths in `features/catalog/repositories/`

**Interfaces:**
- Consumes: `core/services/api.ts` (Axios client), `core/repositories/BaseRepository`, `core/models/result.ts`
- Produces: Catalog views, store, components for the router and layout

- [ ] **Step 1: Copy catalog feature from legacy verbatim**

```bash
cp -r app/legacy/shop/src/features/catalog app/Storefront/src/features/catalog
```

- [ ] **Step 1a: Verify no .vue files were modified**

Run `git diff --stat app/Storefront/src/features/catalog/` and confirm only `.ts` files show changes. If any `.vue` file appears in the diff (besides deleting WishlistButton.vue), revert the `.vue` change — templates and styles must be preserved as-is.

- [ ] **Step 2: Remove WishlistButton**

```bash
rm -f app/Storefront/src/features/catalog/components/WishlistButton.vue
```

Also remove any imports of `WishlistButton` from catalog components. Use `grep -r "WishlistButton" app/Storefront/src/features/catalog/` to find and remove references.

- [ ] **Step 3: Update API repository paths**

In `app/Storefront/src/features/catalog/repositories/`, find all API path strings and replace per the mapping:

```diff
- '/api/products'
+ '/api/storefront/products'

- '/api/categories'
+ '/api/storefront/taxonomies/'
```

The `ProductApiRepository` and `CategoryApiRepository` files contain the actual paths (likely using `this.get()`, `this.getPaged()`, etc. with path arguments). Find every API path literal and update it.

Specific changes likely needed:
- `ProductApiRepository`: `/api/products` → `/api/storefront/products`, `/api/products/{id}` → `/api/storefront/products/{slug}`
- `CategoryApiRepository`: `/api/categories` → `/api/storefront/taxonomies/{id}`, `/api/categories/slug/{slug}` → `/api/storefront/taxons/{id}/products`

- [ ] **Step 4: Commit**

```bash
git add -A app/Storefront/src/features/catalog/
git commit -m "feat(storefront): port catalog module with updated API paths"
```

---

### Task 2: Identity Module

**Files:**
- Create: All files under `app/Storefront/src/features/identity/` from legacy
- Modify: Auth repository paths, drop MFA

- [ ] **Step 1: Copy identity feature from legacy verbatim**

```bash
cp -r app/legacy/shop/src/features/identity app/Storefront/src/features/identity
```

- [ ] **Step 1a: Verify no .vue files modified**

```bash
git diff --stat app/Storefront/src/features/identity/ | grep '\.vue'
```
If any `.vue` file appears in the diff (not related to MFA removal), revert it. Templates and styles are read-only.

- [ ] **Step 2: Drop MFA from auth store**

Remove MFA-related methods from `app/Storefront/src/features/identity/store/auth.ts`:
- Remove `enableMfa()`, `verifyMfa()`, `disableMfa()`, `mfaQrCode`, `mfaEnabled` and any MFA-related state
- Remove MFA-related API calls from the auth repository interface and implementations

- [ ] **Step 3: Update auth repository paths**

In `app/Storefront/src/features/identity/repositories/`, update paths:

```diff
- '/identity/auth/login'
+ '/api/store/identity/auth/login/password'

- '/identity/auth/register'
+ '/api/store/identity/auth/register'

- '/identity/auth/logout'
+ '/api/store/identity/auth/logout'

- '/identity/auth/refresh'
+ '/api/store/identity/auth/sessions/refresh'

- '/identity/auth/forgot-password'
+ '/api/store/identity/passwords/forgot'

- '/identity/auth/change-password'
+ '/api/store/identity/passwords/change'
```

If there is a user repository with `/identity/users/{id}`, drop it — use profile API instead (port in a later task).

- [ ] **Step 4: Commit**

```bash
git add -A app/Storefront/src/features/identity/
git commit -m "feat(storefront): port identity module with updated auth paths, drop MFA"
```

---

### Task 3: Ordering Module (Cart, Orders, Checkout)

**Files:**
- Create: All files under `app/Storefront/src/features/ordering/` from legacy
- Modify: Cart API paths, add cart token flow, drop coupons, update checkout flow

- [ ] **Step 1: Copy ordering feature from legacy verbatim**

```bash
cp -r app/legacy/shop/src/features/ordering app/Storefront/src/features/ordering
```

- [ ] **Step 1a: Verify no .vue files modified**

```bash
git diff --stat app/Storefront/src/features/ordering/ | grep '\.vue'
```
If any `.vue` appears in the diff, revert it immediately. Only `.ts` store/repository files may be edited.

- [ ] **Step 2: Add cart token generation to cart store**

In `app/Storefront/src/features/ordering/store/cart.ts` (or wherever the legacy cart store lives), add cart token generation at the top of the store:

```typescript
const CART_TOKEN_KEY = 'cartToken'

function ensureCartToken(): string {
  let token = localStorage.getItem(CART_TOKEN_KEY)
  if (!token) {
    token = crypto.randomUUID()
    localStorage.setItem(CART_TOKEN_KEY, token)
  }
  return token
}
```

Call `ensureCartToken()` on any cart mutation (addItem, createCart, etc.) to guarantee the token exists before API calls.

- [ ] **Step 3: Update cart API paths**

In cart repository files, update:

```diff
- '/ordering/cart'
+ '/api/storefront/cart'

- '/ordering/cart/items'
+ '/api/storefront/cart/items'

- '/ordering/cart/items/'
+ '/api/storefront/cart/items/'
```

- [ ] **Step 4: Drop coupon endpoints**

Remove any coupon-related methods from the cart repository (apply coupon, remove coupon). Remove coupon-related store actions if they exist.

- [ ] **Step 5: Update checkout flow**

The legacy checkout likely calls `POST /ordering/orders/checkout`. Update to the new multi-step flow:

```typescript
// Legacy: single call
await api.post('/ordering/orders/checkout', body)

// New multi-step:
await api.post('/api/storefront/cart/checkout')  // create order from cart
await api.post('/api/storefront/cart/validate')    // validate state
await api.post('/api/storefront/cart/shipping-rate', { rateId })  // select shipping
await api.post('/api/storefront/payment/create-intent', { orderId })  // payment
```

If the legacy checkout is a single composable/function, you may need to refactor it into steps. At minimum, update the final checkout call path.

- [ ] **Step 6: Update order API paths**

```diff
- '/ordering/orders'
+ '/api/storefront/orders'
```

- [ ] **Step 7: Commit**

```bash
git add -A app/Storefront/src/features/ordering/
git commit -m "feat(storefront): port ordering module - cart, orders, checkout with cart token flow"
```

---

### Task 4: Payment Module

**Files:**
- Create: All files under `app/Storefront/src/features/payment/` from legacy
- Modify: API paths

- [ ] **Step 1: Copy payment feature from legacy verbatim**

```bash
cp -r app/legacy/shop/src/features/payment app/Storefront/src/features/payment
```

- [ ] **Step 1a: Verify no .vue files modified**

```bash
git diff --stat app/Storefront/src/features/payment/ | grep '\.vue'
```
If any `.vue` appears, revert it. Only `.ts` files may be edited.

- [ ] **Step 2: Update payment API paths**

```diff
- '/ordering/payment-methods'
+ '/api/storefront/payment/methods'

- '/payment/intents'
+ '/api/storefront/payment/create-intent'

- '/payment/intents/'
+ '/api/storefront/payment/confirm/'

- '/payment/intents/{id}/cancel'
# Drop — not in new API
```

- [ ] **Step 3: Drop refund-related code**

Remove transaction refund methods if they exist in the payment repository and service.

- [ ] **Step 4: Commit**

```bash
git add -A app/Storefront/src/features/payment/
git commit -m "feat(storefront): port payment module with updated API paths"
```

---

### Task 5: Shipping Module

**Files:**
- Create: All files under `app/Storefront/src/features/shipping/` from legacy
- Modify: API paths, drop shipment tracking

- [ ] **Step 1: Copy shipping feature from legacy verbatim**

```bash
cp -r app/legacy/shop/src/features/shipping app/Storefront/src/features/shipping
```

- [ ] **Step 1a: Verify no .vue files modified**

```bash
git diff --stat app/Storefront/src/features/shipping/ | grep '\.vue'
```
If any `.vue` appears, revert it.

- [ ] **Step 2: Update shipping API paths**

```diff
- '/ordering/shipping-methods'
+ '/api/storefront/shipping/methods'

- '/shipping/rates'
+ '/api/storefront/shipping/rates'

- '/shipping/rates/{id}/calculate'
+ '/api/storefront/shipping/calculate'
```

- [ ] **Step 3: Drop shipment tracking**

Remove any shipment tracking endpoints (`/shipping/shipments`, `/shipping/orders/{id}/shipments`) from repositories and services. Remove related store state and UI if any.

- [ ] **Step 4: Commit**

```bash
git add -A app/Storefront/src/features/shipping/
git commit -m "feat(storefront): port shipping module with updated API paths"
```

---

### Task 6: Locations Module (Addresses)

**Files:**
- Create: All files under `app/Storefront/src/features/locations/` from legacy
- Modify: API paths from `/locations` to `/api/store/profiles/addresses`, add countries/states

- [ ] **Step 1: Copy locations feature from legacy verbatim**

```bash
cp -r app/legacy/shop/src/features/locations app/Storefront/src/features/locations
```

- [ ] **Step 1a: Verify no .vue files modified**

```bash
git diff --stat app/Storefront/src/features/locations/ | grep '\.vue'
```
If any `.vue` appears, revert it.

- [ ] **Step 2: Update address API paths**

```diff
- '/locations'
+ '/api/store/profiles/addresses'

- '/locations/default'
# Drop — filter on client side by isDefault flag
```

- [ ] **Step 3: Drop store locations**

Remove `StoreLocation` repository, service, and mock data (`/locations/stores` — not in new API).

- [ ] **Step 4: Add countries/states repository**

This is new — no legacy equivalent. Create a lightweight repository for the countries/states reference data endpoints:

`app/Storefront/src/features/locations/repositories/ILocationReferenceRepository.ts`:
```typescript
import type { Result, PagedResult } from '@/core/models/result'

export interface Country {
  id: string
  name: string
  isoCode: string
}

export interface State {
  id: string
  name: string
  isoCode: string
  countryId: string
}

export interface ILocationReferenceRepository {
  getCountries(page?: number, pageSize?: number): Promise<PagedResult<Country>>
  getCountryById(id: string): Promise<Result<Country>>
  getCountryByIso(isoCode: string): Promise<Result<Country>>
  getStates(countryId?: string, page?: number, pageSize?: number): Promise<PagedResult<State>>
}
```

Create mock data for countries (at least a few common ones: US, CA, GB, AU, etc.) with their states.

- [ ] **Step 5: Commit**

```bash
git add -A app/Storefront/src/features/locations/
git commit -m "feat(storefront): port locations module - addresses, countries/states reference data"
```

---

### Task 7: Inventory Module

**Files:**
- Create: All files under `app/Storefront/src/features/inventory/` from legacy
- Modify: API paths, add cart reservation

- [ ] **Step 1: Copy inventory feature from legacy**

```bash
cp -r app/legacy/shop/src/features/inventory app/Storefront/src/features/inventory
```

- [ ] **Step 2: Update stock status API paths**

```diff
- '/inventory/{id}/stock-status'
+ '/api/storefront/availability/{variantId}'
```

- [ ] **Step 3: Add cart reservation endpoints**

The new API has `POST /api/storefront/cart/reserve` and `GET /api/storefront/cart/reserve`. Add these to the inventory repository:

```typescript
reserveStock(variantId: string, quantity: number, cartToken: string): Promise<Result<Reservation>>
getReservations(cartToken: string): Promise<Result<Reservation[]>>
```

Create a `Reservation` interface and mock data.

- [ ] **Step 4: Commit**

```bash
git add -A app/Storefront/src/features/inventory/
git commit -m "feat(storefront): port inventory module with cart reservation support"
```

---

### Task 8: Search Module

**Files:**
- Create: All files under `app/Storefront/src/features/search/` from legacy

- [ ] **Step 1: Copy search feature from legacy**

```bash
cp -r app/legacy/shop/src/features/search app/Storefront/src/features/search
```

- [ ] **Step 2: Update search API path**

In search repository, update:

```diff
- '/api/products/search'
+ '/api/storefront/products?search='
```

The query builder from `core/helpers/query.builder.ts` should handle the parameter construction.

- [ ] **Step 3: Commit**

```bash
git add -A app/Storefront/src/features/search/
git commit -m "feat(storefront): port search module with updated API path"
```

---

### Task 9: Reviews Module

**Files:**
- Create: All files under `app/Storefront/src/features/reviews/` from legacy

The legacy reviews module is mock-only (no API repository). Port verbatim — no path changes needed.

- [ ] **Step 1: Copy reviews feature from legacy**

```bash
cp -r app/legacy/shop/src/features/reviews app/Storefront/src/features/reviews
```

- [ ] **Step 2: Commit**

```bash
git add -A app/Storefront/src/features/reviews/
git commit -m "feat(storefront): port reviews module (mock-only)"
```

---

### Task 10: Recommendations Module

**Files:**
- Create: All files under `app/Storefront/src/features/recommendations/` from legacy
- Modify: Add new API endpoints

- [ ] **Step 1: Copy recommendations feature from legacy**

```bash
cp -r app/legacy/shop/src/features/recommendations app/Storefront/src/features/recommendations
```

- [ ] **Step 2: Update similar products path**

```diff
- // No legacy equivalent
+ '/api/storefront/products/{id}/similar'
```

- [ ] **Step 3: Add search-by-image endpoint**

Add to the recommendations repository:

```typescript
searchByImage(file: File): Promise<Result<Product[]>>
```

Implementation: use `FormData` to POST multipart to `/api/storefront/search-by-image`.

- [ ] **Step 4: Commit**

```bash
git add -A app/Storefront/src/features/recommendations/
git commit -m "feat(storefront): port recommendations module with similar products and image search"
```

---

### Task 11: Notifications Module

**Files:**
- Create: All files under `app/Storefront/src/features/notifications/` from legacy
- Modify: API path for notification preferences

- [ ] **Step 1: Copy notifications feature from legacy**

```bash
cp -r app/legacy/shop/src/features/notifications app/Storefront/src/features/notifications
```

- [ ] **Step 2: Update notification preferences path**

```diff
- // No legacy equivalent
+ '/api/store/profiles/notification-preferences'
```

Add the API repository endpoint for notification preferences (GET and PUT).

- [ ] **Step 3: Commit**

```bash
git add -A app/Storefront/src/features/notifications/
git commit -m "feat(storefront): port notifications module with API path"
```

---

### Task 12: Profile Module

**Files:**
- Create: Minimal profile store and views for `/api/store/profiles/profiles` (GET/PUT/DELETE)
- This is a new feature — not directly ported from legacy (legacy identity had `/identity/users/{id}`)

Create a lightweight `features/profile/` with:
- Store: `useProfileStore` (get profile, update profile, deactivate)
- Repository: endpoints for `/api/store/profiles/profiles`
- Mock data
- Integration with AccountView (replace `/identity/users/{id}` calls)

- [ ] **Step 1: Create profile directory structure**

```bash
mkdir -p app/Storefront/src/features/profile/{store,repositories,data,views}
```

- [ ] **Step 2: Create profile types**

`app/Storefront/src/features/profile/types/schemas/profile.schema.ts`:
```typescript
export interface Profile {
  id: string
  userName: string
  email: string
  firstName?: string
  lastName?: string
  phoneNumber?: string
}
```

- [ ] **Step 3: Create profile repository**

`app/Storefront/src/features/profile/repositories/IProfileRepository.ts` with:
- `getProfile(): Promise<Result<Profile>>`
- `updateProfile(data: Partial<Profile>): Promise<Result<Profile>>`
- `deactivateProfile(): Promise<Result<void>>`

Create API and mock implementations.

- [ ] **Step 4: Create profile store**

`app/Storefront/src/features/profile/store/profile.ts` — Pinia store wrapping the repository.

- [ ] **Step 5: Commit**

```bash
git add -A app/Storefront/src/features/profile/
git commit -m "feat(storefront): add profile module for /api/store/profiles/profiles"
```

---

### Task 13: Verify Build

- [ ] **Step 1: Run full verification**

```bash
cd app/Storefront
pnpm install
npx vue-tsc --noEmit
npx vitest run
```

- [ ] **Step 2: Fix any type errors or import issues**

Common issues:
- Missing `@` alias in tsconfig.json
- Missing dependencies in package.json
- Incorrect import paths after file moves
- Legacy type references that need updating

- [ ] **Step 3: Final commit**

```bash
git add -A app/Storefront/
git commit -m "fix(storefront): resolve type errors and verify build"
```
