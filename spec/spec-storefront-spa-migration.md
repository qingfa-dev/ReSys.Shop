---
title: Store SPA Migration — Frontend API Client Alignment with New Route Map
version: 1.0
date_created: 2025-08-11
last_updated: 2025-08-11
owner: ReSys.Shop Platform
tags: design, frontend, spa, migration, api, vue
---

# Introduction

This specification defines the Store SPA frontend changes required to align with the refactored backend route map. All API client files, TypeScript types, Vue composables, and view components must be updated to call the new routes, use the new HTTP methods, and read embedded stock data from product responses instead of separate availability API calls.

## 1. Purpose & Scope

### Purpose

Update all Store SPA API consumer code to match the new storefront endpoint routes, HTTP methods, and response shapes. The SPA must compile, pass all unit tests, and function correctly after the backend routes change atomically.

### Scope

- 7 API service files updated
- 3 files deleted (availabilityApi.ts, useAvailability.ts, related type)
- 2 view components updated (ProductDetailView, related composable)
- 1 TypeScript type interface extended
- `pnpm run test:unit && pnpm run lint` must pass

### Out of Scope

- Admin SPA (admin routes unchanged)
- New frontend features
- UI redesign
- Store SPA build pipeline changes

## 2. Definitions

| Term | Definition |
|------|------------|
| **API Client** | TypeScript service class wrapping `get`, `post`, `put`, `patch`, `del` HTTP helpers |
| **Composable** | Vue 3 `useX()` reactive singleton for shared state and logic |
| **Zod Schema** | Runtime validation schema used with `z.parse()` to verify API responses |
| **PATCH** | HTTP method replacing `PUT` for partial updates — requires `patch()` not `put()` |

## 3. Requirements, Constraints & Guidelines

### Route Change Requirements

- **RTC-001**: All cart routes: `/api/storefront/ordering/cart` → `/api/storefront/cart`
- **RTC-002**: Cart item routes: `/api/storefront/ordering/cart/items` → `/api/storefront/cart/items`
- **RTC-003**: Checkout route: `/api/storefront/ordering/cart/checkout` → `/api/storefront/cart/checkout`
- **RTC-004**: Shipping rate: `/api/storefront/ordering/cart/shipping-rate` → `/api/storefront/cart/shipping-rate`
- **RTC-005**: Payment intent: `/api/storefront/billing/payments/create-intent` → `/api/storefront/cart/payment/intent`
- **RTC-006**: Payment confirm: `/api/storefront/billing/payments/confirm/{id}` → `/api/storefront/cart/payment/intent/{id}/confirm`
- **RTC-007**: Orders: `/api/storefront/ordering/orders` → `/api/storefront/orders`
- **RTC-008**: Order cancel: `PUT` → `POST` method
- **RTC-009**: Shipping calculate: `POST` → `GET` method
- **RTC-010**: Inventory reserve: `/api/storefront/inventory/cart/reserve` → `/api/storefront/inventory/stock-reservations`
- **RTC-011**: Inventory list reservations: `/api/storefront/inventory/cart/reserve` → `/api/storefront/inventory/stock-reservations` (GET)
- **RTC-012**: Inventory release: `/api/storefront/inventory/cart/reserve/{id}` → `/api/storefront/inventory/stock-reservations/{id}`

### HTTP Method Change Requirements

- **HMC-001**: All `put()` calls for partial updates → `patch()` (cart update, cart item qty, customer profile, address, wishlist)
- **HMC-002**: Shipping calculate `post()` → `get()` with query params
- **HMC-003**: Order cancel `put()` → `post()`
- **HMC-004**: Cart empty `post('/cart/empty')` → `del('/cart/items')`

### Deletion Requirements

- **DEL-001**: `app/Store/src/features/inventory/services/availabilityApi.ts` — deleted
- **DEL-002**: `app/Store/src/features/inventory/composables/useAvailability.ts` — deleted
- **DEL-003**: `app/Store/src/features/inventory/types/availability.ts` — review, keep `CartReservation`/`CartReservationStatus` types, remove `AvailabilityEntry`

### Type Update Requirements

- **TYP-001**: `StoreVariantStockInfo` gains: `availableQuantity: number`, `backorderable: boolean`
- **TYP-002**: `ReserveStockRequest` and `CartReservation` types move to inventory types, keep shape unchanged

## 4. File Change Map

### Files to Delete

```
DELETE app/Store/src/features/inventory/services/availabilityApi.ts
DELETE app/Store/src/features/inventory/composables/useAvailability.ts
```

### Files to Modify

```
MODIFY app/Store/src/features/ordering/services/cartApi.ts
MODIFY app/Store/src/features/ordering/services/checkoutApi.ts
MODIFY app/Store/src/features/ordering/services/orderApi.ts
MODIFY app/Store/src/features/ordering/composables/useCart.ts
MODIFY app/Store/src/features/inventory/services/cartReservationApi.ts
MODIFY app/Store/src/features/inventory/types/availability.ts
MODIFY app/Store/src/features/payment/services/paymentApi.ts
MODIFY app/Store/src/features/shipping/services/shippingApi.ts
MODIFY app/Store/src/features/catalog/types/product.ts
MODIFY app/Store/src/features/catalog/composables/useProductDetail.ts
MODIFY app/Store/src/features/catalog/views/ProductDetailView.vue
MODIFY app/Store/src/features/inventory/services/index.ts
MODIFY app/Store/src/features/inventory/index.ts
```

### Detailed File Changes

#### cartApi.ts
```typescript
// BEFORE
private static readonly BASE = '/api/storefront/ordering/cart'
static async emptyCart(): Promise<Result<null>> {
  return await post<Result<null>>(`${this.BASE}/empty`)
}

// AFTER
private static readonly BASE = '/api/storefront/cart'
static async emptyCart(): Promise<Result<null>> {
  return await del<Result<null>>(`${this.BASE}/items`)
}
// All put() → patch() for addItem, updateItem
```

#### checkoutApi.ts
```typescript
// BEFORE
static async updateCheckout(req: UpdateCheckoutRequest): Promise<Result<void>> {
  return await put<Result<void>>('/api/storefront/ordering/cart', req)
}
static async createPaymentIntent(req: CreatePaymentIntentRequest): Promise<Result<PaymentIntentResponse>> {
  const result = await post<Result<PaymentIntentResponse>>('/api/storefront/billing/payments/create-intent', req)
}

// AFTER
static async updateCheckout(req: UpdateCheckoutRequest): Promise<Result<void>> {
  return await patch<Result<void>>('/api/storefront/cart', req)
}
static async createPaymentIntent(req: CreatePaymentIntentRequest): Promise<Result<PaymentIntentResponse>> {
  const result = await post<Result<PaymentIntentResponse>>('/api/storefront/cart/payment/intent', req)
}
```

#### cartReservationApi.ts
```typescript
// BEFORE
export function reserveStock(req: ReserveStockRequest, cartToken: string): Promise<Result<CartReservation>> {
  return post<Result<CartReservation>>('/api/storefront/inventory/cart/reserve', req, {
    headers: { 'X-Cart-Token': cartToken },
  })
}
export function releaseReservation(reservationId: string): Promise<Result<null>> {
  return del<Result<null>>(`/api/storefront/inventory/cart/reserve/${reservationId}`)
}
export function getCartReservations(cartToken: string, params): Promise<PagedResult<CartReservationStatus>> {
  return getPaged<CartReservationStatus>('/api/storefront/inventory/cart/reserve', params, {
    headers: { 'X-Cart-Token': cartToken },
  })
}

// AFTER
export function reserveStock(req: ReserveStockRequest, cartToken: string): Promise<Result<CartReservation>> {
  return post<Result<CartReservation>>('/api/storefront/inventory/stock-reservations', req, {
    headers: { 'X-Cart-Token': cartToken },
  })
}
export function releaseReservation(reservationId: string): Promise<Result<null>> {
  return del<Result<null>>(`/api/storefront/inventory/stock-reservations/${reservationId}`)
}
export function getCartReservations(cartToken: string, params): Promise<PagedResult<CartReservationStatus>> {
  return getPaged<CartReservationStatus>('/api/storefront/inventory/stock-reservations', params, {
    headers: { 'X-Cart-Token': cartToken },
  })
}
```

#### shippingApi.ts
```typescript
// BEFORE
import { getPaged } from '@/shared/api'
// Calculate called with post() elsewhere

// AFTER
import { getPaged, get } from '@/shared/api'
// NEW: add calculate function
export function calculateShipping(shippingMethodId: string, orderId: string): Promise<Result<ShippingCost>> {
  return get<Result<ShippingCost>>(`/api/storefront/shipping/calculate?shippingMethodId=${shippingMethodId}&orderId=${orderId}`)
}
```

## 5. Acceptance Criteria

- **AC-001**: `pnpm run lint` passes with zero errors
- **AC-002**: `pnpm run test:unit` passes — all existing tests updated for new routes
- **AC-003**: `pnpm run build` succeeds with no TypeScript errors
- **AC-004**: No import of `availabilityApi` or `useAvailability` in any file
- **AC-005**: `availabilityApi.ts` file does not exist on disk
- **AC-006**: `useAvailability.ts` file does not exist on disk
- **AC-007**: Cart API calls go to `/api/storefront/cart/*` (verify via network tab or unit test mocks)
- **AC-008**: Payment intent API calls go to `/api/storefront/cart/payment/intent`
- **AC-009**: Order API calls go to `/api/storefront/orders/*`
- **AC-010**: Inventory API calls go to `/api/storefront/inventory/stock-reservations`
- **AC-011**: Shipping calculate uses GET not POST
- **AC-012**: Product detail page renders stock information without extra API call

## 6. Test Automation Strategy

### Unit Tests

- Update mock routes in all service test files (`.spec.ts`) to match new paths
- Verify `patch()` calls replace `put()` calls in cart, customer service tests
- Verify `get()` replaces `post()` in shipping calculate test
- Verify `del()` replaces `post()` in empty cart test
- Remove `availabilityApi.spec.ts`
- Remove `useAvailability.spec.ts` (or their composable equivalents)

### Verification Commands
```bash
cd app/Store
pnpm run lint
pnpm run test:unit
pnpm run build
```

## 7. Rationale & Context

### Why atomic frontend migration?

Backend route constants are `public const string` — changing them instantly removes old routes. There is no overlap period where both old and new routes work. The frontend must be updated in the same deploy. This is standard for monorepo projects where backend and frontend deploy together.

### Why DELETE /cart/items vs POST /cart/empty?

`del('/cart/items')` sends an HTTP DELETE to remove all items from the items collection — this is the frontend-side call matching the backend's `DELETE /api/storefront/cart/items` endpoint. The old `post('/cart/empty')` was semantically wrong (POST for deletion).

### Why PATCH instead of PUT?

The frontend currently sends partial JSON bodies to update endpoints (e.g., `{ email: "new@example.com" }` for cart update). The backend accepts partial updates — it only modifies provided fields. This is PATCH semantics. The `patch()` function exists in the API client (`app/Store/src/shared/api/client.ts`) and works identically to `put()` with regard to request serialization. Changing from `put()` to `patch()` is a search-and-replace in service files.
