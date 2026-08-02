# Storefront MVP — Remove USE_MOCK & Enable Real API

**Date:** 2026-08-02
**Status:** Approved
**Context:** The Storefront SPA currently runs entirely on mock data (`USE_MOCK = true` in all 18 services). API repositories exist and have been aligned with backend routes (prior review fixed: double-unwrap bug, 6 route mismatches, missing cart/order endpoints, dead code). The backend is uncertain — endpoints exist in code but haven't been verified end-to-end.

## Goal

Remove all `USE_MOCK` flags from the Storefront. Enable real API calls against the backend. Drop non-essential features that lack backend support or don't contribute to the core purchase demo flow.

## MVP Scope

### Keep — 5 phases, ~42 endpoints

| Phase | Services to flip | Backend check required |
|---|---|---|
| P1 — Catalog | `product`, `category`, `search` | Product list/detail, categories, similar products |
| P2 — Identity | `auth`, `user` | Login, register, token refresh |
| P3 — Cart + Orders | `cart`, `order` | Cart CRUD, checkout, order list/detail, cancel |
| P4 — Payment + Shipping | `payment-intent`, `payment-method`, `shipping-method`, `shipping-rate` | Payment methods, create/confirm intent, shipping |
| P5 — Profile + Remaining | `profile`, 2× `address`, 2× `inventory`, `notifications`, `recommendations` (similar-only) | Profile, addresses, stock, notifications |

### Drop — 15 features

| Feature | Reason | How we drop it |
|---|---|---|
| Personalized recommendations | Backend endpoint doesn't exist | Hide page, API repo stays |
| Search suggestions / autocomplete | Backend endpoint doesn't exist | Strip UI, API method stays (returns 501) |
| Reviews (list, add, ratings) | No storefront API at all | Hide from product detail |
| Wishlists (full CRUD) | No frontend code, non-essential | Defer to post-MVP |
| Search-by-image (visual search) | Complex, non-essential | Hide page/button |
| External login (Google OAuth) | No frontend implementation | Hide button in login form |
| Email change/confirm/resend | No frontend implementation | Defer to post-MVP |
| Password reset (complete flow) | Only forgot-password exists | Show forgot-password but not full reset |
| Save payment method (setup-intent) | Non-essential | Hide checkbox in checkout |
| Payment transaction history | No storefront API | Hide from order detail |
| Cart validate + shipping-rate select | Backend exists, non-essential for checkout | Skip in checkout flow |
| Guest-to-user cart merge | Non-essential for MVP demo | Hide from cart |
| Country/state reference data | Mock-only, no API repo | Hardcode US/CA in checkout forms |
| Notification preferences | Non-essential | Hide from account |
| Avatar upload | Backend returns 501 | Hide from profile |

### Feature Removal Method

For each dropped feature, apply in this order of preference:
1. **Remove the route** — comment out from `src/app/router/index.ts` with `// MVP: dropped — <reason>`
2. **Hide the UI entry point** — use `v-if="false"` with `<!-- MVP: dropped — <reason> -->` comment
3. **Strip from component** — remove the component import and usage from parent views
4. **Leave repo/service code intact** — all `.api.ts`, mock repos, service implementations stay. Easy to restore.

## Implementation Pattern

### Service transformation (every service)

Before:
```ts
const USE_MOCK = true

export class ProductService implements IProductService {
  private readonly productRepository = USE_MOCK ? mockProductRepository : productApiRepository
  // ...
}
```

After:
```ts
export class ProductService implements IProductService {
  private readonly productRepository = productApiRepository
  // ...
}
```

Remove per file:
- `USE_MOCK` constant declaration
- Mock repository import (keep the `.mock.repository.ts` file on disk)
- Any unused import that was only needed by the mock path

No other changes. The service interface, method signatures, and return types are unchanged. Consumers (stores, composables, views) need zero changes.

### Store/component changes

Stores and composables that use services: no changes needed. They call `xxxService.method()` which already returns `Result<T>`. The service interface doesn't change — only the underlying repository changes from mock to API.

### Pages to hide/comment out

In `src/app/router/index.ts`, comment out these routes with `// MVP: dropped`:
```
/recommendations         → RecommendationsView
/account/wishlists        → (doesn't exist yet, skip)
```

In component templates, hide:
- Search suggestions dropdown in `SearchBar.vue`
- Review section in `ProductDetailView.vue`
- "Sign in with Google" button in `LoginForm.vue`
- Newsletter form in `AppFooter.vue` (no backend)
- "Save payment method" checkbox in checkout
- Avatar upload in `ProfileView.vue`

## Error Handling Strategy

Four layers, no mock fallback:

**Layer 1 — BaseRepository** (already correct): All HTTP errors caught, converted to `Result<T>` with `isFailure: true`, status code, and message. No unhandled rejections. `handlePagedError` returns empty paged result.

**Layer 2 — Service** (no changes needed): Passes `Result<T>` through transparently. No retry, no caching, no fallback to mock.

**Layer 3 — Store/Composable** (minimal updates): Check `result.isSuccess` before using data. If failed, leave existing UI state unchanged. For user-initiated actions (add to cart, checkout, login): surface error to view. For background loads (category list, recommendations): silent fail — show empty/unchanged state.

**Layer 4 — View** (minimal updates): Show error toast for failed user actions. Do NOT show error for failed background loads (avoids noise). Use existing toast service.

## Verification Checklist

For each phase, after flipping services:

### Phase 1 — Catalog
- [ ] Home page loads featured products
- [ ] Shop page loads products with pagination
- [ ] Category filter works
- [ ] Search returns results
- [ ] Product detail page loads by slug
- [ ] Product images display
- [ ] Similar products show on detail page
- [ ] Handle: no products found, network error, server 500

### Phase 2 — Identity
- [ ] Login with valid credentials succeeds (token stored)
- [ ] Login with invalid credentials shows error
- [ ] Register creates account
- [ ] Token refresh works (401 → refresh → retry)
- [ ] Token refresh failure redirects to /login
- [ ] Forgot password sends email
- [ ] Route guards work (protected routes redirect unauthenticated)

### Phase 3 — Cart + Orders
- [ ] Add to cart works (anonymous)
- [ ] Update quantity, remove item work
- [ ] Clear cart works
- [ ] Cart persists across page reload
- [ ] Checkout creates order (authenticated)
- [ ] Order history shows after purchase
- [ ] Order detail shows line items
- [ ] Cancel order updates status
- [ ] Handle: add to cart fails, checkout fails, empty cart

### Phase 4 — Payment + Shipping
- [ ] Payment methods load
- [ ] Create payment intent succeeds
- [ ] Confirm payment works
- [ ] Shipping methods load
- [ ] Shipping cost calculates
- [ ] Handle: payment declined, shipping unavailable

### Phase 5 — Profile + Remaining
- [ ] Profile loads and updates
- [ ] Addresses CRUD works
- [ ] Stock availability shows on product detail
- [ ] Similar products (visual) load

## Risks & Mitigations

| Risk | Mitigation |
|---|---|
| Backend endpoints return unexpected shapes | Test each phase against real backend before proceeding to next |
| Backend requires auth on endpoints thought to be anonymous | Add auth checks; if broken, add auth header in API repo |
| Backend response shape differs from frontend `Result<T>` envelope | Fix type definitions or add mapping layer in service |
| Backend is completely down | Frontend shows appropriate error states; demo still explainable |
| Dropping features breaks imports or build | `v-if="false"` preserves imports; route comments preserve references |
| Token refresh cycle infinite-loops | Already handled by `_retry` flag in response interceptor |
| CORS issues between frontend (5173) and backend (5035) | Vite proxy in `vite.config.ts` already configured for `/api` |

## What We DON'T Do

- Don't delete any `.ts` file (mock repos, types, services all stay)
- Don't refactor unrelated code
- Don't add new features
- Don't fix backend endpoints (that's a separate effort)
- Don't write new tests (existing tests should still pass)
- Don't change the router structure or add new routes

## Files Affected

### Services (18 files — remove USE_MOCK)
- `src/features/catalog/services/product/product.service.ts`
- `src/features/catalog/services/category/category.service.ts`
- `src/features/identity/services/auth/auth.service.ts`
- `src/features/identity/services/user/user.service.ts`
- `src/features/ordering/services/cart/cart.service.ts`
- `src/features/ordering/services/order/order.service.ts`
- `src/features/ordering/services/address/address.service.ts`
- `src/features/ordering/services/payment-method/payment-method.service.ts`
- `src/features/ordering/services/shipping-method/shipping-method.service.ts`
- `src/features/payment/services/payment-intent/payment-intent.service.ts`
- `src/features/profile/services/profile.service.ts`
- `src/features/inventory/services/inventory-item/inventory-item.service.ts`
- `src/features/inventory/services/stock-status/stock-status.service.ts`
- `src/features/locations/services/address/address.service.ts`
- `src/features/shipping/services/shipping-rate/shipping-rate.service.ts`
- `src/features/search/services/search.service.ts`
- `src/features/recommendations/services/recommendations.service.ts`
- `src/features/notifications/services/notifications.service.ts`

### Router (1 file — comment out dropped routes)
- `src/app/router/index.ts`

### Views (5-7 files — hide dropped features)
- `src/features/identity/views/LoginView.vue` (hide external login)
- `src/features/catalog/views/ProductDetailView.vue` (hide reviews)
- `src/features/catalog/components/search/SearchBar.vue` (hide suggestions)
- `src/features/ordering/views/CheckoutView.vue` (hide save-payment)
- `src/features/profile/views/ProfileView.vue` (hide avatar upload)
- `src/app/components/layout/AppFooter.vue` (hide newsletter)
- `src/features/catalog/views/HomeView.vue` (hide recommendations section)
