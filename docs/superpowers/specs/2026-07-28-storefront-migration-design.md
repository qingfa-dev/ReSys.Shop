# Storefront Migration: Legacy Shop → New Storefront

## Overview

Migrate the legacy shop SPA (`app/legacy/shop/`) into the new Storefront
(`app/Storefront/`), adapting all API calls to the new `/api/storefront/*` and
`/api/store/*` endpoints while preserving PrimeVue components, styling, mock
data layer, and test coverage.

## Approach

**In-place rewrite.** Feature modules are ported one by one from legacy into
`app/Storefront/src/features/`, keeping the legacy feature-module structure.
Endpoint paths and auth model are updated per feature. The new Storefront's
existing `views/`, `stores/`, and `api.ts` are replaced by ported legacy code.

Cart token generated via `crypto.randomUUID()` on first add-to-cart.

## Phases

### Phase 0 — Core Infrastructure

Port legacy `core/` and `app/` into `app/Storefront/src/`.

**Port from legacy `core/`:**
- `core/services/api.ts` — Axios client with Bearer token interceptor, 401
  refresh-with-retry queue, and `X-Cart-Token` header injection. Update
  refresh endpoint to `/api/store/identity/auth/sessions/refresh`.
- `core/services/toast.ts`
- `core/http/` — Axios factory + interceptors (covered by api.ts)
- `core/models/result.ts` — `Result<T>`, `PagedResult<T>`, helpers
- `core/repositories/` — `IRepository<T>`, `BaseRepository`, mock repos
- `core/mappers/response-mapper.ts`
- `core/helpers/` — `QueryBuilder`, `MockQueryHelper`
- `core/utils/result.ts` — result utilities

**Port from legacy `app/`:**
- `app/stores/` — uiStore, preferencesStore, counterStore
- `app/router/index.ts` — all 19 routes, auth navigation guard, scrollBehavior
- `app/layouts/` — DefaultLayout, AppHeader, AppFooter, MobileNav
- `app/composables/`

**Delete from new Storefront:**
- `api.ts` (replaced by `core/services/api.ts`)
- `stores/catalog.ts` (replaced by `features/catalog/store/`)
- `stores/cart.ts` (replaced by `features/ordering/store/`)
- `views/` all 5 stubs (replaced by feature views)

**Vite config:**
- Add `@` path alias → `./src`
- Add proxy `/api` → `http://localhost:5035`

### Phase 1 — Catalog + Identity

**Catalog** (`features/catalog/`):
- Port all files verbatim from legacy
- Update repository paths:
  - `/api/products` → `/api/storefront/products`
  - `/api/products/{id}` → `/api/storefront/products/{slug}`
  - `/api/products/search` → `/api/storefront/products?search=`
  - `/api/products/featured` → `/api/storefront/products?featured=true`
  - `/api/products/new` → `/api/storefront/products?sort=newest`
  - `/api/categories` → `/api/storefront/taxonomies/{id}`
  - `/api/categories/slug/{slug}` → `/api/storefront/taxons/{id}/products`
- Drop WishlistButton component
- Preserve components: ProductCard, ProductGrid, ProductFilters,
  ProductVariantSelector, CategoryNav, HeroBanner, FeaturedProducts

**Identity** (`features/identity/`):
- Port all files from legacy
- Update auth paths:
  - `/identity/auth/login` → `/api/store/identity/auth/login/password`
  - `/identity/auth/register` → `/api/store/identity/auth/register`
  - `/identity/auth/logout` → `/api/store/identity/auth/logout`
  - `/identity/auth/refresh` → `/api/store/identity/auth/sessions/refresh`
  - `/identity/auth/forgot-password` → `/api/store/identity/passwords/forgot`
  - `/identity/auth/change-password` → `/api/store/identity/passwords/change`
  - `/identity/users/{id}` → drop (use `/api/store/profiles/profiles`)
- Drop MFA from auth store (removed from new API)
- Preserve views: LoginView, RegisterView, AccountView, TermsView, PrivacyView

### Phase 2 — Cart, Checkout, Orders, Payment, Shipping, Locations, Inventory

**Cart** (`features/ordering/store/cart.ts`):
- Port API-backed cart from legacy; update paths:
  - `/ordering/cart` → `/api/storefront/cart`
  - `/ordering/cart/items` → `/api/storefront/cart/items`
  - `/ordering/cart/items/{id}` → `/api/storefront/cart/items/{lineItemId}`
- Add cart token: generate UUID on first add-to-cart, store in localStorage,
  send as `X-Cart-Token` header on all cart requests
- Drop coupon endpoints (not in new API)

**Checkout** (new multi-step flow):
1. `POST /api/storefront/cart/checkout` — create order from cart
2. `POST /api/storefront/cart/validate` — validate checkout state
3. `POST /api/storefront/cart/shipping-rate` — select shipping rate
4. `POST /api/storefront/payment/create-intent` — create payment for order
5. `POST /api/storefront/payment/confirm/{paymentId}` — confirm after gateway

**Orders** — port with path mapping:
- `/ordering/orders` → `/api/storefront/orders`
- `/ordering/orders/{id}` → `/api/storefront/orders/{id}`

**Payment** — port with path mapping:
- `/ordering/payment-methods` → `/api/storefront/payment/methods`
- `/payment/intents` → `/api/storefront/payment/create-intent`
- `/payment/intents/{id}/confirm` → `/api/storefront/payment/confirm/{paymentId}`

**Shipping** — port with path mapping:
- `/ordering/shipping-methods` → `/api/storefront/shipping/methods`
- `/shipping/rates` → `/api/storefront/shipping/rates`
- `/shipping/rates/{id}/calculate` → `/api/storefront/shipping/calculate`

**Locations** — addresses move:
- `/locations` → `/api/store/profiles/addresses`
- `/locations/default` → drop (filter by `isDefault` on client)
- `/locations/stores` → drop (not in new API)
- New: `/api/store/locations/countries`, `/api/store/locations/states`

**Inventory:**
- `/inventory/{id}/stock-status` → `/api/storefront/availability/{variantId}`
- New: `/api/storefront/cart/reserve` (reserve stock for cart)

### Phase 3 — Remaining Features

| Feature | Adaptation |
|---|---|
| **Search** | `/api/storefront/products?search=` with facet params |
| **Reviews** | Mock-only (no API endpoint yet) |
| **Recommendations** | `/api/storefront/products/{id}/similar` + `/api/storefront/search-by-image` (multipart) |
| **Notifications** | `/api/store/profiles/notification-preferences` |

**Dropped entirely** (no new API equivalent):
- Wishlist, promotions / coupons, returns, settings
- MFA
- Shipment tracking
- Transaction refund

## API Endpoint Mapping (Reference)

| Pattern | Legacy | New |
|---|---|---|
| Catalog prefix | `/api/products*` | `/api/storefront/products*` |
| Identity prefix | `/identity/auth/*` | `/api/store/identity/auth/*` |
| Cart prefix | `/ordering/cart*` | `/api/storefront/cart*` |
| Orders prefix | `/ordering/orders*` | `/api/storefront/orders*` |
| Payment prefix | `/payment/*` | `/api/storefront/payment/*` |
| Shipping prefix | `/shipping/*` | `/api/storefront/shipping/*` |
| Addresses | `/locations` | `/api/store/profiles/addresses` |
| Profile | n/a | `/api/store/profiles/profiles` |
| Locations ref data | n/a | `/api/store/locations/countries`, `/states` |

## Auth Model Changes

- Legacy: simple JWT with `accessToken` + `refreshToken` in localStorage,
  refresh at `/identity/auth/refresh`
- New: session-based, login at `/api/store/identity/auth/login/password`,
  refresh at `/api/store/identity/auth/sessions/refresh`
- New: `X-Cart-Token` header for guest cart identification (UUID generated
  via `crypto.randomUUID()` on first add-to-cart)
- Token storage pattern (localStorage key names) stays the same

## Mock Data Layer

Keep `USE_MOCK` toggle and all mock repositories from legacy. Each service
selects mock vs API repository via `const USE_MOCK = true`. This allows
offline development while the new API is still in flux.

## Features Dropped

The following legacy features are not migrated:
- Wishlist, promotions/coupons, returns, settings
- MFA (enable/verify/disable)
- Shipment tracking
- Transaction refund

## Test Strategy

Port all existing legacy tests alongside their feature modules. Update
mocked API paths in tests to match new endpoints. No new test coverage
required for the migration itself.
