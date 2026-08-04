# Storefront Feature Restoration — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restore 13 dropped legacy storefront features (rich footer, breadcrumbs, hero section, features strip, category grid, featured products, recently viewed, search overlay, product badges, size guide modal, product details info, notification bell, Terms/Privacy views) using real backend API endpoints.

**Architecture:** 6 independent batches. Each batch adds new Vue components following the existing feature-sliced pattern (`features/{module}/components/`). Components consume Spec A teal tokens via Tailwind utility classes. Data comes from real API calls to existing backend endpoints. No mock data. No new Pinia stores (reuse existing `catalogStore`, `cartStore`, `authStore`).

**Tech Stack:** Vue 3.5, TypeScript 6.0, PrimeVue 5 Aura preset, Tailwind CSS 4, Pinia 4.

## Global Constraints

- All new Vue components must follow Code Commenting Standard v3.0 (section comments in `<template>`, label comments in `<script setup>`)
- TypeScript `noUncheckedIndexedAccess: true` enforced
- `pnpm run type-check` — 0 errors after each task
- `pnpm run lint` — 0 violations after each task
- `pnpm run test:unit -- --run` — existing tests still pass after each task
- No mock data — all API calls use existing service functions in `features/*/services/*Api.ts`
- Color classes: `teal-*` for primary, `stone-*` for neutral surfaces/text. No hardcoded hex values in component templates
- All text content (labels, headings, descriptions) hardcoded as English strings (no i18n)
- Spec A (design tokens) must be complete before any Batch 2-6 tasks

---

### Task 1: Rewrite AppFooter with 4-column grid

**Files:**
- Modify: `app/Store/src/app/components/layout/AppFooter.vue`

**Interfaces:**
- Consumes: Spec A design tokens (via Tailwind `stone-*`, `teal-*` classes)
- Produces: renderless — footer renders as part of DefaultLayout

- [ ] **Step 1: Write the rewrite**

Write `app/Store/src/app/components/layout/AppFooter.vue`:

```vue
<template>
  <!-- Section: Footer -->
  <footer class="bg-white border-t border-stone-200 mt-auto">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <!-- Section: Link Grid -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
        <!-- Section: Brand Column -->
        <div class="lg:col-span-1">
          <router-link to="/" class="text-xl font-bold text-stone-900" style="font-family: 'Playfair Display', serif">
            ReSys.Shop
          </router-link>
          <p class="mt-3 text-sm text-stone-500">Discover your style with visual search. Upload an image, find your look.</p>
          <div class="flex items-center gap-4 mt-4">
            <a href="#" class="text-stone-400 hover:text-teal-600 transition-colors" aria-label="Instagram">
              <i class="pi pi-instagram text-lg" />
            </a>
            <a href="#" class="text-stone-400 hover:text-teal-600 transition-colors" aria-label="Twitter">
              <i class="pi pi-twitter text-lg" />
            </a>
            <a href="#" class="text-stone-400 hover:text-teal-600 transition-colors" aria-label="Facebook">
              <i class="pi pi-facebook text-lg" />
            </a>
          </div>
        </div>

        <!-- Section: Shop Links -->
        <div>
          <h3 class="text-sm font-semibold text-stone-900 uppercase tracking-wider mb-4">Shop</h3>
          <ul class="space-y-2">
            <li><router-link to="/shop" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">All Products</router-link></li>
            <li><router-link to="/shop?sort=newest" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">New Arrivals</router-link></li>
            <li><router-link to="/collections" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Collections</router-link></li>
            <li><a href="/shop?sort=price-asc" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Sale</a></li>
          </ul>
        </div>

        <!-- Section: Support Links -->
        <div>
          <h3 class="text-sm font-semibold text-stone-900 uppercase tracking-wider mb-4">Support</h3>
          <ul class="space-y-2">
            <li><a href="#" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Help Center</a></li>
            <li><a href="#" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Shipping Info</a></li>
            <li><a href="#" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Returns</a></li>
            <li><a href="#" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Size Guide</a></li>
          </ul>
        </div>

        <!-- Section: Company Links -->
        <div>
          <h3 class="text-sm font-semibold text-stone-900 uppercase tracking-wider mb-4">Company</h3>
          <ul class="space-y-2">
            <li><a href="#" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">About Us</a></li>
            <li><a href="#" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Careers</a></li>
            <li><router-link to="/privacy" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Privacy</router-link></li>
            <li><router-link to="/terms" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Terms</router-link></li>
          </ul>
        </div>
      </div>

      <!-- Section: Bottom Bar -->
      <div class="mt-12 pt-8 border-t border-stone-200 flex flex-col sm:flex-row justify-between items-center gap-4">
        <p class="text-sm text-stone-400">&copy; {{ new Date().getFullYear() }} ReSys.Shop. All rights reserved.</p>
        <div class="flex items-center gap-3 text-stone-400">
          <i class="pi pi-credit-card text-lg" title="Visa" />
          <i class="pi pi-credit-card text-lg" title="Mastercard" />
          <i class="pi pi-credit-card text-lg" title="Stripe" />
        </div>
      </div>
    </div>
  </footer>
</template>
```

- [ ] **Step 2: Run type-check + lint**

```bash
cd app/Store && pnpm run type-check && pnpm run lint
```
Expected: 0 errors, 0 violations.

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/app/components/layout/AppFooter.vue
git commit -m "feat(store): replace minimal footer with 4-column fashion layout"
```

---

### Task 2: Add breadcrumbs to Shop, Collections, and Checkout pages

**Files:**
- Modify: `app/Store/src/features/catalog/views/ShopView.vue`
- Modify: `app/Store/src/features/catalog/views/CollectionsView.vue`
- Modify: `app/Store/src/features/ordering/views/CheckoutView.vue`
- Modify: `app/Store/src/features/catalog/views/ProductDetailView.vue`

**Interfaces:**
- Consumes: Vue Router `route.path` for breadcrumb computation
- Produces: none — breadcrumbs render as page chrome

- [ ] **Step 1: Add breadcrumb to ShopView.vue**

Read `ShopView.vue`. Find the opening `<div class="max-w-7xl...">` (first element in template after the script). Insert immediately after it, before the flex container:

```vue
<!-- Section: Breadcrumb -->
<Breadcrumb :model="breadcrumbItems" class="mb-4" />
```

Add in `<script setup>`:
```ts
import { computed } from 'vue'

const breadcrumbItems = computed(() => [
  { label: 'Home', to: '/' },
  { label: 'Shop' },
])
```

- [ ] **Step 2: Add breadcrumb to CollectionsView.vue**

Read `CollectionsView.vue`. Insert same breadcrumb pattern, with items `[{ label: 'Home', to: '/' }, { label: 'Collections' }]`.

- [ ] **Step 3: Add breadcrumb to CheckoutView.vue**

Read `CheckoutView.vue`. Insert same pattern, with items `[{ label: 'Home', to: '/' }, { label: 'Cart', to: '/cart' }, { label: 'Checkout' }]`.

- [ ] **Step 4: Upgrade ProductDetailView breadcrumbs to `<Breadcrumb>`**

Read `ProductDetailView.vue`. Lines 100-106 currently have manual breadcrumbs (`<nav>` with `<router-link>`). Replace with:

```vue
<!-- Section: Breadcrumb -->
<Breadcrumb :model="breadcrumbItems" class="mb-4" />
```

Add computed:
```ts
const breadcrumbItems = computed(() => [
  { label: 'Home', to: '/' },
  { label: 'Shop', to: '/shop' },
  { label: product.value?.name ?? 'Product' },
])
```

Remove the old `<nav class="flex items-center gap-2...">` block (lines 100-106).

- [ ] **Step 5: Run type-check + lint for all 4 files**

```bash
cd app/Store && pnpm run type-check && pnpm run lint
```
Expected: 0 errors, 0 violations. Verify `<Breadcrumb>` from PrimeVue 5 resolves via auto-import.

- [ ] **Step 6: Commit**

```bash
git add app/Store/src/features/catalog/views/ShopView.vue app/Store/src/features/catalog/views/CollectionsView.vue app/Store/src/features/catalog/views/ProductDetailView.vue app/Store/src/features/ordering/views/CheckoutView.vue
git commit -m "feat(store): add breadcrumbs to Shop, Collections, Checkout, and ProductDetail pages"
```

---

### Task 3: Create useRecentlyViewed composable + tests

**Files:**
- Create: `app/Store/src/shared/composables/useRecentlyViewed.ts`
- Create: `app/Store/src/shared/composables/__tests__/useRecentlyViewed.spec.ts`

**Interfaces:**
- Produces: `useRecentlyViewed(maxItems?)` -> `{ items, add(product), clear() }`
- Consumes: localStorage

- [ ] **Step 1: Write useRecentlyViewed.ts**

Write `app/Store/src/shared/composables/useRecentlyViewed.ts`:

```ts
import { ref } from 'vue'

export interface RecentlyViewedItem {
  productId: string
  productName: string
  slug: string
  thumbnailUrl: string | null
  minPrice: number | null
  viewedAt: number
}

const STORAGE_KEY = 'recentlyViewed'
const DEFAULT_MAX = 10

let sharedItems: RecentlyViewedItem[] = []

function loadItems(): RecentlyViewedItem[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return []
    const parsed = JSON.parse(raw)
    if (!Array.isArray(parsed)) return []
    return parsed.filter(
      (item: unknown): item is RecentlyViewedItem =>
        typeof item === 'object' &&
        item !== null &&
        typeof (item as RecentlyViewedItem).productId === 'string',
    )
  } catch {
    return []
  }
}

function saveItems(items: RecentlyViewedItem[]): void {
  try { localStorage.setItem(STORAGE_KEY, JSON.stringify(items)) } catch { /* ignore */ }
}

export function useRecentlyViewed(maxItems = DEFAULT_MAX) {
  if (sharedItems.length === 0) {
    sharedItems = loadItems()
  }

  const items = ref<RecentlyViewedItem[]>(sharedItems)

  function add(product: RecentlyViewedItem): void {
    const idx = sharedItems.findIndex(i => i.productId === product.productId)
    if (idx >= 0) sharedItems.splice(idx, 1)
    sharedItems.push(product)
    if (sharedItems.length > maxItems) sharedItems.shift()
    items.value = [...sharedItems]
    saveItems(sharedItems)
  }

  function clear(): void {
    sharedItems = []
    items.value = []
    try { localStorage.removeItem(STORAGE_KEY) } catch { /* ignore */ }
  }

  return { items, add, clear }
}
```

- [ ] **Step 2: Write unit tests**

Write `app/Store/src/shared/composables/__tests__/useRecentlyViewed.spec.ts`:

```ts
import { describe, it, expect, beforeEach } from 'vitest'

const mockItem = {
  productId: 'p1',
  productName: 'Test Product',
  slug: 'test-product',
  thumbnailUrl: null,
  minPrice: 100000,
  viewedAt: Date.now(),
}

describe('useRecentlyViewed', () => {
  beforeEach(() => {
    localStorage.clear()
  })

  it('starts empty', async () => {
    const { useRecentlyViewed } = await import('@/shared/composables/useRecentlyViewed')
    const { items } = useRecentlyViewed()
    expect(items.value).toEqual([])
  })

  it('adds an item', async () => {
    const { useRecentlyViewed } = await import('@/shared/composables/useRecentlyViewed')
    const { items, add } = useRecentlyViewed()
    add({ ...mockItem })
    expect(items.value).toHaveLength(1)
    expect(items.value[0]?.productId).toBe('p1')
  })

  it('deduplicates by productId', async () => {
    const { useRecentlyViewed } = await import('@/shared/composables/useRecentlyViewed')
    const { items, add } = useRecentlyViewed()
    add({ ...mockItem })
    add({ ...mockItem, productName: 'Updated' })
    expect(items.value).toHaveLength(1)
    expect(items.value[0]?.productName).toBe('Updated')
  })

  it('evicts oldest when maxItems reached', async () => {
    const { useRecentlyViewed } = await import('@/shared/composables/useRecentlyViewed')
    const { items, add } = useRecentlyViewed(3)
    add({ ...mockItem, productId: 'p1', viewedAt: 1000 })
    add({ ...mockItem, productId: 'p2', viewedAt: 2000 })
    add({ ...mockItem, productId: 'p3', viewedAt: 3000 })
    add({ ...mockItem, productId: 'p4', viewedAt: 4000 })
    expect(items.value).toHaveLength(3)
    expect(items.value.map(i => i.productId)).toEqual(['p2', 'p3', 'p4'])
  })

  it('clear removes all items', async () => {
    const { useRecentlyViewed } = await import('@/shared/composables/useRecentlyViewed')
    const { items, add, clear } = useRecentlyViewed()
    add({ ...mockItem })
    clear()
    expect(items.value).toEqual([])
  })
})
```

- [ ] **Step 3: Run tests**

```bash
cd app/Store && pnpm run test:unit -- --run
```
Expected: all tests pass including 5 new useRecentlyViewed tests.

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/shared/composables/useRecentlyViewed.ts app/Store/src/shared/composables/__tests__/useRecentlyViewed.spec.ts
git commit -m "feat(store): add useRecentlyViewed composable with localStorage persistence"
```

---

### Task 4: Create ProductBadge component + integrate into ProductCard

**Files:**
- Create: `app/Store/src/features/catalog/components/ProductBadge.vue`
- Modify: `app/Store/src/features/catalog/components/ProductCard.vue`

**Interfaces:**
- Consumes: `StoreProductListItemResponse` type (has `availableOn` field)
- Produces: badge overlay on product card thumbnails

- [ ] **Step 1: Write ProductBadge.vue**

Write `app/Store/src/features/catalog/components/ProductBadge.vue`:

```vue
<script setup lang="ts">
defineProps<{ variant: 'new' | 'sale' }>()
</script>
<template>
  <span
    class="absolute top-3 left-3 z-10 text-xs font-semibold uppercase tracking-wide rounded-full px-2.5 py-1"
    :class="variant === 'new' ? 'bg-teal-600 text-white' : 'bg-red-500 text-white'"
  >
    {{ variant === 'new' ? 'New' : 'Sale' }}
  </span>
</template>
```

- [ ] **Step 2: Integrate into ProductCard.vue**

Read `ProductCard.vue`. Apply these changes:

**Add import** (after existing imports):
```ts
import { computed } from 'vue'
import ProductBadge from './ProductBadge.vue'
```

**Add computed** (inside `<script setup>`, after `displayPrice`):
```ts
const isNew = computed(() => {
  if (!props.product.availableOn) return false
  const diff = Date.now() - new Date(props.product.availableOn).getTime()
  return diff >= 0 && diff <= 14 * 24 * 60 * 60 * 1000
})
```

**Wrap thumbnail area** in `<div class="relative">`, add badge:

Find the `<router-link>` that starts with `class="block aspect-square bg-gray-100 relative overflow-hidden"`. Wrap it:
```vue
<div class="relative">
  <ProductBadge v-if="isNew" variant="'new'" />
  <router-link :to="`/products/${product.slug}`" class="block aspect-square bg-gray-100 relative overflow-hidden">
    <!-- existing thumbnail content unchanged -->
  </router-link>
</div>
```

Replace `bg-gray-100` with `bg-stone-100` in the thumbnail placeholder div for consistency with the new surface palette.

- [ ] **Step 3: Run type-check + lint**

```bash
cd app/Store && pnpm run type-check && pnpm run lint
```
Expected: 0 errors, 0 violations.

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/features/catalog/components/ProductBadge.vue app/Store/src/features/catalog/components/ProductCard.vue
git commit -m "feat(store): add ProductBadge component with New badge on recent products"
```

---

### Task 5: Create Home page components (Hero, FeaturesStrip, CategoryGrid, FeaturedProductsRow, RecentlyViewedRow)

**Files:**
- Create: `app/Store/src/features/catalog/components/HeroSection.vue`
- Create: `app/Store/src/features/catalog/components/FeaturesStrip.vue`
- Create: `app/Store/src/features/catalog/components/CategoryGrid.vue`
- Create: `app/Store/src/features/catalog/components/FeaturedProductsRow.vue`
- Create: `app/Store/src/features/catalog/components/RecentlyViewedRow.vue`
- Modify: `app/Store/src/features/catalog/views/HomeView.vue`

**Interfaces:**
- Consumes: `productApi.getPagedProducts()`, `taxonApi.getTaxons()`, `useRecentlyViewed()`
- Produces: 5 new home page sections, rewired HomeView

- [ ] **Step 1: Write HeroSection.vue**

Write `app/Store/src/features/catalog/components/HeroSection.vue`:

```vue
<template>
  <!-- Section: Hero Banner -->
  <section class="bg-gradient-to-br from-teal-800 via-teal-700 to-teal-900 text-white">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-24 md:py-32 text-center">
      <!-- Section: Badge -->
      <span class="inline-block bg-white/20 text-white text-sm font-medium px-4 py-1 rounded-full mb-6">
        New Collection
      </span>
      <!-- Section: Heading -->
      <h1 class="text-4xl md:text-6xl font-bold mb-4">Discover Your Style</h1>
      <!-- Section: Subtitle -->
      <p class="text-lg text-white/70 mb-8 max-w-xl mx-auto">
        Shop the latest fashion trends with visual search. Upload an image, find your look.
      </p>
      <!-- Section: CTAs -->
      <div class="flex justify-center gap-4">
        <router-link to="/shop">
          <Button label="Shop All" size="large" />
        </router-link>
        <router-link to="/recommendations">
          <Button label="Visual Search" severity="secondary" size="large" />
        </router-link>
      </div>
    </div>
  </section>
</template>
```

- [ ] **Step 2: Write FeaturesStrip.vue**

Write `app/Store/src/features/catalog/components/FeaturesStrip.vue`:

```vue
<template>
  <!-- Section: Features Strip -->
  <section class="bg-white border-b border-stone-200">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div class="grid grid-cols-2 md:grid-cols-4 gap-8">
        <!-- Section: Free Shipping -->
        <div class="text-center">
          <div class="w-12 h-12 mx-auto mb-3 bg-teal-50 text-teal-700 rounded-full flex items-center justify-center">
            <i class="pi pi-truck text-xl" />
          </div>
          <h3 class="text-sm font-semibold text-stone-900 mb-1">Free Shipping</h3>
          <p class="text-xs text-stone-500">On orders over 500.000 d</p>
        </div>
        <!-- Section: Easy Returns -->
        <div class="text-center">
          <div class="w-12 h-12 mx-auto mb-3 bg-teal-50 text-teal-700 rounded-full flex items-center justify-center">
            <i class="pi pi-undo text-xl" />
          </div>
          <h3 class="text-sm font-semibold text-stone-900 mb-1">Easy Returns</h3>
          <p class="text-xs text-stone-500">30-day return policy</p>
        </div>
        <!-- Section: Secure Payment -->
        <div class="text-center">
          <div class="w-12 h-12 mx-auto mb-3 bg-teal-50 text-teal-700 rounded-full flex items-center justify-center">
            <i class="pi pi-shield text-xl" />
          </div>
          <h3 class="text-sm font-semibold text-stone-900 mb-1">Secure Payment</h3>
          <p class="text-xs text-stone-500">SSL encrypted checkout</p>
        </div>
        <!-- Section: 24/7 Support -->
        <div class="text-center">
          <div class="w-12 h-12 mx-auto mb-3 bg-teal-50 text-teal-700 rounded-full flex items-center justify-center">
            <i class="pi pi-headphones text-xl" />
          </div>
          <h3 class="text-sm font-semibold text-stone-900 mb-1">24/7 Support</h3>
          <p class="text-xs text-stone-500">Dedicated customer service</p>
        </div>
      </div>
    </div>
  </section>
</template>
```

- [ ] **Step 3: Write CategoryGrid.vue**

Write `app/Store/src/features/catalog/components/CategoryGrid.vue`:

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getTaxons } from '../services/taxonApi'
import type { StoreTaxonListItemResponse } from '../types/taxon'

const taxons = ref<StoreTaxonListItemResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

// Trigger: Fetch top-level taxons on mount.
onMounted(async () => {
  const result = await getTaxons({ pageNumber: 1, pageSize: 8 })
  if (result.isSuccess) taxons.value = result.items.filter(t => t.depth === 0)
  else error.value = result.message ?? 'Failed to load categories'
  loading.value = false
})
</script>
<template>
  <!-- Section: Category Grid -->
  <section v-if="loading || taxons.length > 0" class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
    <h2 class="text-2xl font-bold text-stone-900 mb-8">Shop by Category</h2>
    <!-- Section: Loading -->
    <SkeletonGrid v-if="loading" :count="4" />
    <!-- Section: Error -->
    <Message v-else-if="error" severity="error" class="mb-4">{{ error }}</Message>
    <!-- Section: Grid -->
    <div v-else class="grid grid-cols-2 lg:grid-cols-4 gap-4">
      <router-link
        v-for="taxon in taxons"
        :key="taxon.id"
        :to="`/shop?taxonId=${taxon.id}`"
        class="group relative aspect-[4/3] rounded-xl overflow-hidden bg-stone-200"
      >
        <img
          v-if="taxon.imageUrl"
          :src="taxon.imageUrl"
          :alt="taxon.name"
          class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
        />
        <div class="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent" />
        <div class="absolute bottom-0 left-0 right-0 p-4">
          <h3 class="text-white text-lg font-semibold">{{ taxon.name }}</h3>
          <p class="text-white/70 text-sm">{{ taxon.taxonCount }} products</p>
        </div>
      </router-link>
    </div>
  </section>
</template>
```

- [ ] **Step 4: Write FeaturedProductsRow.vue**

Write `app/Store/src/features/catalog/components/FeaturedProductsRow.vue`:

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getPagedProducts } from '../services/productApi'
import ProductCard from './ProductCard.vue'
import type { StoreProductListItemResponse } from '../types/product'

const products = ref<StoreProductListItemResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const emit = defineEmits<{ addToCart: [variantId: string] }>()

// Trigger: Fetch featured products on mount.
onMounted(async () => {
  const result = await getPagedProducts({ pageNumber: 1, pageSize: 12, sort: ['-createdAtUtc'] })
  if (result.isSuccess) products.value = result.items
  else error.value = result.message ?? 'Failed to load featured products'
  loading.value = false
})
</script>
<template>
  <!-- Section: Featured Products -->
  <section v-if="loading || products.length > 0" class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
    <div class="flex items-center justify-between mb-8">
      <h2 class="text-2xl font-bold text-stone-900">Featured</h2>
      <router-link to="/shop" class="text-sm text-teal-600 hover:text-teal-700 font-medium">View All &rarr;</router-link>
    </div>
    <!-- Section: Loading -->
    <div v-if="loading" class="flex gap-4 overflow-x-auto pb-4">
      <SkeletonCard v-for="i in 4" :key="i" />
    </div>
    <!-- Section: Error -->
    <Message v-else-if="error" severity="error" class="mb-4">{{ error }}</Message>
    <!-- Section: Scrollable Row -->
    <div v-else class="flex gap-4 overflow-x-auto pb-4">
      <div v-for="product in products" :key="product.id" class="w-64 shrink-0">
        <ProductCard :product="product" @add-to-cart="(id: string) => emit('addToCart', id)" />
      </div>
    </div>
  </section>
</template>
```

- [ ] **Step 5: Write RecentlyViewedRow.vue**

Write `app/Store/src/features/catalog/components/RecentlyViewedRow.vue`:

```vue
<script setup lang="ts">
import { useRecentlyViewed } from '@/shared/composables/useRecentlyViewed'
import ProductCard from './ProductCard.vue'

const { items, clear } = useRecentlyViewed()
const emit = defineEmits<{ addToCart: [variantId: string] }>()
</script>
<template>
  <!-- Section: Recently Viewed -->
  <section v-if="items.value.length > 0" class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
    <div class="flex items-center justify-between mb-8">
      <h2 class="text-2xl font-bold text-stone-900">Recently Viewed</h2>
      <button class="text-sm text-stone-500 hover:text-stone-700 font-medium" @click="clear">Clear</button>
    </div>
    <!-- Section: Scrollable Row -->
    <div class="flex gap-4 overflow-x-auto pb-4">
      <div v-for="item in items.value" :key="item.productId" class="w-64 shrink-0">
        <ProductCard
          :product="{
            id: item.productId,
            masterVariantId: item.productId,
            name: item.productName,
            status: '',
            description: null,
            slug: item.slug,
            minPrice: item.minPrice,
            currency: null,
            thumbnailUrl: item.thumbnailUrl,
            thumbnailAlt: null,
            styleCode: null,
            seasonName: null,
            materialComposition: null,
            careInstructions: null,
            fitNotes: null,
            department: null,
            genderTarget: null,
            variantsCount: 0,
            availableOn: null,
          }"
          @add-to-cart="(id: string) => emit('addToCart', id)"
        />
      </div>
    </div>
  </section>
</template>
```

- [ ] **Step 6: Rewrite HomeView.vue**

Read the current `HomeView.vue` (60 lines). Replace script and template with:

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getPaged } from '@/shared/api/paged'
import { ENDPOINTS } from '@/shared/constants/api'
import type { StoreProductListItemResponse } from '../types/product'
import HeroSection from '../components/HeroSection.vue'
import FeaturesStrip from '../components/FeaturesStrip.vue'
import CategoryGrid from '../components/CategoryGrid.vue'
import FeaturedProductsRow from '../components/FeaturedProductsRow.vue'
import RecentlyViewedRow from '../components/RecentlyViewedRow.vue'
import ProductGrid from '../components/ProductGrid.vue'

const newArrivals = ref<StoreProductListItemResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  const result = await getPaged<StoreProductListItemResponse>(ENDPOINTS.products, {
    pageNumber: 1,
    pageSize: 8,
    sort: ['-createdAtUtc'],
  })
  if (result.isSuccess) newArrivals.value = result.items
  else error.value = result.message
  loading.value = false
})
</script>
<template>
  <!-- Section: Home Page -->
  <div>
    <HeroSection />
    <FeaturesStrip />
    <CategoryGrid />
    <FeaturedProductsRow />
    <RecentlyViewedRow />
    <!-- Section: New Arrivals -->
    <section class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
      <h2 class="text-2xl font-bold text-stone-900 mb-8">New Arrivals</h2>
      <ProductGrid :products="newArrivals" :loading="loading" :error="error" @reload="() => {}" />
      <div class="text-center mt-8">
        <router-link to="/shop">
          <Button label="View All Products" severity="secondary" />
        </router-link>
      </div>
    </section>
  </div>
</template>
```

Replace `bg-gray-900` -> `from-teal-800 via-teal-700 to-teal-900` gradient, `text-gray-*` -> `text-stone-*`, etc., to match new palette.

- [ ] **Step 7: Run type-check + lint**

```bash
cd app/Store && pnpm run type-check && pnpm run lint
```
Expected: 0 errors, 0 violations. Check that `SkeletonCard`, `SkeletonGrid`, `Message` resolve via PrimeVue auto-import.

- [ ] **Step 8: Commit**

```bash
git add app/Store/src/features/catalog/components/HeroSection.vue app/Store/src/features/catalog/components/FeaturesStrip.vue app/Store/src/features/catalog/components/CategoryGrid.vue app/Store/src/features/catalog/components/FeaturedProductsRow.vue app/Store/src/features/catalog/components/RecentlyViewedRow.vue app/Store/src/features/catalog/views/HomeView.vue
git commit -m "feat(store): add Hero, FeaturesStrip, CategoryGrid, FeaturedProducts, RecentlyViewed to Home page"
```

---

### Task 6: Create SearchOverlay with useSearch composable

**Files:**
- Create: `app/Store/src/features/catalog/composables/useSearch.ts`
- Create: `app/Store/src/features/catalog/components/SearchOverlay.vue`
- Modify: `app/Store/src/app/components/layout/AppHeader.vue` — replace search form with icon trigger
- Modify: `app/Store/src/App.vue` — mount SearchOverlay

**Interfaces:**
- Consumes: `productApi.getPagedProducts()` for search results
- Produces: search overlay dialog accessible from any page

- [ ] **Step 1: Write useSearch.ts**

Write `app/Store/src/features/catalog/composables/useSearch.ts`:

```ts
import { ref } from 'vue'
import { getPagedProducts } from '../services/productApi'
import type { StoreProductListItemResponse } from '../types/product'

export function useSearch() {
  const isOpen = ref(false)
  const query = ref('')
  const results = ref<StoreProductListItemResponse[]>([])
  const loading = ref(false)
  const selectedIndex = ref(0)
  const error = ref<string | null>(null)

  let debounceTimer: ReturnType<typeof setTimeout> | null = null

  function open(): void {
    isOpen.value = true
    selectedIndex.value = 0
  }

  function close(): void {
    isOpen.value = false
    query.value = ''
    results.value = []
    error.value = null
  }

  function clear(): void {
    query.value = ''
    results.value = []
    error.value = null
  }

  // Trigger: Debounced keyword search.
  async function search(): Promise<void> {
    if (!query.value.trim()) {
      results.value = []
      return
    }
    if (debounceTimer) clearTimeout(debounceTimer)
    debounceTimer = setTimeout(async () => {
      loading.value = true
      error.value = null
      const result = await getPagedProducts({ pageNumber: 1, pageSize: 5, search: query.value.trim() })
      if (result.isSuccess) {
        results.value = result.items
      } else {
        error.value = result.message ?? 'Search failed'
      }
      loading.value = false
    }, 300)
  }

  function navigateToResult(index: number): void {
    const item = results.value[index]
    if (!item) return
    close()
    window.location.href = `/products/${item.slug}`
  }

  return { isOpen, query, results, loading, selectedIndex, error, open, close, clear, search, navigateToResult }
}
```

- [ ] **Step 2: Write SearchOverlay.vue**

Write `app/Store/src/features/catalog/components/SearchOverlay.vue`:

```vue
<script setup lang="ts">
import { watch } from 'vue'
import { useSearch } from '../composables/useSearch'
import { formatVnd } from '@/shared/utils/currency'

const search = useSearch()

// Trigger: Debounced search when query changes.
watch(() => search.query.value, () => search.search())

// Map: Keyboard navigation.
function onKeyDown(e: KeyboardEvent): void {
  if (e.key === 'ArrowDown') {
    e.preventDefault()
    search.selectedIndex.value = Math.min(search.selectedIndex.value + 1, search.results.value.length - 1)
  } else if (e.key === 'ArrowUp') {
    e.preventDefault()
    search.selectedIndex.value = Math.max(search.selectedIndex.value - 1, 0)
  } else if (e.key === 'Enter') {
    search.navigateToResult(search.selectedIndex.value)
  } else if (e.key === 'Escape') {
    search.close()
  }
}

// Map: Format price for display.
function displayPrice(price: number | null): string {
  return price != null ? formatVnd(price) : 'Contact'
}
</script>
<template>
  <!-- Section: Search Overlay -->
  <Dialog
    :visible="search.isOpen.value"
    modal
    :style="{ width: '600px' }"
    :breakpoints="{ '768px': '100vw' }"
    :pt="{ root: 'border-0', content: 'px-0 pb-0' }"
    @update:visible="(val: boolean) => { if (!val) search.close() }"
  >
    <template #header>
      <span class="p-input-icon-left w-full">
        <i class="pi pi-search text-stone-400" />
        <InputText
          :model-value="search.query.value"
          placeholder="Search products..."
          class="w-full border-0 shadow-none text-lg"
          autofocus
          @update:model-value="(val: string) => search.query.value = val"
          @keydown="onKeyDown"
        />
      </span>
    </template>
    <!-- Section: Loading State -->
    <div v-if="search.loading.value" class="px-6 py-4 space-y-3">
      <div v-for="i in 3" :key="i" class="flex items-center gap-3 animate-pulse">
        <div class="w-12 h-12 bg-stone-200 rounded-lg shrink-0" />
        <div class="flex-1 space-y-1.5">
          <div class="h-4 bg-stone-200 rounded w-3/4" />
          <div class="h-3 bg-stone-200 rounded w-1/4" />
        </div>
      </div>
    </div>
    <!-- Section: Results -->
    <ul v-else-if="search.results.value.length > 0" class="divide-y divide-stone-100">
      <li
        v-for="(item, idx) in search.results.value"
        :key="item.id"
        class="flex items-center gap-3 px-6 py-3 cursor-pointer transition-colors"
        :class="idx === search.selectedIndex.value ? 'bg-teal-50' : 'hover:bg-stone-50'"
        @click="search.navigateToResult(idx)"
      >
        <img
          v-if="item.thumbnailUrl"
          :src="item.thumbnailUrl"
          :alt="item.thumbnailAlt ?? item.name"
          class="w-12 h-12 rounded-lg object-cover bg-stone-100 shrink-0"
        />
        <div v-else class="w-12 h-12 rounded-lg bg-stone-100 flex items-center justify-center shrink-0">
          <i class="pi pi-image text-stone-400" />
        </div>
        <div class="flex-1 min-w-0">
          <p class="text-sm font-medium text-stone-900 truncate">{{ item.name }}</p>
          <p class="text-sm font-semibold text-stone-700">{{ displayPrice(item.minPrice) }}</p>
        </div>
      </li>
    </ul>
    <!-- Section: Empty State -->
    <div v-else-if="search.query.value.trim() && !search.loading.value" class="px-6 py-8 text-center">
      <p class="text-stone-500">No products found for "{{ search.query.value }}"</p>
    </div>
    <!-- Section: View All Footer -->
    <div v-if="search.query.value.trim() && search.results.value.length > 0" class="px-6 py-3 border-t border-stone-100">
      <router-link :to="`/shop?search=${encodeURIComponent(search.query.value)}`" class="text-sm text-teal-600 hover:text-teal-700 font-medium" @click="search.close()">
        View all results for "{{ search.query.value }}" &rarr;
      </router-link>
    </div>
  </Dialog>
</template>
```

- [ ] **Step 3: Replace search form in AppHeader with icon trigger**

Read `AppHeader.vue`. Remove the `<form>` block (lines 33-42 — the `p-input-icon-left` + `InputText`). Replace with:

```vue
<!-- Section: Search Trigger -->
<button
  class="hidden md:flex p-2 text-stone-500 hover:text-teal-700 transition-colors"
  aria-label="Search products"
  @click="search.open()"
>
  <i class="pi pi-search text-xl" />
</button>
```

Add import and composable call in `<script setup>`:
```ts
import { useSearch } from '@/features/catalog/composables/useSearch'
const search = useSearch()
```

Keep the `searchQuery` ref and `onSearch` function — they're no longer needed. Remove them to avoid dead code:
- Remove `import { ref }` (if no other refs use it — check other code)
- Remove `const searchQuery = ref('')`
- Remove `function onSearch() { ... }`

- [ ] **Step 4: Mount SearchOverlay in App.vue**

Read `App.vue`. Add import and component:
```ts
import SearchOverlay from '@/features/catalog/components/SearchOverlay.vue'
```

Add in template (after `<ScrollToTop />`):
```vue
<SearchOverlay />
```

Also add Ctrl+K global listener. Add in `<script setup>`:
```ts
import { onMounted, onUnmounted } from 'vue'

function onGlobalKeyDown(e: KeyboardEvent): void {
  if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
    e.preventDefault()
    const { useSearch } = await import('@/features/catalog/composables/useSearch')
    useSearch().open()
  }
}

onMounted(() => document.addEventListener('keydown', onGlobalKeyDown))
onUnmounted(() => document.removeEventListener('keydown', onGlobalKeyDown))
```

Wait — dynamic import inside an event handler is expensive. Better approach: import the composable eagerly and keep the module-level singleton reference. Update:

```ts
import { useSearch } from '@/features/catalog/composables/useSearch'

const search = useSearch()

function onGlobalKeyDown(e: KeyboardEvent): void {
  if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
    e.preventDefault()
    search.open()
  }
}

onMounted(() => document.addEventListener('keydown', onGlobalKeyDown))
onUnmounted(() => document.removeEventListener('keydown', onGlobalKeyDown))
```

- [ ] **Step 5: Run type-check + lint**

```bash
cd app/Store && pnpm run type-check && pnpm run lint
```
Expected: 0 errors, 0 violations. `Dialog`, `InputText` auto-imported by PrimeVue.

- [ ] **Step 6: Commit**

```bash
git add app/Store/src/features/catalog/composables/useSearch.ts app/Store/src/features/catalog/components/SearchOverlay.vue app/Store/src/app/components/layout/AppHeader.vue app/Store/src/App.vue
git commit -m "feat(store): add SearchOverlay with debounced autocomplete and Ctrl+K shortcut"
```

---

### Task 7: Create SizeGuideModal + ProductDetailsInfo + integrate into ProductDetailView

**Files:**
- Create: `app/Store/src/features/catalog/components/SizeGuideModal.vue`
- Create: `app/Store/src/features/catalog/components/ProductDetailsInfo.vue`
- Modify: `app/Store/src/features/catalog/views/ProductDetailView.vue`

**Interfaces:**
- Consumes: `StoreProductDetailResponse` type (variants, optionValues)
- Produces: size guide modal dialog, product details property list, recently viewed tracking

- [ ] **Step 1: Write SizeGuideModal.vue**

Write `app/Store/src/features/catalog/components/SizeGuideModal.vue`:

```vue
<script setup lang="ts">
import { ref } from 'vue'
import type { StoreProductVariantResponse } from '../types/product'

const props = defineProps<{ variants: StoreProductVariantResponse[]; productName: string }>()

const visible = ref(false)

// Map: Extract unique option values for display.
const sizeOptions = computed(() => {
  const seen = new Set<string>()
  const sizes: string[] = []
  for (const v of props.variants) {
    const label = v.optionValue1?.presentation ?? v.optionValue1?.name
    if (label && !seen.has(label)) {
      seen.add(label)
      sizes.push(label)
    }
  }
  return sizes
})
</script>
<template>
  <button class="text-sm text-teal-600 hover:text-teal-700 font-medium flex items-center gap-1" @click="visible = true">
    <i class="pi pi-ruler" /> Size Guide
  </button>
  <Dialog v-model:visible="visible" modal :header="`Size Guide — ${productName}`" :style="{ width: '480px' }">
    <!-- Section: Size Table -->
    <div v-if="sizeOptions.length > 0" class="grid grid-cols-2 gap-3">
      <div v-for="size in sizeOptions" :key="size" class="text-center p-3 border border-stone-200 rounded-lg">
        <span class="text-sm font-semibold text-stone-900">{{ size }}</span>
      </div>
    </div>
    <!-- Section: No Data -->
    <p v-else class="text-stone-500 text-center py-4">Size information not available for this product.</p>
  </Dialog>
</template>
```

Add `computed` import to the existing import line.

- [ ] **Step 2: Write ProductDetailsInfo.vue**

Write `app/Store/src/features/catalog/components/ProductDetailsInfo.vue`:

```vue
<script setup lang="ts">
import type { StoreProductDetailResponse } from '../types/product'

defineProps<{ product: StoreProductDetailResponse }>()
</script>
<template>
  <!-- Section: Product Details Info -->
  <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
    <div v-if="product.styleCode" class="flex items-center gap-2 text-sm">
      <i class="pi pi-hashtag text-teal-600" />
      <span class="text-stone-500">Style:</span>
      <span class="text-stone-900 font-medium">{{ product.styleCode }}</span>
    </div>
    <div v-if="product.materialComposition" class="flex items-center gap-2 text-sm">
      <i class="pi pi-palette text-teal-600" />
      <span class="text-stone-500">Material:</span>
      <span class="text-stone-900 font-medium">{{ product.materialComposition }}</span>
    </div>
    <div v-if="product.careInstructions" class="flex items-center gap-2 text-sm">
      <i class="pi pi-droplet text-teal-600" />
      <span class="text-stone-500">Care:</span>
      <span class="text-stone-900 font-medium">{{ product.careInstructions }}</span>
    </div>
    <div v-if="product.fitNotes" class="flex items-center gap-2 text-sm">
      <i class="pi pi-ruler text-teal-600" />
      <span class="text-stone-500">Fit:</span>
      <span class="text-stone-900 font-medium">{{ product.fitNotes }}</span>
    </div>
    <div v-if="product.seasonName" class="flex items-center gap-2 text-sm">
      <i class="pi pi-calendar text-teal-600" />
      <span class="text-stone-500">Season:</span>
      <span class="text-stone-900 font-medium">{{ product.seasonName }}</span>
    </div>
    <div v-if="product.department" class="flex items-center gap-2 text-sm">
      <i class="pi pi-tag text-teal-600" />
      <span class="text-stone-500">Department:</span>
      <span class="text-stone-900 font-medium">{{ product.department }}</span>
    </div>
    <div v-if="product.genderTarget" class="flex items-center gap-2 text-sm">
      <i class="pi pi-users text-teal-600" />
      <span class="text-stone-500">Gender:</span>
      <span class="text-stone-900 font-medium">{{ product.genderTarget }}</span>
    </div>
  </div>
</template>
```

- [ ] **Step 3: Modify ProductDetailView.vue**

Read `ProductDetailView.vue`. Apply changes:

**Add imports** (after existing imports):
```ts
import SizeGuideModal from '../components/SizeGuideModal.vue'
import ProductDetailsInfo from '../components/ProductDetailsInfo.vue'
import { useRecentlyViewed } from '@/shared/composables/useRecentlyViewed'
```

**Add recently viewed tracking** in `loadProduct`, after `product.value = result.value`:
```ts
useRecentlyViewed().add({
  productId: result.value.id,
  productName: result.value.name,
  slug: result.value.slug,
  thumbnailUrl: result.value.thumbnailUrl,
  minPrice: result.value.minPrice,
  viewedAt: Date.now(),
})
```

**Add SizeGuideModal trigger** in template, after the `<h1>` heading line, before the price section:
```vue
<SizeGuideModal v-if="product.variants.length > 0" :variants="product.variants" :product-name="product.name" />
```

**Add ProductDetailsInfo** in template, after the fashion metadata badges section (lines 116-122), before the variant options:
```vue
<ProductDetailsInfo :product="product" />
```

- [ ] **Step 4: Run type-check + lint**

```bash
cd app/Store && pnpm run type-check && pnpm run lint
```
Expected: 0 errors, 0 violations.

- [ ] **Step 5: Commit**

```bash
git add app/Store/src/features/catalog/components/SizeGuideModal.vue app/Store/src/features/catalog/components/ProductDetailsInfo.vue app/Store/src/features/catalog/views/ProductDetailView.vue
git commit -m "feat(store): add SizeGuide modal, ProductDetailsInfo, and recently viewed tracking to PDP"
```

---

### Task 8: Create NotificationBell + Terms/Privacy views + routes

**Files:**
- Create: `app/Store/src/features/catalog/components/NotificationBell.vue`
- Create: `app/Store/src/features/catalog/views/TermsView.vue`
- Create: `app/Store/src/features/catalog/views/PrivacyView.vue`
- Modify: `app/Store/src/features/catalog/routes/index.ts`
- Modify: `app/Store/src/app/components/layout/AppHeader.vue` — add NotificationBell

- [ ] **Step 1: Write NotificationBell.vue**

Write `app/Store/src/features/catalog/components/NotificationBell.vue`:

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/features/identity/stores/authStore'
import { getNotificationPreferences } from '@/features/profile/services/notificationApi'
import type { NotificationCategory } from '@/features/profile/types/notification'

const auth = useAuthStore()
const op = ref(null)
const preferences = ref<NotificationCategory[]>([])

// Trigger: Fetch notification preferences on mount.
onMounted(async () => {
  if (!auth.isAuthenticated) return
  const result = await getNotificationPreferences()
  if (result.isSuccess && result.value) {
    preferences.value = Array.isArray(result.value) ? result.value : []
  }
})
</script>
<template>
  <div v-if="auth.isAuthenticated">
    <button
      class="relative p-2 text-stone-500 hover:text-teal-700 transition-colors"
      aria-label="Notifications"
      @click="(e: MouseEvent) => op?.toggle(e)"
    >
      <i class="pi pi-bell text-xl" />
    </button>
    <Popover ref="op">
      <div class="w-72">
        <p class="text-sm font-semibold text-stone-900 mb-3">Notification Preferences</p>
        <div v-for="pref in preferences" :key="pref.id" class="flex items-center justify-between py-2">
          <span class="text-sm text-stone-700">{{ pref.name ?? pref.id }}</span>
          <i :class="pref.enabled ? 'pi pi-check-circle text-teal-600' : 'pi pi-times-circle text-stone-400'" class="text-lg" />
        </div>
        <div v-if="preferences.length === 0" class="text-sm text-stone-400 text-center py-4">No notification preferences configured.</div>
        <div class="mt-3 pt-3 border-t border-stone-100">
          <router-link to="/account/notifications" class="text-sm text-teal-600 hover:text-teal-700 font-medium">Manage Preferences &rarr;</router-link>
        </div>
      </div>
    </Popover>
  </div>
</template>
```

- [ ] **Step 2: Write TermsView.vue**

```vue
<template>
  <!-- Section: Terms of Service Page -->
  <div class="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
    <h1 class="text-3xl font-bold text-stone-900 mb-8">Terms of Service</h1>
    <div class="prose prose-stone max-w-none">
      <p class="text-stone-600 leading-relaxed mb-4">Last updated: January 2026</p>
      <h2 class="text-xl font-semibold text-stone-900 mt-8 mb-4">1. Acceptance of Terms</h2>
      <p class="text-stone-600 leading-relaxed mb-4">By accessing and using ReSys.Shop, you agree to be bound by these Terms of Service.</p>
      <h2 class="text-xl font-semibold text-stone-900 mt-8 mb-4">2. Use of Service</h2>
      <p class="text-stone-600 leading-relaxed mb-4">You agree to use the service only for lawful purposes and in accordance with these terms.</p>
      <h2 class="text-xl font-semibold text-stone-900 mt-8 mb-4">3. Account Responsibilities</h2>
      <p class="text-stone-600 leading-relaxed mb-4">You are responsible for maintaining the confidentiality of your account credentials.</p>
      <h2 class="text-xl font-semibold text-stone-900 mt-8 mb-4">4. Intellectual Property</h2>
      <p class="text-stone-600 leading-relaxed mb-4">All content on ReSys.Shop is protected by intellectual property laws.</p>
    </div>
  </div>
</template>
```

- [ ] **Step 3: Write PrivacyView.vue**

Same structure, replace title with "Privacy Policy" and section headings with privacy-appropriate content (data collection, use, sharing, cookies, security, rights).

- [ ] **Step 4: Add routes to catalog routes**

Read `app/Store/src/features/catalog/routes/index.ts`. Add after the last route entry, before the closing `]`:

```ts
{
  path: '/terms',
  name: 'terms',
  component: () => import('../views/TermsView.vue'),
  meta: { title: 'Terms of Service' },
},
{
  path: '/privacy',
  name: 'privacy',
  component: () => import('../views/PrivacyView.vue'),
  meta: { title: 'Privacy Policy' },
},
```

- [ ] **Step 5: Add NotificationBell to AppHeader**

Read `AppHeader.vue`. Add import:
```ts
import NotificationBell from '@/features/catalog/components/NotificationBell.vue'
```

Insert `<NotificationBell />` in the actions div, between the search trigger and the ThemeToggle:
```
Search -> NotificationBell -> ThemeToggle -> Cart -> User
```

- [ ] **Step 6: Run type-check + lint**

```bash
cd app/Store && pnpm run type-check && pnpm run lint
```
Expected: 0 errors, 0 violations. `Popover` auto-imported by PrimeVue.

- [ ] **Step 7: Commit**

```bash
git add app/Store/src/features/catalog/components/NotificationBell.vue app/Store/src/features/catalog/views/TermsView.vue app/Store/src/features/catalog/views/PrivacyView.vue app/Store/src/features/catalog/routes/index.ts app/Store/src/app/components/layout/AppHeader.vue
git commit -m "feat(store): add NotificationBell, Terms, and Privacy views with routes"
```

---

### Task 9: Final verification

- [ ] **Step 1: Full test suite**

```bash
cd app/Store && pnpm run type-check && pnpm run lint && pnpm run test:unit -- --run
```
Expected: 0 errors, 0 violations, all tests pass.

- [ ] **Step 2: Manual verification checklist**

```bash
cd app/Store && pnpm run dev &
```

1. Home page: 6 sections render in order — Hero, Features, Categories, Featured, Recently Viewed, New Arrivals
2. Ctrl+K / Cmd+K: search overlay opens, type query, results appear, Enter navigates
3. Footer: 4 columns desktop, stacked mobile. /terms and /privacy links work
4. Product detail: "Size Guide" opens modal. ProductDetailsInfo shows non-null fields. "New" badge on recent products (< 14 days)
5. Notification bell: visible when authenticated, popover opens with preferences
6. /terms, /privacy: render content without 404
7. Breadcrumbs: present on /shop, /collections, /checkout, /products/:slug
8. Recently viewed: visit 3 product pages -> Home shows Recently Viewed row with 3 cards
9. Dark mode: toggle via Spec A ThemeToggle — all new components render correctly

```bash
kill %1
```

---

## Verification

1. `pnpm run type-check` — 0 errors
2. `pnpm run lint` — 0 violations
3. `pnpm run test:unit -- --run` — all tests pass (existing + new useRecentlyViewed tests)
4. 13 features render in browser, all states (loading/error/empty) covered
5. No mock data — all API calls use existing backend endpoints via existing service files
6. Playfair Display visible on headings (h1-h6), DM Sans on body text
7. No hardcoded hex colors in component templates (all use Tailwind teal-*/stone-*)
