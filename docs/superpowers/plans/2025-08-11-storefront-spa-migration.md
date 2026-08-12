# Store SPA Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Update all Store SPA API client files to call new backend routes, use correct HTTP methods (`patch()` for PATCH, `get()` for GET reads), and drop old ordering/billing prefixes.

**Architecture:** 7 API service files updated, 2 files deleted (already done in catalog stock plan), 1 type extended (already done). Frontend must compile and pass all unit tests after all backend route changes. Import `patch` from shared API client — it already exists in `app/Store/src/shared/api/client.ts`.

**Tech Stack:** Vue 3, TypeScript 6, pnpm, Vitest, Zod

## Global Constraints

- `pnpm run lint && pnpm run test:unit && pnpm run build` must pass
- Use `patch()` for PATCH endpoints (already exists in shared API client)
- Use `get()` for GET endpoints (not `post()`)
- Use `del()` for DELETE endpoints (not `post()`)
- All route strings must match backend `public const string Route` values exactly

---

### Task 1: Update Cart API Service (cartApi.ts)

**Files:**
- Modify: `app/Store/src/features/ordering/services/cartApi.ts`

- [ ] **Step 1: Update BASE and all method calls**

```typescript
// BEFORE
private static readonly BASE = '/api/storefront/ordering/cart'

// AFTER
private static readonly BASE = '/api/storefront/cart'
```

Update `emptyCart`:
```typescript
// BEFORE
static async emptyCart(): Promise<Result<null>> {
  return await post<Result<null>>(`${this.BASE}/empty`)
}

// AFTER
static async emptyCart(): Promise<Result<null>> {
  return await del<Result<null>>(`${this.BASE}/items`)
}
```

No other method changes — `get`, `post`, `del` calls on `this.BASE`, `this.BASE/items`, `this.BASE/associate` are correct. The BASE change covers all.

- [ ] **Step 2: Run tests**

```bash
cd app/Store && pnpm run test:unit -- --run cartApi
```

If tests mock routes, update mock URLs to `/api/storefront/cart`.

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/features/ordering/services/cartApi.ts
git commit -m "refactor(store-spa): update cartApi to /storefront/cart routes

BASE changed from /ordering/cart to /cart.
emptyCart changed from post(/empty) to del(/items)."
```

### Task 2: Update Checkout API Service (checkoutApi.ts)

**Files:**
- Modify: `app/Store/src/features/ordering/services/checkoutApi.ts`

- [ ] **Step 1: Update all routes**

```typescript
// BEFORE
static async updateCheckout(req): Promise<Result<void>> {
  return await put<Result<void>>('/api/storefront/ordering/cart', req)
}
static async selectShippingRate(req): Promise<Result<void>> {
  return await post<Result<void>>('/api/storefront/ordering/cart/shipping-rate', req)
}
static async validateCheckout(): Promise<Result<void>> {
  return await post<Result<void>>('/api/storefront/ordering/cart/validate')
}
static async createPaymentIntent(req): Promise<Result<PaymentIntentResponse>> {
  const result = await post<Result<PaymentIntentResponse>>('/api/storefront/billing/payments/create-intent', req)
}
static async placeOrder(req): Promise<Result<PlaceOrderResponse>> {
  const result = await post<Result<PlaceOrderResponse>>('/api/storefront/ordering/cart/checkout', req)
}

// AFTER
static async updateCheckout(req): Promise<Result<void>> {
  return await patch<Result<void>>('/api/storefront/cart', req)      // PUT → PATCH
}
static async selectShippingRate(req): Promise<Result<void>> {
  return await patch<Result<void>>('/api/storefront/cart/shipping-rate', req)  // POST → PATCH
}
static async validateCheckout(): Promise<Result<void>> {
  return await get<Result<void>>('/api/storefront/cart/checkout')    // POST → GET, validate → checkout
}
static async createPaymentIntent(req): Promise<Result<PaymentIntentResponse>> {
  const result = await post<Result<PaymentIntentResponse>>('/api/storefront/cart/payment/intent', req)  // billing → cart
}
static async placeOrder(req): Promise<Result<PlaceOrderResponse>> {
  const result = await post<Result<PlaceOrderResponse>>('/api/storefront/cart/checkout', req)  // ordering/cart → cart
}
```

- [ ] **Step 2: Add import for `patch`**

```typescript
import { post, put, patch, get } from '@/shared/api/client'
```

Verify `patch` exists in `client.ts`. It already does (line 53: `export async function patch<T>`).

- [ ] **Step 3: Run tests**

```bash
cd app/Store && pnpm run test:unit
```

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/features/ordering/services/checkoutApi.ts
git commit -m "refactor(store-spa): update checkoutApi to /cart routes + PATCH

All routes moved from /ordering/cart to /cart.
Payment intent moved from /billing/payments to /cart/payment/intent.
PUT → PATCH for updateCheckout, POST → PATCH for shipping rate.
POST /validate → GET /checkout."
```

### Task 3: Update Orders + Shipping + Payment API Services

**Files:**
- Modify: `app/Store/src/features/ordering/services/orderApi.ts`
- Modify: `app/Store/src/features/shipping/services/shippingApi.ts`
- Modify: `app/Store/src/features/payment/services/paymentApi.ts`
- Modify: `app/Store/src/features/inventory/services/cartReservationApi.ts`

- [ ] **Step 1: Update orderApi.ts**

```typescript
// BEFORE
private static readonly BASE = '/api/storefront/ordering/orders'
static async cancelOrder(id: string): Promise<Result<null>> {
  return await put<Result<null>>(`${this.BASE}/${id}/cancel`)
}

// AFTER
private static readonly BASE = '/api/storefront/orders'
static async cancelOrder(id: string): Promise<Result<null>> {
  return await post<Result<null>>(`${this.BASE}/${id}/cancel`)  // PUT → POST
}
```

- [ ] **Step 2: Update shippingApi.ts**

Current file only has `getShippingMethods` and `getShippingRates` (both GET) — there is NO existing POST-based calculate function to remove. Add the GET-based calculate function:

```typescript
import { get } from '@/shared/api/client'

export function calculateShipping(shippingMethodId: string, orderId: string): Promise<Result<ShippingCost>> {
  return get<Result<ShippingCost>>(`/api/storefront/shipping/calculate?shippingMethodId=${shippingMethodId}&orderId=${orderId}`)
}
```

Verify `get` is exported from `@/shared/api/client` (it is). Add `ShippingCost` to the shipping types if it doesn't exist yet (`../types/shipping`).

- [ ] **Step 3: Update paymentApi.ts**

```typescript
// BEFORE
export function confirmPayment(paymentId: string): Promise<Result<ConfirmPaymentResponse>> {
  return post<Result<ConfirmPaymentResponse>>(`/api/storefront/billing/payments/confirm/${paymentId}`)
}

// AFTER
export function confirmPayment(paymentId: string): Promise<Result<ConfirmPaymentResponse>> {
  return post<Result<ConfirmPaymentResponse>>(`/api/storefront/cart/payment/intent/${paymentId}/confirm`)
}
```

Keep `getPaymentMethods` and `createSetupIntent` — their routes are unchanged.

- [ ] **Step 4: Update cartReservationApi.ts**

```typescript
// BEFORE
'/api/storefront/inventory/cart/reserve'
'/api/storefront/inventory/cart/reserve'  // GET
`/api/storefront/inventory/cart/reserve/${reservationId}`  // DELETE

// AFTER
'/api/storefront/inventory/stock-reservations'            // POST
'/api/storefront/inventory/stock-reservations'            // GET
`/api/storefront/inventory/stock-reservations/${reservationId}`  // DELETE
```

- [ ] **Step 5: Run tests**

```bash
cd app/Store && pnpm run test:unit
```

- [ ] **Step 6: Commit**

```bash
git add app/Store/src/features/ordering/services/orderApi.ts
git add app/Store/src/features/shipping/services/shippingApi.ts
git add app/Store/src/features/payment/services/paymentApi.ts
git add app/Store/src/features/inventory/services/cartReservationApi.ts
git commit -m "refactor(store-spa): update orders/shipping/payment/inventory routes

OrderApi: /ordering/orders → /orders, cancel PUT → POST
ShippingApi: calculate POST → GET with query params
PaymentApi: confirm under /cart/payment/intent/{id}/confirm
CartReservationApi: /cart/reserve → /stock-reservations"
```

### Task 4: Full SPA Build + Test

- [ ] **Step 1: Lint**

```bash
cd app/Store && pnpm run lint
```

- [ ] **Step 2: Unit tests**

```bash
cd app/Store && pnpm run test:unit
```

- [ ] **Step 3: Build**

```bash
cd app/Store && pnpm run build
```

- [ ] **Step 4: Verify no old route strings remain**

```bash
rg "ordering/cart|ordering/orders|billing/payments/create-intent|billing/payments/confirm|billing/payments/status|inventory/cart/reserve" app/Store/src/ --no-heading
```

Expected: zero matches. If any remain, fix them.

```bash
rg "api/storefront/cart|api/storefront/orders|api/storefront/inventory/stock-reservations" app/Store/src/ --no-heading
```

Expected: matches in cartApi.ts, checkoutApi.ts, orderApi.ts, paymentApi.ts, cartReservationApi.ts.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "chore: verify SPA builds and all routes migrated to new prefixes"
```
