# Storefront API Path Alignment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix 18 API path mismatches between Storefront calls and backend endpoints across 7 modules.

**Architecture:** Mechanical path corrections in `.api.ts` repository files and `.constants.ts` files. No template/style changes, no new features. Each fix maps Storefront to the closest backend equivalent, drops methods with no backend counterpart, and updates stale refresh paths in interceptors.

**Tech Stack:** TypeScript, Vue 3, Axios, Vite

## Global Constraints

- Only `.ts` files may be edited — never `.vue` templates or styles
- `USE_MOCK = true` toggle preserved in all services so mock data works offline
- Every path change must be verified against actual backend endpoint at `service/Api/src/Module/*/Features/*/Endpoint/*.cs`
- Build must pass after each task: `pnpm build` (type-check + vite build)

---

### Task 1: Catalog — Drop non-existent endpoints

**Files:**
- Modify: `app/Storefront/src/features/catalog/repositories/product/product.api.ts`
- Modify: `app/Storefront/src/features/catalog/repositories/product/product.repository.interface.ts`
- Modify: `app/Storefront/src/features/catalog/repositories/category/category.api.ts`
- Modify: `app/Storefront/src/features/catalog/repositories/category/category.repository.interface.ts`

**Interfaces:**
- Consumes: `BaseRepository` from `@/core/repositories`
- Produces: Updated `IProductRepository` and `ICategoryRepository` with only backend-supported methods

- [ ] **Step 1: Remove `searchProducts` from product API repo**

In `product.api.ts`, remove the `searchProducts` method (lines 27-34). Search is done via `getAll` with search params hitting `GET /api/storefront/products?search=...`.

```typescript
// Remove entire method:
// async searchProducts(query: string, limit = 10): Promise<PagedResult<ProductResponse>> {
```

- [ ] **Step 2: Replace `getFeaturedProducts` to use query param filter**

In `product.api.ts`, replace the `getFeaturedProducts` method (lines 36-41):

```typescript
  async getFeaturedProducts(limit = 8): Promise<PagedResult<ProductResponse>> {
    return super.getPaged<ProductResponse>(
      this.endpoint,
      { page: 1, pageSize: limit },
      { filter: 'featured:true' }
    )
  }
```

- [ ] **Step 3: Replace `getNewArrivals` to use sort param**

In `product.api.ts`, replace the `getNewArrivals` method (lines 43-48):

```typescript
  async getNewArrivals(limit = 8): Promise<PagedResult<ProductResponse>> {
    return super.getPaged<ProductResponse>(
      this.endpoint,
      { page: 1, pageSize: limit },
      undefined,
      undefined,
      { sortBy: 'createdAt', sortOrder: 'desc' }
    )
  }
```

- [ ] **Step 4: Remove `searchProducts`, `getFeaturedByCategory` and `getNewArrivals` from interface**

In `product.repository.interface.ts`, remove the method signatures for `searchProducts`, `getFeaturedProducts` (if `getByCategory` equivalent doesn't exist), `getNewArrivals`. Keep `getAll`, `getById`, `getProductBySlug`.

- [ ] **Step 5: Remove `getBySlug` from category API repo**

In `category.api.ts`, remove the `getBySlug` method (lines 17-19) entirely.

- [ ] **Step 6: Remove `getBySlug` from category interface**

In `category.repository.interface.ts`, remove the `getBySlug` method signature.

- [ ] **Step 7: Commit**

```bash
git add -A app/Storefront/src/features/catalog/
git commit -m "fix(storefront): remove non-existent catalog endpoints (search, featured, new, bySlug)"
```

---

### Task 2: Inventory — Rewrite to match backend availability/reserve endpoints

**Files:**
- Modify: `app/Storefront/src/features/inventory/repositories/inventory-item/inventory-item.api.ts`
- Modify: `app/Storefront/src/features/inventory/repositories/inventory-item/inventory-item.repository.interface.ts`
- Modify: `app/Storefront/src/features/inventory/repositories/inventory-item/inventory-item.mock.repository.ts`
- Modify: `app/Storefront/src/features/inventory/types/constants/inventory.constants.ts`
- Modify: `app/Storefront/src/features/inventory/services/inventory-item/inventory-item.service.interface.ts`
- Modify: `app/Storefront/src/features/inventory/services/inventory-item/inventory-item.service.ts`
- Modify: `app/Storefront/src/features/inventory/repositories/stock-status/stock-status.api.ts`
- Modify: `app/Storefront/src/features/inventory/services/stock-status/stock-status.service.ts`

**Interfaces:**
- Consumes: `BaseRepository`, `Result`, `PagedResult` from `@/core`
- Produces: Updated `IInventoryItemRepository` matching backend availability/cart-reserve endpoints

Backend storefront inventory endpoints:
- `GET /api/storefront/availability/{variantId}` — stock check
- `POST /api/storefront/cart/reserve` — create reservation
- `GET /api/storefront/cart/reserve` — list reservations

- [ ] **Step 1: Update `IInventoryItemRepository` interface**

In `inventory-item.repository.interface.ts`:

```typescript
import type { Result } from '@/core/models/result'

export interface Reservation {
  id: string
  variantId: string
  quantity: number
  expiresAt: string
}

export interface IInventoryItemRepository {
  getById<T = any>(id: string): Promise<Result<T>>
  getStockStatus(productId: string): Promise<Result<any>>
  reserveStock(variantId: string, quantity: number, cartToken: string): Promise<Result<any>>
  getReservations(cartToken: string): Promise<Result<Reservation[]>>
}
```

Remove `getAll`, `updateQuantity`, `releaseStock`. Add `getReservations` and `Reservation` type.

- [ ] **Step 2: Rewrite `InventoryItemApiRepository`**

In `inventory-item.api.ts`, replace all methods:

```typescript
import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { IInventoryItemRepository, Reservation } from './inventory-item.repository.interface'

export class InventoryItemApiRepository extends BaseRepository implements IInventoryItemRepository {
  async getById<T = any>(id: string): Promise<Result<T>> {
    return this.get<T>(`/api/storefront/availability/${id}`)
  }

  async getStockStatus(productId: string): Promise<Result<any>> {
    return this.get<any>(`/api/storefront/availability/${productId}`)
  }

  async reserveStock(variantId: string, quantity: number, cartToken: string): Promise<Result<any>> {
    return this.post<any>('/api/storefront/cart/reserve', { variantId, quantity, cartToken })
  }

  async getReservations(cartToken: string): Promise<Result<Reservation[]>> {
    return this.get<Reservation[]>('/api/storefront/cart/reserve', { filter: `cartToken:${cartToken}` })
  }
}

export const inventoryItemApiRepository = new InventoryItemApiRepository()
```

- [ ] **Step 3: Update `MockInventoryItemRepository`**

In `inventory-item.mock.repository.ts`, remove `getAll`, `updateQuantity`, `releaseStock`. Keep `getById`, `getStockStatus`, `reserveStock` (adapt reserveStock to accept the new signature). Add `getReservations` returning mock data.

- [ ] **Step 4: Update inventory constants**

In `inventory.constants.ts`:

```typescript
export const INVENTORY_ENDPOINTS = {
  AVAILABILITY: (variantId: string) => `/api/storefront/availability/${variantId}`,
  CART_RESERVE: '/api/storefront/cart/reserve',
} as const
```

Replace all old `/api/storefront/inventory/*` paths.

- [ ] **Step 5: Update `IInventoryItemService` interface**

In `inventory-item.service.interface.ts`, remove `getLowStockProducts`, `updateQuantity`, `releaseStock`. Add `getReservations(cartToken: string): Promise<Result<Reservation[]>>`.

- [ ] **Step 6: Update `InventoryItemService`**

In `inventory-item.service.ts`, remove `getLowStockProducts`, `updateQuantity`, `releaseStock` methods. Add `getReservations`. Adapt `reserveStock` to pass `cartToken` from localStorage.

```typescript
  async getReservations(): Promise<Result<Reservation[]>> {
    const cartToken = localStorage.getItem('cartToken') || ''
    return this.inventoryItemRepository.getReservations(cartToken)
  }
```

- [ ] **Step 7: Fix stock-status API path**

In `stock-status.api.ts`, change line 8:
```typescript
return this.get<StockStatusResponse>(`/api/storefront/availability/${productId}`)
```

- [ ] **Step 8: Fix stock-status service if needed**

In `stock-status.service.ts`, verify it still compiles with the updated path. No logic change needed if it delegates to the repo.

- [ ] **Step 9: Build and verify**

```bash
cd app/Storefront && pnpm build
```

Expected: clean type-check + build.

- [ ] **Step 10: Commit**

```bash
git add -A app/Storefront/src/features/inventory/
git commit -m "fix(storefront): rewrite inventory API to match backend availability/cart-reserve endpoints"
```

---

### Task 3: Locations — Fix country/state paths, drop non-existent constants

**Files:**
- Modify: `app/Storefront/src/features/locations/types/constants/locations.constants.ts`

- [ ] **Step 1: Fix location constants**

In `locations.constants.ts`, update:

```typescript
export const LOCATIONS_ENDPOINTS = {
  COUNTRIES: '/api/store/locations/countries',
  COUNTRY: (id: string) => `/api/store/locations/countries/${id}`,
  COUNTRY_BY_ISO: (isoCode: string) => `/api/store/locations/countries/by-iso/${isoCode}`,
  STATES: '/api/store/locations/states',
  STATE: (id: string) => `/api/store/locations/states/${id}`,
  STATE_BY_ISO: (isoCode: string) => `/api/store/locations/states/by-iso/${isoCode}`,
} as const
```

Drop `REGIONS`, `CITIES`, `LOCATIONS`, `LOCATION` from this block (they're duplicates of `ADDRESS_ENDPOINTS` or don't exist on backend).

The `ADDRESS_ENDPOINTS` block is correct — keep as-is. Drop `DEFAULT` and `SET_DEFAULT` from it:

```typescript
export const ADDRESS_ENDPOINTS = {
  ADDRESSES: '/api/store/profiles/addresses',
  ADDRESS: (id: string) => `/api/store/profiles/addresses/${id}`,
} as const
```

- [ ] **Step 2: Build and verify**

```bash
cd app/Storefront && pnpm build
```

- [ ] **Step 3: Commit**

```bash
git add -A app/Storefront/src/features/locations/
git commit -m "fix(storefront): fix location country/state paths, drop non-existent constants"
```

---

### Task 4: Profile — Drop userId from paths, remove uploadAvatar from API repo

**Files:**
- Modify: `app/Storefront/src/features/profile/repositories/profile.api.ts`

- [ ] **Step 1: Rewrite `ProfileApiRepository`**

In `profile.api.ts`:

```typescript
import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { ProfileResponse } from '../types/response'
import type { IProfileRepository } from './profile.repository.interface'
import { PROFILE_ENDPOINTS } from '../types/constants'

export class ProfileApiRepository extends BaseRepository implements IProfileRepository {
  async getProfile(_userId: string): Promise<Result<ProfileResponse>> {
    return this.get<ProfileResponse>(PROFILE_ENDPOINTS.GET_PROFILE)
  }

  async updateProfile(_userId: string, updates: Partial<ProfileResponse>): Promise<Result<ProfileResponse>> {
    return this.put<ProfileResponse>(PROFILE_ENDPOINTS.UPDATE_PROFILE, updates)
  }

  async uploadAvatar(_userId: string, _file: File): Promise<Result<ProfileResponse>> {
    return { isSuccess: false, isFailure: true, statusCode: 501, message: 'Avatar upload not available via API' }
  }
}

export const profileApiRepository = new ProfileApiRepository()
```

Key changes:
- `getProfile`: ignore `userId`, call `GET /api/store/profiles/profiles`
- `updateProfile`: ignore `userId`, call `PUT /api/store/profiles/profiles` with body
- `uploadAvatar`: stub returns 501 — mock still works when `USE_MOCK=true`

- [ ] **Step 2: Build and verify**

```bash
cd app/Storefront && pnpm build
```

- [ ] **Step 3: Commit**

```bash
git add -A app/Storefront/src/features/profile/
git commit -m "fix(storefront): drop userId from profile API paths, stub uploadAvatar"
```

---

### Task 5: Payment — Remove non-existent endpoints (payment intents GET, transactions)

**Files:**
- Modify: `app/Storefront/src/features/payment/repositories/payment-intent/payment-intent.api.ts`
- Delete: `app/Storefront/src/features/payment/repositories/transaction/transaction.api.ts`

- [ ] **Step 1: Remove `getById` from payment intent API repo**

In `payment-intent.api.ts`, remove the `getById` method. Keep `create` and `confirm` — those match backend endpoints.

```typescript
export class PaymentIntentApiRepository extends BaseRepository implements IPaymentIntentRepository {
  async create(amount: number, currency = 'USD'): Promise<Result<PaymentIntentResponse>> {
    return this.post<PaymentIntentResponse>('/api/storefront/payment/create-intent', { amount, currency })
  }

  async getById<T = PaymentIntentResponse>(_id: string): Promise<Result<T>> {
    return { isSuccess: false, isFailure: true, statusCode: 501, message: 'Payment intent lookup not available via API' } as Result<T>
  }

  async confirm(paymentIntentId: string, paymentMethodId: string): Promise<Result<PaymentIntentResponse>> {
    return this.post<PaymentIntentResponse>(`/api/storefront/payment/confirm/${paymentIntentId}`, { paymentMethodId })
  }
}
```

Keep `getById` as a stub returning 501 (interface requires it, mock implements it).

- [ ] **Step 2: Delete transaction API repo file**

```bash
rm app/Storefront/src/features/payment/repositories/transaction/transaction.api.ts
```

The `ITransactionRepository` interface stays (mock implements it). The `TransactionService` already defaults to `USE_MOCK=true`, so it will use the mock repository and never hit the deleted API file.

- [ ] **Step 3: Update payment constants**

In `payment.constants.ts`, remove `PROCESS`, `VERIFY`, `REFUND` entries if present — they don't match any backend storefront endpoint. Keep only `METHODS`, `METHOD` (payment methods listing). The `create-intent` and `confirm` paths are hardcoded in the API repo, not using constants.

- [ ] **Step 4: Build and verify**

```bash
cd app/Storefront && pnpm build
```

- [ ] **Step 5: Commit**

```bash
git add -A app/Storefront/src/features/payment/
git commit -m "fix(storefront): remove non-existent payment endpoints (intents GET, transactions API)"
```

---

### Task 6: Interceptors — Fix stale refresh path

**Files:**
- Modify: `app/Storefront/src/core/interceptors/response.interceptor.ts`
- Modify: `app/Storefront/src/core/http/interceptors/response.interceptor.ts`

- [ ] **Step 1: Fix first interceptor**

In `core/interceptors/response.interceptor.ts`, line 28:

```typescript
// Change from:
const { data } = await axios.post<RefreshResponse>(`${baseURL}/identity/auth/refresh`, {
// To:
const { data } = await axios.post<RefreshResponse>(`/api/store/identity/auth/sessions/refresh`, {
```

Remove the `baseURL` variable usage for the refresh call — use an absolute path.

Also remove the now-unused line:
```typescript
const baseURL = originalRequest.baseURL || '/api'
```

- [ ] **Step 2: Fix second interceptor**

In `core/http/interceptors/response.interceptor.ts`, same change on line 28.

- [ ] **Step 3: Build and verify**

```bash
cd app/Storefront && pnpm build
```

- [ ] **Step 4: Commit**

```bash
git add -A app/Storefront/src/core/
git commit -m "fix(storefront): fix stale identity refresh path in both response interceptors"
```

---

### Task 7: Shipping — Fix missing prefix in constants

**Files:**
- Modify: `app/Storefront/src/features/shipping/types/constants/shipping.constants.ts`

- [ ] **Step 1: Fix shipping constants**

In `shipping.constants.ts`:

```typescript
export const SHIPPING_ENDPOINTS = {
  METHODS: '/api/storefront/shipping/methods',
  METHOD: (id: string) => `/api/storefront/shipping/methods/${id}`,
  RATES: '/api/storefront/shipping/rates',
} as const
```

Change `'/shipping/methods'` → `'/api/storefront/shipping/methods'`.

- [ ] **Step 2: Build and verify**

```bash
cd app/Storefront && pnpm build
```

- [ ] **Step 3: Commit**

```bash
git add -A app/Storefront/src/features/shipping/
git commit -m "fix(storefront): add missing /api/storefront/ prefix to shipping constants"
```
