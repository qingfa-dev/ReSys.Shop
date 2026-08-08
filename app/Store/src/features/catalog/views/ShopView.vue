<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useCatalogStore } from '../stores/catalogStore'
import { useProductListStore } from '../stores/productListStore'
import ProductCard from '../components/ProductCard.vue'
import TaxonTree from '../components/TaxonTree.vue'

usePageTitle('Shop')
const catalog = useCatalogStore()
const productList = useProductListStore()
const mobileFiltersOpen = ref(false)

const sortOptions = [
  { label: 'Newest', value: '-CreatedAtUtc' },
  { label: 'Price: Low to High', value: 'Price' },
  { label: 'Price: High to Low', value: '-Price' },
  { label: 'Name: A-Z', value: 'Name' },
  { label: 'Name: Z-A', value: '-Name' },
]

// Derive: paginator first index (0-based) from store page (1-based)
const pageFirst = computed({
  get: () => (productList.page - 1) * productList.pageSize,
  set: (val: number) => {
    productList.goToPage(Math.floor(val / productList.pageSize) + 1)
  },
})

onMounted(() => {
  catalog.loadTaxonomyGroups()
  catalog.loadOptionTypes()
  productList.init()
})

function onOptionValueClick(id: string): void {
  catalog.toggleOptionValue(id)
}

function onClearFilters(): void {
  catalog.clearFilters()
}

function onSortChange(event: { value: string }): void {
  catalog.setSort(event.value)
}

function onPage(event: { page: number }): void {
  productList.goToPage(event.page + 1)
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

          <!-- Taxonomy Tree — collapsible, searchable, depth-indented -->
          <div v-for="group in catalog.taxonomyGroups" :key="group.taxonomy.id">
            <h3 class="text-xs font-semibold text-neutral-900 uppercase tracking-wide mb-2">{{ group.taxonomy.name }}</h3>
            <TaxonTree
              :nodes="group.tree"
              :selected-ids="catalog.selectedTaxonIds"
              :show-search="group.tree.length > 4"
              :max-visible="8"
              @toggle="catalog.toggleTaxon($event)"
            />
          </div>

          <!-- Price Range -->
          <div>
            <h3 class="text-xs font-semibold text-neutral-900 uppercase tracking-wide mb-2">Price</h3>
            <div class="flex items-center gap-2">
              <InputText
                type="number"
                placeholder="Min"
                :model-value="catalog.minPrice != null ? String(catalog.minPrice) : ''"
                class="w-full text-sm"
                @update:model-value="(v: any) => catalog.setPriceRange(v ? Number(v) : null, catalog.maxPrice)"
              />
              <span class="text-neutral-300">&mdash;</span>
              <InputText
                type="number"
                placeholder="Max"
                :model-value="catalog.maxPrice != null ? String(catalog.maxPrice) : ''"
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
        <!-- Sort Bar — sort dropdown + mobile filter toggle + result count -->
        <div class="flex items-center justify-between mb-6">
          <p class="text-sm text-neutral-500">
            {{ productList.totalCount }} product{{ productList.totalCount !== 1 ? 's' : '' }}
          </p>
          <div class="flex items-center gap-3">
            <button
              class="lg:hidden flex items-center gap-1.5 rounded-lg border border-neutral-200 px-3 py-1.5 text-sm text-neutral-700 hover:bg-neutral-50"
              @click="mobileFiltersOpen = true"
            >
              <i class="pi pi-filter text-sm" />
              Filters
              <span v-if="catalog.activeFilterCount > 0" class="rounded-full bg-teal-500 px-1.5 py-0.5 text-xs text-white">
                {{ catalog.activeFilterCount }}
              </span>
            </button>
            <Select
              :model-value="catalog.sortField"
              :options="sortOptions"
              option-label="label"
              option-value="value"
              class="w-48"
              @change="onSortChange"
            />
          </div>
        </div>

        <!-- Loading State -->
        <div v-if="productList.loading" class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
          <div v-for="i in 8" :key="i" class="animate-pulse">
            <div class="aspect-[3/4] rounded-lg bg-neutral-100" />
            <div class="mt-2 space-y-1.5">
              <div class="h-3 w-16 rounded bg-neutral-100" />
              <div class="h-4 w-3/4 rounded bg-neutral-100" />
              <div class="h-4 w-1/3 rounded bg-neutral-100" />
            </div>
          </div>
        </div>

        <!-- Error State -->
        <div v-else-if="productList.error" class="rounded-lg border border-red-200 bg-red-50 p-6 text-center">
          <p class="text-sm text-red-800">{{ productList.error }}</p>
          <Button label="Try again" severity="danger" text size="small" class="mt-3" @click="productList.refresh()" />
        </div>

        <!-- Empty State -->
        <div v-else-if="productList.items.length === 0" class="py-24 text-center">
          <i class="pi pi-search text-5xl text-neutral-300 mb-4 block" />
          <h3 class="mt-3 text-sm font-medium text-neutral-900">No products found</h3>
          <p class="mt-1 text-sm text-neutral-500">Try adjusting your filters or search.</p>
          <Button label="Clear all filters" severity="secondary" text size="small" class="mt-4" @click="onClearFilters()" />
        </div>

        <!-- Product Grid -->
        <div v-else class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
          <ProductCard
            v-for="product in productList.items"
            :key="product.id"
            :product="product"
          />
        </div>

        <!-- Pagination -->
        <div v-if="productList.totalCount > productList.pageSize" class="mt-8 flex justify-center">
          <Paginator
            v-model:first="pageFirst"
            :rows="productList.pageSize"
            :total-records="productList.totalCount"
            :page-link-limit="5"
            @page="onPage"
          />
        </div>
      </div>
    </div>
  </div>

  <!-- Section: Mobile Filter Drawer — full-screen overlay for small screens -->
  <Teleport to="body">
    <Transition name="fade">
        <div v-if="mobileFiltersOpen" class="fixed inset-0 z-50 lg:hidden">
          <!-- Overlay backdrop -->
          <div class="absolute inset-0 bg-black/40" @click="mobileFiltersOpen = false" />

          <!-- Drawer panel -->
          <div class="absolute inset-y-0 left-0 w-full max-w-sm overflow-y-auto bg-white p-6 shadow-xl">
            <div class="flex items-center justify-between">
              <h2 class="text-lg font-semibold text-neutral-900">Filters</h2>
              <Button icon="pi pi-times" text rounded severity="secondary" @click="mobileFiltersOpen = false" />
            </div>

            <!-- Active Filters Header -->
            <div v-if="catalog.activeFilterCount > 0" class="mt-4 flex items-center justify-between">
              <span class="text-sm font-medium text-neutral-900">{{ catalog.activeFilterCount }} active</span>
              <button class="text-xs text-neutral-500 hover:text-neutral-900" @click="onClearFilters()">Clear All</button>
            </div>

            <!-- Taxonomy Tree — collapsible, searchable, depth-indented -->
            <div v-for="group in catalog.taxonomyGroups" :key="group.taxonomy.id" class="mt-6">
              <h3 class="text-xs font-semibold text-neutral-900 uppercase tracking-wide mb-2">{{ group.taxonomy.name }}</h3>
              <TaxonTree
                :nodes="group.tree"
                :selected-ids="catalog.selectedTaxonIds"
                :show-search="group.tree.length > 4"
                :max-visible="8"
                @toggle="catalog.toggleTaxon($event)"
              />
            </div>

            <!-- Price Range -->
            <div class="mt-6">
              <h3 class="text-xs font-semibold text-neutral-900 uppercase tracking-wide mb-2">Price</h3>
              <div class="flex items-center gap-2">
                <InputText
                  type="number"
                  placeholder="Min"
                  :model-value="catalog.minPrice != null ? String(catalog.minPrice) : ''"
                  class="w-full text-sm"
                  @update:model-value="(v: any) => catalog.setPriceRange(v ? Number(v) : null, catalog.maxPrice)"
                />
                <span class="text-neutral-300">&mdash;</span>
                <InputText
                  type="number"
                  placeholder="Max"
                  :model-value="catalog.maxPrice != null ? String(catalog.maxPrice) : ''"
                  class="w-full text-sm"
                  @update:model-value="(v: any) => catalog.setPriceRange(catalog.minPrice, v ? Number(v) : null)"
                />
              </div>
            </div>

            <!-- Option Values -->
            <div v-for="opt in catalog.optionTypes" :key="opt.id" class="mt-6">
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
        </div>
      </Transition>
  </Teleport>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
