# Storefront API Path Alignment

## Overview

Fix 18 path mismatches between Storefront (`app/Storefront/src/`) and the backend API (`service/Api/src/`). Every fix is a mechanical path change in `.ts` data-layer files — no template edits, no new features, no architectural changes.

## Principles

- Map every Storefront API call to the closest backend equivalent
- Drop methods that have no backend equivalent (transactions list, payment intent GET, cities, getBySlug)
- Let the auth token identify the user — drop `{userId}` from profile paths
- Simplify inventory to match backend's actual endpoints

## Fix Plan by Module

### Catalog (4 files)

| File | Change |
|------|--------|
| `product.api.ts:29` | Remove `searchProducts()` — search uses `getAll` with search params |
| `product.api.ts:36-41` | Replace `getFeaturedProducts()` — call `GET /api/storefront/products` with `{ filter: 'featured:true' }` |
| `product.api.ts:43-48` | Replace `getNewArrivals()` — call `GET /api/storefront/products` with `{ sort: { sortBy: 'createdAt', sortOrder: 'desc' } }` |
| `category.api.ts:17-19` | Remove `getBySlug()` — backend has no taxon-by-slug endpoint |

### Inventory (3 files)

Backend storefront inventory endpoints:
- `GET /api/storefront/availability/{variantId}` — stock check
- `POST /api/storefront/cart/reserve` — create reservation
- `GET /api/storefront/cart/reserve` — list reservations

| File | Change |
|------|--------|
| `inventory-item.api.ts` | Replace `getById` → `GET /api/storefront/availability/{variantId}`. Drop `getAll` (low-stock is admin). Replace `reserveStock` → `POST /api/storefront/cart/reserve`. Drop `releaseStock`. Drop `updateQuantity`. |
| `stock-status.api.ts` | Replace path from `/inventory/{id}/stock-status` → `/availability/{variantId}` |
| `inventory-item.repository.interface.ts` | Drop `getAll`, `updateQuantity`, `releaseStock`. Add `getReservations(cartToken): Promise<Result<Reservation[]>>` calling `GET /api/storefront/cart/reserve` |
| `inventory.constants.ts` | Replace all `/api/storefront/inventory/*` paths with `/api/storefront/availability/*` and `/api/storefront/cart/reserve` |

### Locations (1 file)

| File | Change |
|------|--------|
| `locations.constants.ts` | `COUNTRIES`: `/api/store/profiles/addresses/countries` → `/api/store/locations/countries`. `REGIONS`: → `/api/store/locations/states`. Drop `CITIES`, `DEFAULT`, `SET_DEFAULT` constants. |

### Profile (1 file)

| File | Change |
|------|--------|
| `profile.api.ts:8-9` | `getProfile(userId)` → drop `{userId}`, call `GET /api/store/profiles/profiles` |
| `profile.api.ts:11-13` | `updateProfile(userId, ...)` → drop `{userId}`, call `PUT /api/store/profiles/profiles` |
| `profile.api.ts:15-18` | Remove `uploadAvatar()` — backend has no upload endpoint. Mock still works via `MockProfileRepository`. |

### Payment (2 files)

| File | Change |
|------|--------|
| `payment-intent.api.ts:11-13` | Remove `getById` — backend has no GET payment intent endpoint |
| `transaction.api.ts` | Delete file — backend has no storefront transactions endpoint |
| `transaction.repository.interface.ts` | Keep interface — mock still implements it. Service continues to work with mock data. |

### Interceptors (2 files)

| File | Change |
|------|--------|
| `core/interceptors/response.interceptor.ts:28` | `${baseURL}/identity/auth/refresh` → `/api/store/identity/auth/sessions/refresh` (absolute path, no baseURL) |
| `core/http/interceptors/response.interceptor.ts:28` | Same fix |

### Shipping (1 file)

| File | Change |
|------|--------|
| `shipping.constants.ts:2-3` | `'/shipping/methods'` → `/api/storefront/shipping/methods` |

## Files Not Modified

- `.vue` files — zero template/style changes
- Mock repositories — stay as-is (mock data doesn't hit the network)
- Service layer — stays as-is (services delegate to repos, repo paths are the fix)
- Store layer — stays as-is (stores call services)
- `profile.repository.interface.ts` — `uploadAvatar` stays in interface (mock implements it)
- `profile.service.ts` — `uploadAvatar` stays (mock still works, API impl removed)

## Verification

- Build must pass: `pnpm build` (type-check + vite build)
- All paths verified against backend `service/Api/src/Module/*/Features/*/Endpoint/*.cs` files

## Risk

- Dropping `getBySlug` may affect any view that resolves categories by slug. Catalog views currently use category IDs in route params (not slugs), so impact should be zero.
- Dropping `transaction.api.ts` means transaction history will use mock data until backend adds the endpoint.
