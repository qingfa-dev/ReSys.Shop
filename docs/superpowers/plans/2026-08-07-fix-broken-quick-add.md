# Fix Broken Quick Add Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add `@add-to-cart` event listeners to 3 row components that define the emit but never receive it.

**Architecture:** One-line fix per component. Import cartStore in parent views, wire the emit.

**Tech Stack:** Vue 3, Pinia, PrimeVue 5

## Global Constraints

- Warnings-as-errors: any TypeScript/lint warning fails build
- Cart store must be imported in each parent view
- Toast notification should show on successful add

---

## File Structure

| File | Action | Purpose |
|------|--------|---------|
| `features/catalog/views/HomeView.vue` | MODIFY | Wire addToCart on FeaturedProductsRow + RecentlyViewedRow |
| `features/catalog/views/ProductDetailView.vue` | MODIFY | Wire addToCart on RelatedProductsRow |

---

## Tasks

### Task 1: Wire Quick Add on HomeView

**Files:**
- Modify: `app/Store/src/features/catalog/views/HomeView.vue`

**Interfaces:**
- Consumes: `useCartStore` from `ordering/stores/cartStore`
- Produces: `@add-to-cart` event handled

- [ ] **Step 1: Read HomeView.vue**

Read `app/Store/src/features/catalog/views/HomeView.vue`. Find where `FeaturedProductsRow` and `RecentlyViewedRow` are rendered.

- [ ] **Step 2: Import cartStore**

Add import at top of script:

```typescript
import { useCartStore } from '@/features/ordering/stores/cartStore'
```

Add store reference:

```typescript
const cart = useCartStore()
```

- [ ] **Step 3: Add @add-to-cart to FeaturedProductsRow**

Find `<FeaturedProductsRow` in template. Add:

```vue
@add-to-cart="(id) => cart.addItem(id)"
```

- [ ] **Step 4: Add @add-to-cart to RecentlyViewedRow**

Find `<RecentlyViewedRow` in template. Add:

```vue
@add-to-cart="(id) => cart.addItem(id)"
```

- [ ] **Step 5: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 6: Run tests**

```bash
cd app/Store && pnpm run test:unit
```

Expected: PASS

- [ ] **Step 7: Commit**

```bash
cd app/Store && git add src/features/catalog/views/HomeView.vue
git commit -m "fix(catalog): wire @add-to-cart on FeaturedProductsRow and RecentlyViewedRow"
```

### Task 2: Wire Quick Add on ProductDetailView

**Files:**
- Modify: `app/Store/src/features/catalog/views/ProductDetailView.vue`

**Interfaces:**
- Consumes: `useCartStore` (already imported)
- Produces: `@add-to-cart` event handled

- [ ] **Step 1: Read ProductDetailView.vue**

Read `app/Store/src/features/catalog/views/ProductDetailView.vue`. Find where `RelatedProductsRow` is rendered (around line 206).

- [ ] **Step 2: Verify cartStore already imported**

Check if `useCartStore` is already imported. If not, add it.

- [ ] **Step 3: Add @add-to-cart to RelatedProductsRow**

Find `<RelatedProductsRow` in template. Add:

```vue
@add-to-cart="(id) => cart.addItem(id)"
```

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
cd app/Store && git add src/features/catalog/views/ProductDetailView.vue
git commit -m "fix(catalog): wire @add-to-cart on RelatedProductsRow"
```
