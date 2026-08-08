# Store SPA Cycle 2: Catalog Views — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace all 10 catalog skeleton views and 1 overlay component with fully functional PrimeVue + Tailwind implementations — editorial home hero, sidebar-filtered product grid, side-by-side product detail with visual similarity, image search, and static content pages.

**Architecture:** 1 shared ProductCard component consumed by 6 views. All stores already wired — views just consume existing Pinia store APIs. No new services or stores. PrimeVue components (Card, TabView, SelectButton, Paginator, InputNumber, FileUpload, Tag, Badge) auto-imported via unplugin-vue-components.

**Tech Stack:** Vue 3.5, PrimeVue 5 (Aura), Tailwind CSS v4, Vitest + jsdom, @pinia/testing

## Global Constraints

- `TreatWarningsAsErrors=true` — no TypeScript warnings
- Neutral color palette only (`neutral-*`), teal primary (`#0d7377`) for CTAs/links only
- Inter body font, Newsreader italic for hero headline only, JetBrains Mono for prices only
- All views use `max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8` layout container
- Product cards use `aspect-[3/4]` images, `bg-neutral-100` placeholder fallback
- All animations respect `prefers-reduced-motion: reduce`
- PrimeVue components are auto-imported — never import them manually in .vue files
- Card, Skeleton, Breadcrumb, Message, Button, TabView, SelectButton, Paginator, InputNumber, Tag, Badge are all auto-imported

---

### Task 1: ProductCard — shared component

**Files:**
- Create: `app/Store/src/features/catalog/components/ProductCard.vue`

**Interfaces:**
- Produces: `ProductCard` component with props: `product` (StoreProductListItemResponse), `aspectRatio?: string`, `showSimilarity?: boolean`
- Exported for use in: HomeView, ShopView, ProductDetailView, CollectionsView, VisualSearchView

- [ ] **Step 1: Create ProductCard.vue**

```vue
<script setup lang="ts">
import type { StoreProductListItemResponse } from '../types'
import { useCurrency } from '@/shared/utils/currency'

const props = withDefaults(defineProps<{
  product: StoreProductListItemResponse
  aspectRatio?: string
  showSimilarity?: boolean
  similarityScore?: number
}>(), {
  aspectRatio: 'aspect-[3/4]',
  showSimilarity: false,
  similarityScore: 0,
})

const imageUrl = props.product.masterVariant?.images?.[0]?.url ?? null
const price = props.product.masterVariant?.price
const formattedPrice = price != null ? useCurrency(price) : null
const brand = props.product.department ?? null
</script>
<template>
  <router-link
    :to="`/products/${product.slug}`"
    class="group block"
  >
    <div :class="[aspectRatio, 'relative overflow-hidden rounded-lg bg-neutral-100']">
      <img
        v-if="imageUrl"
        :src="imageUrl"
        :alt="product.name"
        class="w-full h-full object-cover transition-opacity duration-300 group-hover:opacity-90"
      />
      <div
        v-else
        class="w-full h-full flex items-center justify-center text-neutral-300"
      >
        <i class="pi pi-image text-3xl" />
      </div>
      <span
        v-if="showSimilarity && similarityScore > 0"
        class="absolute top-2 right-2 bg-teal-500/90 text-white text-xs font-medium rounded px-1.5 py-0.5"
      >{{ (similarityScore * 100).toFixed(1) }}%</span>
    </div>
    <div class="mt-2 space-y-0.5">
      <p v-if="brand" class="text-xs text-neutral-500 uppercase tracking-wide">{{ brand }}</p>
      <p class="text-sm font-medium text-neutral-900 truncate">{{ product.name }}</p>
      <p v-if="formattedPrice" class="text-sm font-medium text-neutral-900 font-mono">{{ formattedPrice }}</p>
    </div>
  </router-link>
</template>
```

- [ ] **Step 2: Verify build**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/features/catalog/components/ProductCard.vue
git commit -m "feat(store): add shared ProductCard component"
```

---

### Task 2: HomeView — hero + categories + featured products

**Files:**
- Modify: `app/Store/src/features/catalog/views/HomeView.vue`

**Interfaces:**
- Consumes: `catalogStore.taxonomyGroups`, `productListStore.items`, `ProductCard` component

- [ ] **Step 1: Replace HomeView.vue**

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useCatalogStore } from '../stores/catalogStore'
import { useProductListStore } from '../stores/productListStore'
import ProductCard from '../components/ProductCard.vue'

usePageTitle('Home')
const catalog = useCatalogStore()
const productList = useProductListStore()

onMounted(async () => {
  await catalog.loadTaxonomyGroups()
  productList.init()
})
</script>
<template>
  <!-- Section: Hero — editorial statement with serif headline -->
  <section class="min-h-[60vh] flex items-center justify-center bg-neutral-100">
    <div class="text-center px-4 max-w-2xl">
      <h1 class="text-4xl md:text-5xl lg:text-6xl text-neutral-900 mb-6 italic" style="font-family: 'Newsreader', serif;">
        Curated fashion, intelligently found
      </h1>
      <p class="text-lg text-neutral-500 mb-8 max-w-xl mx-auto">
        Discover pieces that match your style — AI-powered recommendations from hundreds of independent brands.
      </p>
      <Button
        label="Shop New Arrivals"
        severity="primary"
        as="router-link"
        to="/shop"
        class="px-8 py-3"
      />
    </div>
  </section>

  <!-- Section: Featured Categories — taxonomy group cards -->
  <section class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-16">
    <h2 class="text-sm font-medium text-neutral-500 uppercase tracking-wide mb-8">Shop by Category</h2>
    <div class="grid grid-cols-2 md:grid-cols-4 gap-6">
      <router-link
        v-for="group in catalog.taxonomyGroups"
        :key="group.taxonomy.id"
        :to="`/shop`"
        class="group"
      >
        <div class="aspect-[3/4] bg-neutral-100 rounded-lg flex items-center justify-center transition-opacity group-hover:opacity-90">
          <span class="text-neutral-400 text-sm">Image</span>
        </div>
        <div class="mt-3">
          <p class="text-base font-semibold text-neutral-900">{{ group.taxonomy.name }}</p>
          <p class="text-sm text-neutral-500">Browse collection</p>
        </div>
      </router-link>
    </div>
  </section>

  <!-- Section: Featured Products — newest arrivals grid -->
  <section class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-16">
    <h2 class="text-sm font-medium text-neutral-500 uppercase tracking-wide mb-8">New Arrivals</h2>
    <div v-if="productList.loading && productList.items.length === 0" class="grid grid-cols-2 md:grid-cols-4 gap-6">
      <Skeleton v-for="i in 8" :key="i" class="aspect-[3/4] rounded-lg" />
    </div>
    <div v-else-if="productList.items.length > 0" class="grid grid-cols-2 md:grid-cols-4 gap-6">
      <ProductCard v-for="item in productList.items.slice(0, 8)" :key="item.id" :product="item" />
    </div>
    <div class="mt-8 text-right">
      <router-link to="/shop" class="text-sm font-medium text-neutral-900 hover:text-neutral-600 transition-colors">
        View All &rarr;
      </router-link>
    </div>
  </section>

  <!-- Section: Bottom CTA — newsletter signup placeholder -->
  <section class="bg-neutral-100 py-16">
    <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 text-center">
      <h2 class="text-lg font-semibold text-neutral-900 mb-4">Join the waitlist for exclusive drops</h2>
      <div class="flex items-center justify-center gap-3 max-w-md mx-auto">
        <InputText placeholder="Enter your email" class="flex-1" />
        <Button label="Subscribe" severity="secondary" outlined />
      </div>
    </div>
  </section>
</template>
```

- [ ] **Step 2: Verify build**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/features/catalog/views/HomeView.vue
git commit -m "feat(store): implement HomeView with editorial hero, categories, and featured products"
```

---

### Task 3: ShopView — sidebar filters

**Files:**
- Modify: `app/Store/src/features/catalog/views/ShopView.vue`

**Interfaces:**
- Consumes: `catalogStore` (taxonomyGroups, optionTypes, selectedTaxonIds, selectedOptionValueIds, minPrice, maxPrice, activeFilterCount, toggleTaxon, toggleOptionValue, setPriceRange, clearFilters)
- Produces: filter sidebar with taxonomy tree, price range, option value checkboxes

- [ ] **Step 1: Replace ShopView script + add taxonomy tree recursive component**

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useCatalogStore } from '../stores/catalogStore'
import { useProductListStore } from '../stores/productListStore'
import ProductCard from '../components/ProductCard.vue'

usePageTitle('Shop')
const catalog = useCatalogStore()
const productList = useProductListStore()
const mobileFiltersOpen = ref(false)

onMounted(() => {
  catalog.loadTaxonomyGroups()
  catalog.loadOptionTypes()
  productList.init()
})

function onTaxonClick(id: string): void {
  catalog.toggleTaxon(id)
}

function onOptionValueClick(id: string): void {
  catalog.toggleOptionValue(id)
}

function onClearFilters(): void {
  catalog.clearFilters()
}
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Shop' }]" />

    <div class="flex gap-8 mt-4">
      <!-- Section: Filter Sidebar — taxonomy tree, price range, option values -->
      <aside class="hidden lg:block w-64 shrink-0">
        <div class="sticky top-20 space-y-6">
          <!-- Active Filters Header -->
          <div v-if="catalog.activeFilterCount > 0" class="flex items-center justify-between">
            <span class="text-sm font-medium text-neutral-900">{{ catalog.activeFilterCount }} active</span>
            <button class="text-xs text-neutral-500 hover:text-neutral-900" @click="onClearFilters()">Clear All</button>
          </div>

          <!-- Taxonomy Tree -->
          <div v-for="group in catalog.taxonomyGroups" :key="group.taxonomy.id">
            <h3 class="text-xs font-semibold text-neutral-900 uppercase tracking-wide mb-2">{{ group.taxonomy.name }}</h3>
            <div v-for="taxon in group.tree" :key="taxon.id" class="ml-2">
              <label class="flex items-center gap-2 py-1 cursor-pointer text-sm text-neutral-700 hover:text-neutral-900">
                <input
                  type="checkbox"
                  :checked="catalog.selectedTaxonIds.includes(taxon.id)"
                  class="rounded border-neutral-300"
                  @change="onTaxonClick(taxon.id)"
                />
                <span :class="{ 'font-semibold text-neutral-900': catalog.selectedTaxonIds.includes(taxon.id) }">
                  {{ taxon.name }}
                </span>
              </label>
              <!-- Nested children -->
              <div v-for="child in taxon.children" :key="child.id" class="ml-4">
                <label class="flex items-center gap-2 py-1 cursor-pointer text-sm text-neutral-700 hover:text-neutral-900">
                  <input
                    type="checkbox"
                    :checked="catalog.selectedTaxonIds.includes(child.id)"
                    class="rounded border-neutral-300"
                    @change="onTaxonClick(child.id)"
                  />
                  <span :class="{ 'font-semibold text-neutral-900': catalog.selectedTaxonIds.includes(child.id) }">
                    {{ child.name }}
                  </span>
                </label>
              </div>
            </div>
          </div>

          <!-- Price Range -->
          <div>
            <h3 class="text-xs font-semibold text-neutral-900 uppercase tracking-wide mb-2">Price</h3>
            <div class="flex items-center gap-2">
              <InputText
                type="number"
                placeholder="Min"
                :model-value="catalog.minPrice ?? ''"
                class="w-full text-sm"
                @update:model-value="(v: any) => catalog.setPriceRange(v ? Number(v) : null, catalog.maxPrice)"
              />
              <span class="text-neutral-300">&mdash;</span>
              <InputText
                type="number"
                placeholder="Max"
                :model-value="catalog.maxPrice ?? ''"
                class="w-full text-sm"
                @update:model-value="(v: any) => catalog.setPriceRange(catalog.minPrice, v ? Number(v) : null)"
              />
            </div>
          </div>

          <!-- Option Values -->
          <div v-for="opt in catalog.optionTypes" :key="opt.id">
            <h3 class="text-xs font-semibold text-neutral-900 uppercase tracking-wide mb-2">{{ opt.presentation || opt.name }}</h3>
            <div class="space-y-1">
              <label
                v-for="val in opt.values"
                :key="val.id"
                class="flex items-center gap-2 py-1 cursor-pointer text-sm text-neutral-700 hover:text-neutral-900"
              >
                <input
                  type="checkbox"
                  :checked="catalog.selectedOptionValueIds.includes(val.id)"
                  class="rounded border-neutral-300"
                  @change="onOptionValueClick(val.id)"
                />
                <span>{{ val.name }}</span>
              </label>
            </div>
          </div>
        </div>
      </aside>

      <!-- Section: Content Area — sort bar + product grid + pagination -->
      <div class="flex-1 min-w-0">
        <!-- Placeholder for Task 4 -->
        <div class="text-center py-24 text-neutral-500">
          Product grid content will be implemented in the next task.
        </div>
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Verify build**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/features/catalog/views/ShopView.vue
git commit -m "feat(store): implement ShopView filter sidebar with taxonomy tree and option filters"
```

---

### Task 4: ShopView — product grid + sort + pagination

**Files:**
- Modify: `app/Store/src/features/catalog/views/ShopView.vue`

**Interfaces:**
- Consumes: `productListStore` (items, loading, error, page, totalPages, totalCount, goToPage), `catalogStore.sortField`
- This task replaces the placeholder "Product grid content will be implemented in the next task" div from Task 3

- [ ] **Step 1: Replace the placeholder content div in ShopView.vue with:**

Replace the `<div class="flex-1 min-w-0">` content area after the `</aside>` tag:

```html
      <div class="flex-1 min-w-0">
        <!-- Sort Bar -->
        <div class="flex items-center justify-between mb-6">
          <div class="flex items-center gap-4">
            <Button
              icon="pi pi-filter"
              label="Filters"
              severity="secondary"
              outlined
              size="small"
              class="lg:hidden"
              @click="mobileFiltersOpen = true"
            />
            <span class="text-sm text-neutral-500">
              Showing {{ (productList.page - 1) * productList.pageSize + 1 }}&ndash;{{ Math.min(productList.page * productList.pageSize, productList.totalCount) }} of {{ productList.totalCount }}
            </span>
          </div>
          <Select
            :model-value="catalog.sortField"
            :options="sortOptions"
            option-label="label"
            option-value="value"
            size="small"
            class="w-44"
            @update:model-value="(v: string) => catalog.sortField = v"
          />
        </div>

        <!-- Product Grid -->
        <div v-if="productList.loading && productList.items.length === 0" class="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-4">
          <Skeleton v-for="i in 12" :key="i" class="aspect-[3/4] rounded-lg" />
        </div>
        <div v-else-if="productList.error" class="text-center py-16">
          <p class="text-neutral-500 mb-4">{{ productList.error }}</p>
          <Button label="Retry" severity="secondary" outlined @click="productList.refresh()" />
        </div>
        <div v-else-if="productList.items.length === 0" class="text-center py-16">
          <i class="pi pi-search text-4xl text-neutral-300 mb-4 block" />
          <p class="text-neutral-500 mb-4">No products found matching your filters.</p>
          <Button label="Clear all filters" severity="secondary" outlined @click="onClearFilters()" />
        </div>
        <div v-else class="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-4">
          <ProductCard v-for="item in productList.items" :key="item.id" :product="item" />
        </div>

        <!-- Pagination -->
        <Paginator
          v-if="productList.totalPages > 1"
          :rows="productList.pageSize"
          :total-records="productList.totalCount"
          :first="(productList.page - 1) * productList.pageSize"
          class="mt-8"
          @page="(e: { page: number }) => productList.goToPage(e.page + 1)"
        />
      </div>
```

And add the sort options to the script:

```typescript
const sortOptions = [
  { label: 'Newest', value: '-createdAtUtc' },
  { label: 'Price: Low to High', value: 'price' },
  { label: 'Price: High to Low', value: '-price' },
  { label: 'Name A-Z', value: 'name' },
]
```

- [ ] **Step 2: Add mobile filter drawer**

After the closing `</div>` of the main flex container, add:

```html
    <!-- Mobile Filters Drawer -->
    <div v-if="mobileFiltersOpen" class="fixed inset-0 z-50 lg:hidden">
      <div class="absolute inset-0 bg-black/50" @click="mobileFiltersOpen = false" />
      <div class="absolute left-0 top-0 h-full w-72 bg-white shadow-xl overflow-y-auto p-6">
        <div class="flex justify-between items-center mb-6">
          <span class="text-lg font-semibold text-neutral-900">Filters</span>
          <Button icon="pi pi-times" text rounded @click="mobileFiltersOpen = false" />
        </div>
        <!-- Repeat filter content (same as desktop sidebar) -->
        <div class="space-y-6">
          <div v-if="catalog.activeFilterCount > 0" class="flex items-center justify-between">
            <span class="text-sm font-medium text-neutral-900">{{ catalog.activeFilterCount }} active</span>
            <button class="text-xs text-neutral-500 hover:text-neutral-900" @click="onClearFilters()">Clear All</button>
          </div>
          <div v-for="group in catalog.taxonomyGroups" :key="group.taxonomy.id">
            <h3 class="text-xs font-semibold text-neutral-900 uppercase tracking-wide mb-2">{{ group.taxonomy.name }}</h3>
            <div v-for="taxon in group.tree" :key="taxon.id" class="ml-2">
              <label class="flex items-center gap-2 py-1 cursor-pointer text-sm text-neutral-700 hover:text-neutral-900">
                <input
                  type="checkbox"
                  :checked="catalog.selectedTaxonIds.includes(taxon.id)"
                  class="rounded border-neutral-300"
                  @change="onTaxonClick(taxon.id)"
                />
                <span :class="{ 'font-semibold text-neutral-900': catalog.selectedTaxonIds.includes(taxon.id) }">{{ taxon.name }}</span>
              </label>
            </div>
          </div>
        </div>
        <div class="mt-6">
          <Button label="Apply Filters" severity="primary" class="w-full" @click="mobileFiltersOpen = false" />
        </div>
      </div>
    </div>
```

- [ ] **Step 2: Verify build**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/features/catalog/views/ShopView.vue
git commit -m "feat(store): implement ShopView product grid, sort bar, pagination, and mobile filters"
```

---

### Task 5: ProductDetailView — layout, gallery, info, variant selector, ATC

**Files:**
- Modify: `app/Store/src/features/catalog/views/ProductDetailView.vue`

**Interfaces:**
- Consumes: `productDetailStore` (product, loading, error, selectedVariant, selectedVariantId, quantity, stockLabel, isInStock, selectVariant, incrementQuantity, decrementQuantity, addToCart)
- Consumes: `ProductCard` component for similar/related

- [ ] **Step 1: Replace entire ProductDetailView.vue**

```vue
<script setup lang="ts">
import { watch, computed } from 'vue'
import { useRoute } from 'vue-router'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useProductDetailStore } from '../stores/productDetailStore'
import { useCurrency } from '@/shared/utils/currency'
import ProductCard from '../components/ProductCard.vue'

usePageTitle('Product')
const route = useRoute()
const detail = useProductDetailStore()

watch(() => route.params.slug, (slug) => {
  if (typeof slug === 'string') detail.load(slug)
}, { immediate: true })

const product = computed(() => detail.product)
const variant = computed(() => detail.selectedVariant)
const mainImage = computed(() =>
  variant.value?.images?.[0]?.url ?? product.value?.masterVariant?.images?.[0]?.url ?? null
)
const activeImages = computed(() => variant.value?.images?.length ? variant.value.images : (product.value?.masterVariant?.images ?? []))
const displayPrice = computed(() => variant.value?.price ?? product.value?.masterVariant?.price ?? 0)
const formattedPrice = computed(() => useCurrency(displayPrice.value))
const lineTotal = computed(() => useCurrency(displayPrice.value * detail.quantity))

// Group variants by option type for selector rendering
const optionGroups = computed(() => {
  if (!product.value) return []
  const groups: Record<string, { optionName: string; values: { id: string; name: string; variantId: string }[] }> = {}
  for (const v of product.value.variants) {
    for (const ov of v.optionValues) {
      const key = ov.optionTypeId
      if (!groups[key]) groups[key] = { optionName: ov.optionTypeName ?? ov.name, values: [] }
      if (!groups[key].values.find(x => x.variantId === v.id)) {
        groups[key].values.push({ id: ov.id, name: ov.name, variantId: v.id })
      }
    }
  }
  return Object.values(groups)
})

async function onAddToCart(): Promise<void> {
  const added = await detail.addToCart()
  if (added) {
    // Toast is handled by notify system
  }
}

const similarScroll = ref<HTMLElement | null>(null)

function scrollSimilar(dir: 'left' | 'right'): void {
  if (!similarScroll.value) return
  const amount = 300
  similarScroll.value.scrollBy({ left: dir === 'left' ? -amount : amount, behavior: 'smooth' })
}
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Shop', to: '/shop' }, { label: product?.name ?? 'Product' }]" />

    <!-- Loading State -->
    <div v-if="detail.loading" class="flex gap-8 mt-8">
      <Skeleton class="w-7/12 aspect-[3/4] rounded-lg" />
      <div class="w-5/12 space-y-4">
        <Skeleton width="30%" height="1rem" />
        <Skeleton width="80%" height="2rem" />
        <Skeleton width="20%" height="2rem" />
        <Skeleton width="100%" height="1rem" />
        <Skeleton width="60%" height="3rem" />
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="detail.error" class="text-center py-24">
      <p class="text-neutral-500 mb-4">{{ detail.error }}</p>
      <Button label="Back to Shop" severity="secondary" outlined as="router-link" to="/shop" />
    </div>

    <!-- Product Content -->
    <div v-else-if="product" class="mt-8">
      <div class="flex flex-col lg:flex-row gap-8">
        <!-- Section: Gallery — main image + thumbnails -->
        <div class="lg:w-7/12">
          <div class="aspect-[3/4] bg-neutral-100 rounded-lg overflow-hidden">
            <img
              v-if="mainImage"
              :src="mainImage"
              :alt="product.name"
              class="w-full h-full object-cover"
            />
            <div v-else class="w-full h-full flex items-center justify-center text-neutral-300">
              <i class="pi pi-image text-6xl" />
            </div>
          </div>
          <div v-if="activeImages.length > 1" class="flex gap-2 mt-4">
            <button
              v-for="(img, i) in activeImages"
              :key="img.id"
              class="w-20 h-24 rounded-md overflow-hidden border-2 flex-shrink-0"
              :class="mainImage === img.url ? 'border-neutral-900' : 'border-transparent'"
            >
              <img :src="img.url" :alt="img.alt ?? ''" class="w-full h-full object-cover" />
            </button>
          </div>
        </div>

        <!-- Section: Product Info — name, price, variants, add to cart -->
        <div class="lg:w-5/12">
          <p v-if="product.department" class="text-xs text-neutral-500 uppercase tracking-wide mb-1">{{ product.department }}</p>
          <h1 class="text-2xl font-semibold text-neutral-900 mb-3">{{ product.name }}</h1>
          <p class="text-xl font-medium text-neutral-900 mb-4 font-mono">{{ formattedPrice }}</p>
          <p v-if="product.description" class="text-sm text-neutral-600 mb-6 line-clamp-3">{{ product.description }}</p>

          <!-- Variant Selector -->
          <div v-for="group in optionGroups" :key="group.optionName" class="mb-5">
            <p class="text-xs font-semibold text-neutral-500 uppercase tracking-wide mb-2">{{ group.optionName }}</p>
            <div class="flex flex-wrap gap-2">
              <button
                v-for="opt in group.values"
                :key="opt.variantId"
                class="px-4 py-2 text-sm rounded-lg border transition-colors"
                :class="detail.selectedVariantId === opt.variantId
                  ? 'border-neutral-900 bg-neutral-900 text-white'
                  : 'border-neutral-200 text-neutral-700 hover:border-neutral-400'"
                @click="detail.selectVariant(opt.variantId)"
              >{{ opt.name }}</button>
            </div>
          </div>

          <!-- Quantity -->
          <div class="mb-5">
            <p class="text-xs font-semibold text-neutral-500 uppercase tracking-wide mb-2">Quantity</p>
            <div class="flex items-center gap-3">
              <Button icon="pi pi-minus" text rounded size="small" :disabled="detail.quantity <= 1" @click="detail.decrementQuantity()" />
              <span class="text-sm font-medium w-8 text-center">{{ detail.quantity }}</span>
              <Button icon="pi pi-plus" text rounded size="small" :disabled="detail.quantity >= 99" @click="detail.incrementQuantity()" />
            </div>
          </div>

          <!-- Add to Cart -->
          <Button
            :label="detail.isInStock ? `Add to Cart — ${lineTotal}` : 'Out of Stock'"
            severity="primary"
            class="w-full mb-3"
            :disabled="!detail.isInStock || !detail.selectedVariantId"
            @click="onAddToCart()"
          />

          <!-- Stock Label -->
          <p v-if="detail.stockLabel" class="text-sm mb-4" :class="detail.isInStock ? 'text-amber-600' : 'text-red-600'">
            {{ detail.stockLabel }}
          </p>

          <!-- Trust Signals -->
          <div class="text-xs text-neutral-500 space-y-1 pt-4 border-t border-neutral-100">
            <p>&check; Free shipping over $100</p>
            <p>&check; 30-day returns</p>
            <p>&check; Secure checkout</p>
          </div>

          <!-- Wishlist -->
          <Button label="Add to Wishlist" icon="pi pi-heart" severity="secondary" outlined class="w-full mt-5" />
        </div>
      </div>

      <!-- Section: Tabs — description, details, shipping -->
      <Tabs value="0" class="mt-12">
        <TabList>
          <Tab value="0">Description</Tab>
          <Tab value="1">Details</Tab>
          <Tab value="2">Shipping</Tab>
        </TabList>
        <TabPanels>
          <TabPanel value="0">
            <div v-if="product.description" class="text-sm text-neutral-700 leading-relaxed max-w-3xl" v-html="product.description" />
            <p v-else class="text-sm text-neutral-500">No description available.</p>
          </TabPanel>
          <TabPanel value="1">
            <div class="text-sm text-neutral-700 space-y-3 max-w-3xl">
              <div v-if="product.materialComposition" class="flex gap-4">
                <span class="font-medium text-neutral-500 w-36">Material</span>
                <span>{{ product.materialComposition }}</span>
              </div>
              <div v-if="product.careInstructions" class="flex gap-4">
                <span class="font-medium text-neutral-500 w-36">Care</span>
                <span>{{ product.careInstructions }}</span>
              </div>
              <div v-if="product.fitNotes" class="flex gap-4">
                <span class="font-medium text-neutral-500 w-36">Fit</span>
                <span>{{ product.fitNotes }}</span>
              </div>
              <div v-if="product.styleCode" class="flex gap-4">
                <span class="font-medium text-neutral-500 w-36">Style Code</span>
                <span>{{ product.styleCode }}</span>
              </div>
            </div>
          </TabPanel>
          <TabPanel value="2">
            <div class="text-sm text-neutral-700 leading-relaxed max-w-3xl">
              <p class="mb-3">Free standard shipping on orders over $100.</p>
              <p class="mb-3">Standard: 5-7 business days — $5.99</p>
              <p class="mb-3">Express: 2-3 business days — $14.99</p>
              <p>International shipping available to select countries.</p>
            </div>
          </TabPanel>
        </TabPanels>
      </Tabs>

      <!-- Section: Visually Similar — horizontal scroll -->
      <section v-if="detail.similarProducts.length > 0" class="mt-16">
        <h2 class="text-sm font-medium text-neutral-500 uppercase tracking-wide mb-2">Visually Similar</h2>
        <p class="text-sm text-neutral-400 mb-6">AI-powered recommendations based on visual style</p>
        <div class="relative">
          <button
            class="absolute left-0 top-1/2 -translate-y-1/2 -ml-3 z-10 w-8 h-8 bg-white rounded-full shadow-md border border-neutral-200 flex items-center justify-center hover:border-neutral-400"
            @click="scrollSimilar('left')"
          >&larr;</button>
          <div ref="similarScroll" class="flex gap-4 overflow-x-auto scrollbar-hide py-2">
            <div v-for="item in detail.similarProducts" :key="item.id" class="flex-shrink-0 w-44">
              <ProductCard :product="item" :show-similarity="false" />
            </div>
          </div>
          <button
            class="absolute right-0 top-1/2 -translate-y-1/2 -mr-3 z-10 w-8 h-8 bg-white rounded-full shadow-md border border-neutral-200 flex items-center justify-center hover:border-neutral-400"
            @click="scrollSimilar('right')"
          >&rarr;</button>
        </div>
      </section>

      <!-- Section: You May Also Like — grid -->
      <section v-if="detail.relatedProducts.length > 0" class="mt-16 mb-12">
        <h2 class="text-sm font-medium text-neutral-500 uppercase tracking-wide mb-2">You May Also Like</h2>
        <p class="text-sm text-neutral-400 mb-6">Customers who viewed this also bought</p>
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
          <ProductCard v-for="item in detail.relatedProducts" :key="item.id" :product="item" />
        </div>
      </section>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Add missing import for ref in script**

Make sure `ref` is imported from `vue` at the top of the script.

- [ ] **Step 3: Verify build**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/features/catalog/views/ProductDetailView.vue
git commit -m "feat(store): implement ProductDetailView with gallery, variant selector, tabs, and similar/related"
```

---

### Task 6: SearchOverlay — wiring + keyboard navigation

**Files:**
- Modify: `app/Store/src/features/catalog/components/SearchOverlay.vue`

- [ ] **Step 1: Replace SearchOverlay.vue with wired implementation**

```vue
<script setup lang="ts">
import { watch } from 'vue'
import { useSearch } from '../composables/useSearch'

const search = useSearch()

watch(() => search.query.value, () => {
  search.search()
})

function onKeyDown(e: KeyboardEvent): void {
  if (e.key === 'Escape') {
    search.close()
  } else if (e.key === 'ArrowDown') {
    e.preventDefault()
    if (search.selectedIndex.value < search.results.value.length - 1) {
      search.selectedIndex.value++
    }
  } else if (e.key === 'ArrowUp') {
    e.preventDefault()
    if (search.selectedIndex.value > 0) {
      search.selectedIndex.value--
    }
  } else if (e.key === 'Enter') {
    search.navigateToResult(search.selectedIndex.value)
  }
}

function onSelectResult(index: number): void {
  search.navigateToResult(index)
}
</script>
<template>
  <Teleport to="body">
    <div v-if="search.isOpen.value" class="fixed inset-0 z-50 flex items-start justify-center pt-[20vh]">
      <div class="absolute inset-0 bg-black/50" @click="search.close()" />
      <div class="relative bg-white rounded-xl shadow-2xl w-full max-w-2xl mx-4 overflow-hidden">
        <div class="p-4">
          <input
            v-model="search.query.value"
            type="text"
            placeholder="Search products..."
            class="w-full text-lg px-4 py-3 border-0 outline-none"
            autofocus
            @keydown="onKeyDown"
          />
        </div>

        <!-- Loading -->
        <div v-if="search.loading.value" class="border-t border-neutral-100 p-4 space-y-3">
          <Skeleton v-for="i in 3" :key="i" height="2rem" />
        </div>

        <!-- Results -->
        <div v-else-if="search.results.value.length > 0" class="border-t border-neutral-100">
          <button
            v-for="(item, i) in search.results.value"
            :key="item.id"
            class="w-full flex items-center gap-3 px-4 py-3 text-left hover:bg-neutral-50 transition-colors"
            :class="{ 'bg-neutral-50': i === search.selectedIndex.value }"
            @click="onSelectResult(i)"
            @mouseenter="search.selectedIndex.value = i"
          >
            <img
              v-if="item.masterVariant?.images?.[0]?.url"
              :src="item.masterVariant.images[0].url"
              :alt="item.name"
              class="w-10 h-10 object-cover rounded-md bg-neutral-100"
            />
            <div v-else class="w-10 h-10 rounded-md bg-neutral-100 flex items-center justify-center">
              <i class="pi pi-image text-neutral-300 text-sm" />
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-neutral-900 truncate">{{ item.name }}</p>
              <p class="text-xs text-neutral-500" v-if="item.masterVariant?.price">
                ${{ item.masterVariant.price.toFixed(2) }}
              </p>
            </div>
          </button>
        </div>

        <!-- No Results -->
        <div v-else-if="search.query.value.trim()" class="border-t border-neutral-100 p-6 text-center">
          <p class="text-sm text-neutral-500">No products match "{{ search.query.value }}"</p>
        </div>

        <!-- Initial Hint -->
        <div v-else class="border-t border-neutral-100 p-4 text-sm text-neutral-500">
          Type to search products, collections, and more...
        </div>
      </div>
    </div>
  </Teleport>
</template>
```

- [ ] **Step 2: Verify build and search composable API match**

```bash
cd app/Store && npx tsc --noEmit
```

The `useSearch()` composable exposes `.value` refs — ensure template accesses match.

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/features/catalog/components/SearchOverlay.vue
git commit -m "feat(store): wire SearchOverlay with live results, keyboard navigation, and debounce"
```

---

### Task 7: CollectionsView + NotFoundView + static pages

**Files:**
- Modify: `app/Store/src/features/catalog/views/CollectionsView.vue`
- Modify: `app/Store/src/features/catalog/views/NotFoundView.vue`
- Modify: `app/Store/src/features/catalog/views/AboutView.vue`
- Modify: `app/Store/src/features/catalog/views/TermsView.vue`
- Modify: `app/Store/src/features/catalog/views/PrivacyView.vue`

- [ ] **Step 1: Replace CollectionsView.vue**

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useCatalogStore } from '../stores/catalogStore'

usePageTitle('Collections')
const catalog = useCatalogStore()

onMounted(() => catalog.loadTaxonomyGroups())
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Collections' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Collections</h1>
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      <router-link
        v-for="group in catalog.taxonomyGroups"
        :key="group.taxonomy.id"
        :to="`/shop`"
        class="group"
      >
        <div class="aspect-[2/3] bg-neutral-100 rounded-lg overflow-hidden flex items-center justify-center transition-opacity group-hover:opacity-90">
          <span class="text-neutral-400 text-lg">{{ group.taxonomy.name }}</span>
        </div>
        <div class="mt-3">
          <p class="text-lg font-semibold text-neutral-900">{{ group.taxonomy.name }}</p>
          <p class="text-sm text-neutral-500">Browse collection</p>
        </div>
      </router-link>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Replace NotFoundView.vue**

```vue
<script setup lang="ts">
import { usePageTitle } from '@/shared/composables/usePageTitle'
usePageTitle('Not Found')
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-24 text-center">
    <p class="text-6xl font-light text-neutral-300 mb-4">404</p>
    <h1 class="text-2xl font-semibold text-neutral-900 mb-3">Page not found</h1>
    <p class="text-neutral-500 mb-8">The page you're looking for doesn't exist or has been moved.</p>
    <Button label="Back to Home" severity="secondary" outlined as="router-link" to="/" />
  </div>
</template>
```

- [ ] **Step 3: Replace AboutView.vue**

```vue
<script setup lang="ts">
import { usePageTitle } from '@/shared/composables/usePageTitle'
usePageTitle('About')
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'About' }]" />
    <div class="max-w-3xl mx-auto py-12">
      <h1 class="text-2xl font-bold text-neutral-900 mb-8">About ReSys.Shop</h1>
      <div class="prose prose-neutral text-neutral-700 space-y-6 leading-relaxed">
        <p>ReSys.Shop is an AI-powered fashion e-commerce platform that helps you discover clothing and accessories that match your personal style. We combine machine learning with curated selection to make finding the perfect piece effortless.</p>
        <h2 class="text-lg font-semibold text-neutral-900 mt-8 mb-4">Our Story</h2>
        <p>Founded in 2025, ReSys.Shop was born from the idea that shopping for fashion should be personal, intelligent, and enjoyable. Our platform uses computer vision and natural language processing to understand the visual and textual attributes of every product, making recommendations that feel truly personal.</p>
        <h2 class="text-lg font-semibold text-neutral-900 mt-8 mb-4">Our Technology</h2>
        <p>We use state-of-the-art machine learning models to analyze product images, descriptions, and attributes. This allows us to surface products that are visually similar, stylistically complementary, or contextually relevant to what you're looking at.</p>
        <h2 class="text-lg font-semibold text-neutral-900 mt-8 mb-4">Contact</h2>
        <p>Email: hello@resys.shop</p>
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 4: Replace TermsView.vue**

```vue
<script setup lang="ts">
import { usePageTitle } from '@/shared/composables/usePageTitle'
usePageTitle('Terms of Service')
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Terms of Service' }]" />
    <div class="max-w-3xl mx-auto py-12">
      <h1 class="text-2xl font-bold text-neutral-900 mb-8">Terms of Service</h1>
      <div class="prose prose-neutral text-neutral-700 space-y-6 leading-relaxed">
        <p>Last updated: 2026</p>
        <h2 class="text-lg font-semibold text-neutral-900 mt-8 mb-4">Acceptance of Terms</h2>
        <p>By accessing and using ReSys.Shop, you agree to be bound by these Terms of Service. If you do not agree, please do not use our platform.</p>
        <h2 class="text-lg font-semibold text-neutral-900 mt-8 mb-4">Account Terms</h2>
        <p>You are responsible for maintaining the security of your account and password. ReSys.Shop cannot and will not be liable for any loss or damage from your failure to comply with this security obligation.</p>
        <h2 class="text-lg font-semibold text-neutral-900 mt-8 mb-4">Purchases and Payment</h2>
        <p>All purchases made through ReSys.Shop are subject to product availability. Prices are subject to change without notice. We reserve the right to refuse or cancel any order at our sole discretion.</p>
        <h2 class="text-lg font-semibold text-neutral-900 mt-8 mb-4">Returns and Refunds</h2>
        <p>We accept returns within 30 days of delivery. Items must be unworn, unwashed, and in their original condition with all tags attached. Refunds are processed within 5-10 business days after we receive the returned item.</p>
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 5: Replace PrivacyView.vue**

```vue
<script setup lang="ts">
import { usePageTitle } from '@/shared/composables/usePageTitle'
usePageTitle('Privacy Policy')
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Privacy Policy' }]" />
    <div class="max-w-3xl mx-auto py-12">
      <h1 class="text-2xl font-bold text-neutral-900 mb-8">Privacy Policy</h1>
      <div class="prose prose-neutral text-neutral-700 space-y-6 leading-relaxed">
        <p>Last updated: 2026</p>
        <h2 class="text-lg font-semibold text-neutral-900 mt-8 mb-4">Data Collection</h2>
        <p>We collect information you provide when creating an account, making a purchase, or contacting support. This includes your name, email, shipping address, and payment information. Payment details are processed securely by our payment partners and are not stored on our servers.</p>
        <h2 class="text-lg font-semibold text-neutral-900 mt-8 mb-4">How We Use Your Data</h2>
        <p>We use your data to process orders, improve our recommendations, communicate about your orders, and send marketing communications (which you can opt out of at any time). We never sell your personal data to third parties.</p>
        <h2 class="text-lg font-semibold text-neutral-900 mt-8 mb-4">Cookies</h2>
        <p>We use essential cookies for site functionality and analytics cookies to understand how you use our platform. You can manage cookie preferences in your browser settings.</p>
        <h2 class="text-lg font-semibold text-neutral-900 mt-8 mb-4">Your Rights</h2>
        <p>You have the right to access, correct, or delete your personal data. You can also request a copy of your data or restrict its processing. Contact us at privacy@resys.shop for any privacy-related requests.</p>
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 6: Verify build**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 7: Commit**

```bash
git add app/Store/src/features/catalog/views/CollectionsView.vue app/Store/src/features/catalog/views/NotFoundView.vue app/Store/src/features/catalog/views/AboutView.vue app/Store/src/features/catalog/views/TermsView.vue app/Store/src/features/catalog/views/PrivacyView.vue
git commit -m "feat(store): implement CollectionsView, NotFoundView, and static content pages"
```

---

### Task 8: VisualSearchView — upload, preview, results

**Files:**
- Modify: `app/Store/src/features/catalog/views/VisualSearchView.vue`

- [ ] **Step 1: Replace VisualSearchView.vue**

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useVisualSearchStore } from '../stores/visualSearchStore'
import ProductCard from '../components/ProductCard.vue'

usePageTitle('Visual Search')
const vs = useVisualSearchStore()
const fileInput = ref<HTMLInputElement | null>(null)

onMounted(() => vs.loadModels())

function onFileChange(e: Event): void {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) vs.selectFile(file)
}

function onDrop(e: DragEvent): void {
  e.preventDefault()
  const file = e.dataTransfer?.files?.[0]
  if (file) vs.selectFile(file)
}

function onDragOver(e: DragEvent): void {
  e.preventDefault()
}

function onChangeImage(): void {
  vs.reset()
  if (fileInput.value) fileInput.value.value = ''
}

function onSearch(): void {
  vs.search()
}

function formatScore(product: any): string {
  const score = product.similarityScore ?? product.score ?? 0
  return `${(score * 100).toFixed(1)}%`
}
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Visual Search' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Visual Search</h1>
    <p class="text-sm text-neutral-500 mb-8">Find visually similar products by uploading an image</p>

    <!-- Upload State -->
    <div
      v-if="vs.state === 'empty'"
      class="border-2 border-dashed border-neutral-300 rounded-xl py-16 text-center cursor-pointer hover:border-neutral-500 transition-colors"
      @click="fileInput?.click()"
      @drop="onDrop"
      @dragover="onDragOver"
    >
      <i class="pi pi-cloud-upload text-4xl text-neutral-300 mb-4 block" />
      <p class="text-lg font-medium text-neutral-900 mb-2">Upload an image</p>
      <p class="text-sm text-neutral-500">JPEG, PNG, WebP — Max 10 MB</p>
      <input ref="fileInput" type="file" accept="image/jpeg,image/png,image/webp" class="hidden" @change="onFileChange" />
    </div>

    <!-- Upload Selected + Preview -->
    <div v-if="vs.state === 'upload' || vs.state === 'loading'" class="flex items-start gap-6">
      <div class="relative">
        <img v-if="vs.previewUrl" :src="vs.previewUrl" alt="Preview" class="w-40 h-40 object-cover rounded-lg" />
        <button class="absolute top-2 right-2 w-6 h-6 bg-white rounded-full shadow text-xs flex items-center justify-center hover:bg-neutral-100" @click="onChangeImage()">&times;</button>
      </div>
      <div class="flex-1">
        <Select
          v-if="vs.availableModels.length > 0"
          v-model="vs.selectedModelId"
          :options="vs.availableModels"
          option-label="name"
          option-value="id"
          placeholder="Select model"
          class="w-full mb-3"
        />
        <p v-if="vs.validationError" class="text-sm text-red-600 mb-3">{{ vs.validationError }}</p>
        <Button
          label="Search"
          severity="primary"
          :loading="vs.state === 'loading'"
          :disabled="!vs.selectedFile"
          @click="onSearch()"
        />
      </div>
    </div>

    <!-- Loading Skel eton -->
    <div v-if="vs.state === 'loading'" class="grid grid-cols-2 md:grid-cols-4 gap-4 mt-8">
      <Skeleton v-for="i in 8" :key="i" class="aspect-[3/4] rounded-lg" />
    </div>

    <!-- Results -->
    <div v-if="vs.state === 'results'">
      <h2 class="text-sm font-medium text-neutral-500 uppercase tracking-wide mb-6 mt-8">Results ({{ vs.results.length }})</h2>
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <router-link
          v-for="item in vs.results"
          :key="item.id"
          :to="`/products/${item.slug}`"
          class="group block"
        >
          <div class="aspect-[3/4] rounded-lg bg-neutral-100 overflow-hidden relative">
            <img
              v-if="item.imageUrl"
              :src="item.imageUrl"
              :alt="item.name"
              class="w-full h-full object-cover transition-opacity group-hover:opacity-90"
            />
            <div v-else class="w-full h-full flex items-center justify-center text-neutral-300">
              <i class="pi pi-image text-3xl" />
            </div>
            <span class="absolute top-2 right-2 bg-teal-500/90 text-white text-xs font-medium rounded px-1.5 py-0.5">
              {{ formatScore(item) }}
            </span>
          </div>
          <p class="text-sm font-medium text-neutral-900 truncate mt-2">{{ item.name }}</p>
          <p v-if="item.price" class="text-sm font-medium text-neutral-900 font-mono">${{ item.price.toFixed(2) }}</p>
        </router-link>
      </div>
    </div>

    <!-- Error Toast -->
    <Message v-if="vs.error" severity="error" class="mt-4">{{ vs.error }}</Message>
    <Message v-if="vs.validationError && vs.state === 'empty'" severity="error" class="mt-4">{{ vs.validationError }}</Message>
  </div>
</template>
```

- [ ] **Step 2: Verify build**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/features/catalog/views/VisualSearchView.vue
git commit -m "feat(store): implement VisualSearchView with upload, preview, model selector, and results"
```

---

### Task 9: Smoke tests

**Files:**
- Create: `app/Store/src/features/catalog/views/__tests__/HomeView.spec.ts`
- Create: `app/Store/src/features/catalog/views/__tests__/ShopView.spec.ts`
- Create: `app/Store/src/features/catalog/components/__tests__/ProductCard.spec.ts`

- [ ] **Step 1: Write ProductCard smoke test**

Create `app/Store/src/features/catalog/components/__tests__/ProductCard.spec.ts`:

```typescript
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import ProductCard from '../ProductCard.vue'

const mockProduct = {
  id: '1',
  masterVariantId: 'v1',
  name: 'Silk Dress',
  status: 'active',
  description: 'A beautiful dress',
  slug: 'silk-dress',
  styleCode: null,
  seasonName: null,
  materialComposition: null,
  careInstructions: null,
  fitNotes: null,
  department: 'Women',
  genderTarget: null,
  variantsCount: 1,
  availableOn: null,
  masterVariant: {
    id: 'v1',
    sku: 'SKU-001',
    isMaster: true,
    price: 89.99,
    currency: 'USD',
    optionValues: [],
    images: [],
    prices: [],
    stock: { availableQuantity: 10, backorderable: false },
  },
  classifications: [],
}

const router = createRouter({
  history: createMemoryHistory(),
  routes: [{ path: '/products/:slug', component: { template: '<div />' } }],
})

describe('ProductCard', () => {
  it('renders product name', () => {
    const wrapper = mount(ProductCard, {
      props: { product: mockProduct },
      global: { plugins: [router] },
    })
    expect(wrapper.text()).toContain('Silk Dress')
  })

  it('renders product price', () => {
    const wrapper = mount(ProductCard, {
      props: { product: mockProduct },
      global: { plugins: [router] },
    })
    expect(wrapper.text()).toContain('$89.99')
  })

  it('renders link to product detail', () => {
    const wrapper = mount(ProductCard, {
      props: { product: mockProduct },
      global: { plugins: [router] },
    })
    const link = wrapper.find('a')
    expect(link.attributes('href')).toContain('/products/silk-dress')
  })
})
```

- [ ] **Step 2: Write HomeView smoke test**

Create `app/Store/src/features/catalog/views/__tests__/HomeView.spec.ts`:

```typescript
import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import HomeView from '../HomeView.vue'

vi.mock('@/shared/composables/usePageTitle', () => ({
  usePageTitle: vi.fn(),
}))

const router = createRouter({
  history: createMemoryHistory(),
  routes: [
    { path: '/', component: { template: '<div />' } },
    { path: '/shop', component: { template: '<div />' } },
    { path: '/products/:slug', component: { template: '<div />' } },
  ],
})

describe('HomeView', () => {
  it('renders the editorial headline', () => {
    const wrapper = mount(HomeView, {
      global: {
        plugins: [router, createTestingPinia({ createSpy: vi.fn })],
        stubs: { ProductCard: { template: '<div class="product-card" />' } },
      },
    })
    expect(wrapper.html()).toContain('Curated fashion')
  })

  it('renders the hero CTA button', () => {
    const wrapper = mount(HomeView, {
      global: {
        plugins: [router, createTestingPinia({ createSpy: vi.fn })],
        stubs: { ProductCard: { template: '<div class="product-card" />' } },
      },
    })
    expect(wrapper.html()).toContain('Shop New Arrivals')
  })
})
```

- [ ] **Step 3: Write ShopView smoke test**

Create `app/Store/src/features/catalog/views/__tests__/ShopView.spec.ts`:

```typescript
import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import ShopView from '../ShopView.vue'

vi.mock('@/shared/composables/usePageTitle', () => ({
  usePageTitle: vi.fn(),
}))

const router = createRouter({
  history: createMemoryHistory(),
  routes: [
    { path: '/', component: { template: '<div />' } },
    { path: '/shop', component: { template: '<div />' } },
    { path: '/products/:slug', component: { template: '<div />' } },
  ],
})

describe('ShopView', () => {
  it('renders the breadcrumb', () => {
    const wrapper = mount(ShopView, {
      global: {
        plugins: [router, createTestingPinia({ createSpy: vi.fn })],
        stubs: { ProductCard: { template: '<div class="product-card" />' } },
      },
    })
    expect(wrapper.text()).toContain('Shop')
  })

  it('renders filter sidebar sections', () => {
    const wrapper = mount(ShopView, {
      global: {
        plugins: [router, createTestingPinia({ createSpy: vi.fn })],
        stubs: { ProductCard: { template: '<div class="product-card" />' } },
      },
    })
    expect(wrapper.html()).toContain('Price')
  })
})
```

- [ ] **Step 4: Run smoke tests**

```bash
cd app/Store && npx vitest run src/features/catalog/
```

- [ ] **Step 5: Commit**

```bash
git add app/Store/src/features/catalog/components/__tests__/ProductCard.spec.ts app/Store/src/features/catalog/views/__tests__/HomeView.spec.ts app/Store/src/features/catalog/views/__tests__/ShopView.spec.ts
git commit -m "test(store): add smoke tests for ProductCard, HomeView, and ShopView"
```

---

### Task 10: Full verification

- [ ] **Step 1: Run all tests**

```bash
cd app/Store && npx vitest run
```

- [ ] **Step 2: Run type check**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 3: Run build**

```bash
cd app/Store && pnpm run build-only
```

- [ ] **Step 4: Commit verification**

```bash
git status
```
