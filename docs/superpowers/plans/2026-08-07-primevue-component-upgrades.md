# PrimeVue Component Upgrades Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace custom implementations with PrimeVue v5 components for better UX and less maintenance.

**Architecture:** 10 component upgrades. Each task replaces one custom component with its PrimeVue equivalent. Tasks are independent.

**Tech Stack:** Vue 3, PrimeVue 5 (Aura theme), Tailwind CSS v4

## Global Constraints

- Warnings-as-errors: any TypeScript/lint warning fails build
- PrimeVue components auto-import via `PrimeVueResolver` — no manual imports needed
- Follow existing `tailwindcss-primeui` patterns for styling
- Run `pnpm run lint` and `pnpm run test:unit` after each task

---

## File Structure

| File | Action | Purpose |
|------|--------|---------|
| `features/catalog/components/ProductGallery.vue` | MODIFY | Replace with Galleria |
| `features/catalog/components/FeaturedProductsRow.vue` | MODIFY | Replace with Carousel |
| `features/catalog/components/SimilarProductsRow.vue` | MODIFY | Replace with Carousel |
| `features/catalog/components/RelatedProductsRow.vue` | MODIFY | Replace with Carousel |
| `features/ordering/components/CheckoutStepper.vue` | MODIFY | Replace with Steps |
| `features/catalog/components/ProductBadge.vue` | MODIFY | Replace with Tag |
| `shared/components/StatusTag.vue` | MODIFY | Replace with Tag |
| `shared/components/ScrollToTop.vue` | MODIFY | Replace with ScrollTop |
| `app/components/layout/AppHeader.vue` | MODIFY | Add Badge to cart icon |
| `features/catalog/components/FilterSidebar.vue` | MODIFY | Add Chip for active filters |
| `features/catalog/views/ProductDetailView.vue` | MODIFY | Replace Accordion with TabView |

---

## Tasks

### Task 1: ProductGallery → Galleria

**Files:**
- Modify: `app/Store/src/features/catalog/components/ProductGallery.vue`

**Interfaces:**
- Consumes: `images: StoreProductImageResponse[]`, `alt: string`
- Produces: No change to interface

- [ ] **Step 1: Read current ProductGallery.vue**

Read the file. Note the template structure (main image + thumbnail strip).

- [ ] **Step 2: Replace with Galleria**

Replace the entire template with:

```vue
<template>
  <!-- Section: Product Gallery -->
  <Galleria
    :value="images"
    :num-visible="5"
    :show-thumbnails="images.length > 1"
    :show-item-navigators="images.length > 1"
    container-class="rounded-xl overflow-hidden"
  >
    <template #item="{ item }">
      <img :src="item.url" :alt="item.alt ?? alt" class="w-full object-cover" />
    </template>
    <template #thumbnail="{ item }">
      <img :src="item.url" :alt="item.alt ?? alt" class="w-20 h-20 object-cover rounded-lg" />
    </template>
  </Galleria>
  <div v-if="images.length === 0" class="aspect-square bg-stone-100 rounded-xl flex items-center justify-center text-stone-400">
    <i class="pi pi-image text-6xl" />
  </div>
</template>
```

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 4: Commit**

```bash
cd app/Store && git add src/features/catalog/components/ProductGallery.vue
git commit -m "feat(catalog): replace ProductGallery with PrimeVue Galleria"
```

### Task 2: Product scrollers → Carousel

**Files:**
- Modify: `app/Store/src/features/catalog/components/FeaturedProductsRow.vue`
- Modify: `app/Store/src/features/catalog/components/SimilarProductsRow.vue`
- Modify: `app/Store/src/features/catalog/components/RelatedProductsRow.vue`

**Interfaces:**
- Consumes: Same props as before
- Produces: No change to interface

- [ ] **Step 1: Read FeaturedProductsRow.vue**

Read the file. Note the custom scroll container.

- [ ] **Step 2: Replace with Carousel**

Replace template with:

```vue
<template>
  <!-- Section: Featured Products -->
  <div>
    <h2 class="text-xl font-bold text-stone-900 mb-4">Featured Products</h2>
    <Carousel
      :value="products"
      :num-visible="4"
      :num-scroll="1"
      :show-indicators="false"
      responsive-options="[{ breakpoint: '768px', numVisible: 2 }, { breakpoint: '560px', numVisible: 1 }]"
    >
      <template #item="{ data }">
        <div class="px-2">
          <ProductCard :product="data" @add-to-cart="(id) => $emit('addToCart', id)" />
        </div>
      </template>
    </Carousel>
  </div>
</template>
```

- [ ] **Step 3: Apply same pattern to SimilarProductsRow and RelatedProductsRow**

Same Carousel wrapper, different header text.

- [ ] **Step 4: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 5: Commit**

```bash
cd app/Store && git add src/features/catalog/components/FeaturedProductsRow.vue src/features/catalog/components/SimilarProductsRow.vue src/features/catalog/components/RelatedProductsRow.vue
git commit -m "feat(catalog): replace product scrollers with PrimeVue Carousel"
```

### Task 3: CheckoutStepper → Steps

**Files:**
- Modify: `app/Store/src/features/ordering/components/CheckoutStepper.vue`

**Interfaces:**
- Consumes: `currentStep: number`
- Produces: No change to interface

- [ ] **Step 1: Read CheckoutStepper.vue**

Read the file. Note the manual step rendering.

- [ ] **Step 2: Replace with PrimeVue Steps**

```vue
<script setup lang="ts">
defineProps<{ currentStep: number }>()

const steps = [
  { label: 'Address' },
  { label: 'Delivery' },
  { label: 'Payment' },
  { label: 'Confirm' },
  { label: 'Complete' },
]
</script>
<template>
  <Steps :model="steps" :active-index="currentStep - 1" class="mb-8" />
</template>
```

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 4: Commit**

```bash
cd app/Store && git add src/features/ordering/components/CheckoutStepper.vue
git commit -m "feat(checkout): replace CheckoutStepper with PrimeVue Steps"
```

### Task 4: ProductBadge → Tag

**Files:**
- Modify: `app/Store/src/features/catalog/components/ProductBadge.vue`

**Interfaces:**
- Consumes: `variant: 'new' | 'sale'`
- Produces: No change

- [ ] **Step 1: Read ProductBadge.vue**

Read the file.

- [ ] **Step 2: Replace with Tag**

```vue
<script setup lang="ts">
defineProps<{ variant: 'new' | 'sale' }>()
</script>
<template>
  <Tag :severity="variant === 'new' ? 'success' : 'danger'" :value="variant === 'new' ? 'New' : 'Sale'" class="absolute top-3 left-3 z-10" />
</template>
```

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 4: Commit**

```bash
cd app/Store && git add src/features/catalog/components/ProductBadge.vue
git commit -m "feat(catalog): replace ProductBadge with PrimeVue Tag"
```

### Task 5: StatusTag → Tag

**Files:**
- Modify: `app/Store/src/shared/components/StatusTag.vue`

**Interfaces:**
- Consumes: `status: string`
- Produces: No change

- [ ] **Step 1: Read StatusTag.vue**

Read the file. Note the severity mapping logic.

- [ ] **Step 2: Replace with Tag**

```vue
<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{ status: string }>()

const severity = computed(() => {
  const map: Record<string, string> = {
    Placed: 'info',
    Shipped: 'warn',
    Delivered: 'success',
    Canceled: 'danger',
    Draft: 'secondary',
    Expired: 'secondary',
    in_stock: 'success',
    low_stock: 'warn',
    out_of_stock: 'danger',
  }
  return map[props.status] ?? 'secondary'
})
</script>
<template>
  <Tag :severity="severity" :value="status" />
</template>
```

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 4: Commit**

```bash
cd app/Store && git add src/shared/components/StatusTag.vue
git commit -m "feat(shared): replace StatusTag with PrimeVue Tag"
```

### Task 6: ScrollToTop → ScrollTop

**Files:**
- Modify: `app/Store/src/shared/components/ScrollToTop.vue`

**Interfaces:**
- Consumes: None
- Produces: No change

- [ ] **Step 1: Read ScrollToTop.vue**

Read the file.

- [ ] **Step 2: Replace with ScrollTop**

```vue
<template>
  <ScrollTop :threshold="500" icon="pi pi-arrow-up" />
</template>
```

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 4: Commit**

```bash
cd app/Store && git add src/shared/components/ScrollToTop.vue
git commit -m "feat(shared): replace ScrollToTop with PrimeVue ScrollTop"
```

### Task 7: Cart badge → Badge

**Files:**
- Modify: `app/Store/src/app/components/layout/AppHeader.vue`

**Interfaces:**
- Consumes: `cart.itemCount`
- Produces: Badge on cart icon

- [ ] **Step 1: Read AppHeader.vue**

Read the file. Find the cart icon section.

- [ ] **Step 2: Add Badge**

On the cart icon, add:

```vue
<Badge v-if="cart.itemCount > 0" :value="cart.itemCount" class="absolute -top-2 -right-2" />
```

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 4: Commit**

```bash
cd app/Store && git add src/app/components/layout/AppHeader.vue
git commit -m "feat(layout): add PrimeVue Badge to cart icon"
```

### Task 8: Active filter chips → Chip

**Files:**
- Modify: `app/Store/src/features/catalog/components/FilterSidebar.vue`

**Interfaces:**
- Consumes: `selectedTaxonIds`, `selectedOptionValueIds`
- Produces: Chip display with remove

- [ ] **Step 1: Read FilterSidebar.vue**

Read the file. Find the "Clear all" button section.

- [ ] **Step 2: Add active filter chips**

Before the "Clear all" button, add:

```vue
<div v-if="hasSelection" class="flex flex-wrap gap-2 mb-4">
  <Chip v-for="id in selectedTaxonIds" :key="id" :label="getTaxonName(id)" removable @remove="emit('toggleTaxon', id)" />
  <Chip v-for="id in selectedOptionValueIds" :key="id" :label="getOptionName(id)" removable @remove="emit('toggleOptionValue', id)" />
</div>
```

- [ ] **Step 3: Add helper functions**

```typescript
function getTaxonName(id: string): string {
  // Find taxon name from taxonomyGroups
  for (const group of props.taxonomyGroups) {
    const found = findTaxon(group.tree, id)
    if (found) return found.name
  }
  return id
}

function getOptionName(id: string): string {
  for (const type of props.optionTypes) {
    const found = type.values.find(v => v.id === id)
    if (found) return found.presentation ?? found.name
  }
  return id
}
```

- [ ] **Step 4: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 5: Commit**

```bash
cd app/Store && git add src/features/catalog/components/FilterSidebar.vue
git commit -m "feat(catalog): add PrimeVue Chip for active filter display"
```

### Task 9: Product detail tabs → TabView

**Files:**
- Modify: `app/Store/src/features/catalog/views/ProductDetailView.vue`

**Interfaces:**
- Consumes: Same product data
- Produces: Tabbed layout for description/details

- [ ] **Step 1: Read ProductDetailView.vue**

Read the file. Find the Accordion section (lines 179-186).

- [ ] **Step 2: Replace Accordion with TabView**

Replace the Accordion with:

```vue
<TabView>
  <TabPanel header="Description">
    <p class="text-stone-600">{{ product.description }}</p>
  </TabPanel>
  <TabPanel header="Details">
    <ProductDetailsInfo :product="product" />
  </TabPanel>
</TabView>
```

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 4: Commit**

```bash
cd app/Store && git add src/features/catalog/views/ProductDetailView.vue
git commit -m "feat(catalog): replace Accordion with TabView on product detail"
```
