# Wire Disconnected Backend Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Hook up 6 dead API functions to frontend components. Connect inventory, shipping, and error handling.

**Architecture:** Each task wires one API function to its intended consumer. Tasks are independent.

**Tech Stack:** Vue 3, Pinia, TypeScript, PrimeVue 5

## Global Constraints

- Warnings-as-errors: any TypeScript/lint warning fails build
- All API calls return `Result<T>` — check `.isSuccess` before use
- Inventory APIs use `X-Cart-Token` header
- Run `pnpm run lint` and `pnpm run test:unit` after each task

---

## File Structure

| File | Action | Purpose |
|------|--------|---------|
| `features/inventory/services/availabilityApi.ts` | READ | Verify API exists |
| `features/inventory/services/cartReservationApi.ts` | READ | Verify API exists |
| `features/catalog/views/ProductDetailView.vue` | MODIFY | Wire availability check |
| `features/ordering/stores/cartStore.ts` | MODIFY | Wire reservation lifecycle |
| `features/ordering/components/CheckoutStepDelivery.vue` | MODIFY | Wire shipping rates |
| `shared/composables/useApiErrorHandler.ts` | READ | Verify composable exists |
| Multiple view files | MODIFY | Wire error handler |

---

## Tasks

### Task 1: Wire availability check on ProductDetail

**Files:**
- Modify: `app/Store/src/features/catalog/views/ProductDetailView.vue`
- Read: `app/Store/src/features/inventory/services/availabilityApi.ts`

**Interfaces:**
- Consumes: `checkAvailability(variantId)` from availabilityApi
- Produces: Stock per-location display

- [ ] **Step 1: Read availabilityApi.ts**

Read `app/Store/src/features/inventory/services/availabilityApi.ts`. Verify `checkAvailability` function exists and its signature.

- [ ] **Step 2: Read ProductDetailView.vue**

Read `app/Store/src/features/catalog/views/ProductDetailView.vue`. Find where stock info is displayed (around line 39-56).

- [ ] **Step 3: Import availability API**

Add import:

```typescript
import { checkAvailability } from '@/features/inventory/services/availabilityApi'
```

- [ ] **Step 4: Add availability state**

```typescript
const availability = ref<{ locationName: string; availableCount: number }[]>([])
```

- [ ] **Step 5: Fetch availability on variant change**

In the `watch` or after `selectedVariantId` changes, add:

```typescript
if (selectedVariantId.value) {
  const result = await checkAvailability(selectedVariantId.value)
  if (result.isSuccess) availability.value = result.value
}
```

- [ ] **Step 6: Display availability in template**

After the stock badge section, add:

```vue
<div v-if="availability.length > 0" class="mt-2 space-y-1">
  <p v-for="loc in availability" :key="loc.locationName" class="text-xs text-stone-500">
    {{ loc.locationName }}: {{ loc.availableCount }} in stock
  </p>
</div>
```

- [ ] **Step 7: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 8: Run tests**

```bash
cd app/Store && pnpm run test:unit
```

Expected: PASS

- [ ] **Step 9: Commit**

```bash
cd app/Store && git add src/features/catalog/views/ProductDetailView.vue
git commit -m "feat(catalog): show per-location stock availability on product detail"
```

### Task 2: Wire cart reservation lifecycle

**Files:**
- Modify: `app/Store/src/features/ordering/stores/cartStore.ts`
- Read: `app/Store/src/features/inventory/services/cartReservationApi.ts`

**Interfaces:**
- Consumes: `reserveStock`, `releaseReservation`, `getCartReservations` from cartReservationApi
- Produces: Reservation lifecycle in cart store

- [ ] **Step 1: Read cartReservationApi.ts**

Read `app/Store/src/features/inventory/services/cartReservationApi.ts`. Verify all 3 functions exist.

- [ ] **Step 2: Read cartStore.ts**

Read `app/Store/src/features/ordering/stores/cartStore.ts`. Find `addItem`, `removeItem`, `clearCart`, `fetchCart` actions.

- [ ] **Step 3: Import reservation API**

Add import:

```typescript
import { reserveStock, releaseReservation, getCartReservations } from '@/features/inventory/services/cartReservationApi'
```

- [ ] **Step 4: Add reservation on addItem**

After `addItem` succeeds, add:

```typescript
await reserveStock({ variantId, quantity, cartToken: getCartToken() })
```

- [ ] **Step 5: Add release on removeItem**

Before `removeItem` API call, find the reservation ID and release:

```typescript
const reservation = reservations.value.find(r => r.variantId === variantId)
if (reservation) await releaseReservation(reservation.id)
```

- [ ] **Step 6: Add fetch on fetchCart**

After `fetchCart` succeeds, add:

```typescript
const reservationsResult = await getCartReservations()
if (reservationsResult.isSuccess) reservations.value = reservationsResult.items
```

- [ ] **Step 7: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 8: Run tests**

```bash
cd app/Store && pnpm run test:unit
```

Expected: PASS

- [ ] **Step 9: Commit**

```bash
cd app/Store && git add src/features/ordering/stores/cartStore.ts
git commit -m "feat(ordering): wire cart stock reservation lifecycle"
```

### Task 3: Wire shipping rates with delivery range

**Files:**
- Modify: `app/Store/src/features/ordering/components/CheckoutStepDelivery.vue`
- Read: `app/Store/src/features/shipping/services/shippingApi.ts`

**Interfaces:**
- Consumes: `getShippingRates()` from shippingApi
- Produces: Delivery range display per rate

- [ ] **Step 1: Read shippingApi.ts**

Read `app/Store/src/features/shipping/services/shippingApi.ts`. Verify `getShippingRates` exists and returns `ShippingRate` with `deliveryRange`.

- [ ] **Step 2: Read CheckoutStepDelivery.vue**

Read `app/Store/src/features/ordering/components/CheckoutStepDelivery.vue`.

- [ ] **Step 3: Import getShippingRates**

Add import:

```typescript
import { getShippingRates } from '@/features/shipping/services/shippingApi'
```

- [ ] **Step 4: Add rates state**

```typescript
const rates = ref<Map<string, string>>(new Map())
```

- [ ] **Step 5: Fetch rates after methods load**

After methods load, fetch rates for each method:

```typescript
const ratesResult = await getShippingRates()
if (ratesResult.isSuccess) {
  for (const rate of ratesResult.items) {
    if (rate.deliveryRange) rates.value.set(rate.shippingMethodId, rate.deliveryRange)
  }
}
```

- [ ] **Step 6: Display delivery range**

In template, after method name, add:

```vue
<p v-if="rates.get(method.id)" class="text-xs text-stone-500">
  Est. delivery: {{ rates.get(method.id) }}
</p>
```

- [ ] **Step 7: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 8: Run tests**

```bash
cd app/Store && pnpm run test:unit
```

Expected: PASS

- [ ] **Step 9: Commit**

```bash
cd app/Store && git add src/features/ordering/components/CheckoutStepDelivery.vue
git commit -m "feat(checkout): show delivery range per shipping rate"
```

### Task 4: Wire centralized error handler

**Files:**
- Read: `app/Store/src/shared/composables/useApiErrorHandler.ts`
- Modify: Multiple view files

**Interfaces:**
- Consumes: `useApiErrorHandler()` composable
- Produces: Consistent error handling

- [ ] **Step 1: Read useApiErrorHandler.ts**

Read `app/Store/src/shared/composables/useApiErrorHandler.ts`. Verify `handleError` and `handleResult` methods exist.

- [ ] **Step 2: Identify views with ad-hoc error handling**

```bash
cd app/Store && grep -r "notify.error\|notifyError" src/features/*/views/*.vue -l
```

- [ ] **Step 3: Wire error handler in each view**

For each view file found:
1. Add `import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'`
2. Add `const { handleError } = useApiErrorHandler()`
3. Replace `notify.error(...)` calls with `handleError(error)`

- [ ] **Step 4: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 5: Run tests**

```bash
cd app/Store && pnpm run test:unit
```

Expected: PASS

- [ ] **Step 6: Commit**

```bash
cd app/Store && git add -A
git commit -m "refactor: wire useApiErrorHandler across all views"
```
