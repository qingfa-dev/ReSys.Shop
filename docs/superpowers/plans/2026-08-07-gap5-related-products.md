# Gap 5: Related Products Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Wire the existing `GET /api/storefront/products/related` API to a new `RelatedProductsRow` component on ProductDetailView.

**Architecture:** New presentational component matching `SimilarProductsRow.vue` pattern. Fetch triggered in `ProductDetailView.vue` on mount. Reuses existing `ProductCard` component.

**Tech Stack:** Vue 3, Pinia, existing `productApi.ts` functions

## Global Constraints

- Warnings-as-errors: any TypeScript/lint warning fails build
- Follow existing component patterns (see `SimilarProductsRow.vue`)
- All API calls return `Result<T>` objects — check `.isSuccess` before using `.items`

---

## File Structure

| File | Action | Purpose |
|------|--------|---------|
| `app/Store/src/features/catalog/components/RelatedProductsRow.vue` | CREATE | Horizontal scrollable product row |
| `app/Store/src/features/catalog/views/ProductDetailView.vue` | MODIFY | Fetch + render related products |
| `app/Store/src/features/catalog/services/productApi.ts` | READ | Verify `getRelatedProducts` exists |

---

## Tasks

### Task 1: Verify existing API function

**Files:**
- Read: `app/Store/src/features/catalog/services/productApi.ts`

**Interfaces:**
- Consumes: `getRelatedProducts(productId: string)` — should return `Promise<PagedResult<StoreProductListItemResponse>>`
- Produces: No changes — verification only

- [ ] **Step 1: Read productApi.ts**

Verify `getRelatedProducts` function exists at `app/Store/src/features/catalog/services/productApi.ts` lines 26-31. It should call `GET api/storefront/products/related?productId=...`.

- [ ] **Step 2: Verify return type**

The function should return a `PagedResult<StoreProductListItemResponse>` with `.isSuccess`, `.items`, `.totalCount` properties.

### Task 2: Create RelatedProductsRow.vue

**Files:**
- Create: `app/Store/src/features/catalog/components/RelatedProductsRow.vue`

**Interfaces:**
- Consumes: `products: StoreProductListItemResponse[]`, `loading?: boolean`
- Produces: No exports — presentational component only

- [ ] **Step 1: Create the component file**

Create `app/Store/src/features/catalog/components/RelatedProductsRow.vue`:

```vue
<script setup lang="ts">
import type { StoreProductListItemResponse } from '../types/product'
import ProductCard from './ProductCard.vue'

defineProps<{
  products: StoreProductListItemResponse[]
  loading?: boolean
}>()

defineEmits<{ addToCart: [variantId: string] }>()
</script>
<template>
  <!-- Section: Related Products -->
  <div>
    <h2 class="text-xl font-bold text-stone-900 mb-4">You Might Also Like</h2>
    <div class="flex gap-4 overflow-x-auto pb-2">
      <div
        v-for="item in products"
        :key="item.id"
        class="w-64 shrink-0"
      >
        <ProductCard :product="item" @add-to-cart="(id) => $emit('addToCart', id)" />
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 3: Wire into ProductDetailView

**Files:**
- Modify: `app/Store/src/features/catalog/views/ProductDetailView.vue:4,19,62-78,190-195`

**Interfaces:**
- Consumes: `getRelatedProducts` from `productApi.ts`
- Produces: Renders `RelatedProductsRow` below `SimilarProductsRow`

- [ ] **Step 1: Add import for getRelatedProducts**

Edit `app/Store/src/features/catalog/views/ProductDetailView.vue`. On line 4, add `getRelatedProducts` to the import:

```typescript
import { getProductBySlug, getSimilarProducts, getRelatedProducts } from '../services/productApi'
```

- [ ] **Step 2: Add import for RelatedProductsRow**

After line 9 (`import SimilarProductsRow from '../components/SimilarProductsRow.vue'`), add:

```typescript
import RelatedProductsRow from '../components/RelatedProductsRow.vue'
```

- [ ] **Step 3: Add related products state**

After line 19 (`const similar = ref<StoreProductListItemResponse[]>([])`), add:

```typescript
const related = ref<StoreProductListItemResponse[]>([])
const relatedLoading = ref(true)
```

- [ ] **Step 4: Fetch related products in loadProduct**

After line 78 (`if (simResult.isSuccess) similar.value = simResult.items`), add:

```typescript
    relatedLoading.value = true
    const relResult = await getRelatedProducts(result.value.id)
    if (relResult.isSuccess) related.value = relResult.items
    relatedLoading.value = false
```

- [ ] **Step 5: Reset related on product change**

After line 62 (`similar.value = []`), add:

```typescript
    related.value = []
```

- [ ] **Step 6: Render RelatedProductsRow in template**

After line 195 (the closing `</SimilarProductsRow>` tag), add:

```vue
      <!-- Section: Related Products -->
      <RelatedProductsRow
        v-if="related.length > 0"
        :products="related"
        class="mt-16"
      />
```

- [ ] **Step 7: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 8: Run unit tests**

```bash
cd app/Store && pnpm run test:unit
```

Expected: PASS

- [ ] **Step 9: Commit**

```bash
cd app/Store && git add src/features/catalog/components/RelatedProductsRow.vue src/features/catalog/views/ProductDetailView.vue
git commit -m "feat(catalog): wire related products to product detail page"
```
