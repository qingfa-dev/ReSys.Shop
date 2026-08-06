<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useCatalogStore } from '../stores/catalogStore'
import { useCartStore } from '@/features/ordering/stores/cartStore'
import { useWishlistStore } from '@/features/profile/stores/wishlistStore'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { getTaxonomies, getTaxons } from '../services/taxonApi'
import { getOptionTypes } from '../services/optionTypeApi'
import { getOptionValues } from '../services/optionValueApi'
import { buildTaxonTree } from '../utils/taxonTree'
import { ENDPOINTS } from '@/shared/constants/api'
import ProductGrid from '../components/ProductGrid.vue'
import FilterSidebar from '../components/FilterSidebar.vue'
import type { StoreProductListItemResponse } from '../types/product'
import type { TaxonomyGroup } from '../types/taxon'
import type { StoreOptionTypeListItem, StoreOptionValueListItemResponse } from '../types/optionType'

const route = useRoute()
const catalog = useCatalogStore()
const cart = useCartStore()
const wishlist = useWishlistStore()
const notify = useNotify()
const { handleError } = useApiErrorHandler()

// State: Grid or list layout for product cards
const viewMode = ref<'grid' | 'list'>('grid')

// Map: Build paged query URL from catalogStore filter state
const query = usePagedQuery<StoreProductListItemResponse>(
  () => {
    const params = new URLSearchParams()
    if (catalog.searchQuery) params.append('search', catalog.searchQuery)
    catalog.selectedTaxonIds.forEach(id => params.append('taxonId', id))
    catalog.selectedOptionValueIds.forEach(id => params.append('optionValueId', id))
    if (catalog.minPrice != null) params.append('minPrice', String(catalog.minPrice))
    if (catalog.maxPrice != null) params.append('maxPrice', String(catalog.maxPrice))
    const qs = params.toString()
    return qs ? `${ENDPOINTS.products}?${qs}` : ENDPOINTS.products
  },
  { defaultPageSize: 20, defaultSort: catalog.sortField ? [catalog.sortField] : [], immediate: false },
)
const { items, loading, error, totalCount, totalPages, page, pageSize, refresh, setPage, setSort } = query

// State: Taxonomy groups and filters
const taxonomyGroups = ref<TaxonomyGroup[]>([])
const optionTypes = ref<(StoreOptionTypeListItem & { values: StoreOptionValueListItemResponse[] })[]>([])
const filtersLoading = ref(true)

// Trigger: Reset to the first page and re-fetch after a filter change
function applyFilters(): void {
  page.value = 1
  refresh()
}

// Map: Breadcrumb trail for the shop page
const breadcrumbItems = computed(() => [
  { label: 'Home', to: '/' },
  { label: 'Shop' },
])

const sortOptions = [
  { label: 'Newest', value: '-createdAtUtc' },
  { label: 'Price: Low-High', value: 'Variants.Prices.Amount' },
  { label: 'Price: High-Low', value: '-Variants.Prices.Amount' },
]

// Load: Fetch taxonomies + taxons, group into taxonomy groups with trees
async function loadTaxonomyGroups(): Promise<TaxonomyGroup[]> {
  const [taxonomiesResult, taxonsResult] = await Promise.all([
    getTaxonomies({ pageNumber: 1, pageSize: 50 }),
    getTaxons({ pageNumber: 1, pageSize: 999 }),
  ])
  if (!taxonomiesResult.isSuccess || !taxonsResult.isSuccess) return []
  const taxons = taxonsResult.items
  return taxonomiesResult.items.map(taxonomy => ({
    taxonomy,
    tree: buildTaxonTree(taxons, taxonomy.id),
  }))
}

// State: Variant currently being quick-added (drives the card button loading state).
const quickAddLoading = ref<string | null>(null)

// Trigger: Quick-add the master variant of a product card to the cart.
async function quickAdd(variantId: string): Promise<void> {
  if (!variantId) {
    notify.warn('Unavailable', 'This product has no purchasable variant')
    return
  }
  quickAddLoading.value = variantId
  try {
    const ok = await cart.addItem(variantId, 1)
    if (ok) notify.success('Added to cart')
    else handleError(new Error(cart.error ?? 'Could not add item'))
  } catch {
    handleError(new Error(cart.error ?? 'Could not add item'))
  } finally {
    quickAddLoading.value = null
  }
}

// Trigger: Load taxonomy groups, option filters, and initial products on mount
onMounted(async () => {
  // Sync: Initialize search query from the URL
  const searchParam = route.query.search
  if (typeof searchParam === 'string') catalog.setSearch(searchParam)

  // Load: Fetch taxonomy groups, option types, and option values in parallel
  const [groups, otResult, ovResult] = await Promise.all([
    loadTaxonomyGroups(),
    getOptionTypes({ pageNumber: 1, pageSize: 50 }),
    getOptionValues({ pageNumber: 1, pageSize: 200 }),
  ])
  taxonomyGroups.value = groups
  if (otResult.isSuccess) {
    const valuesByType = new Map<string, StoreOptionValueListItemResponse[]>()
    if (ovResult.isSuccess) {
      for (const v of ovResult.items) {
        const list = valuesByType.get(v.optionTypeId) ?? []
        list.push(v)
        valuesByType.set(v.optionTypeId, list)
      }
    }
    optionTypes.value = otResult.items.map(t => ({
      ...t,
      values: valuesByType.get(t.id) ?? [],
    }))
  }
  filtersLoading.value = false

  // Trigger: Initial products fetch
  refresh()

  // Trigger: Load wishlist state for heart icons
  wishlist.fetchWishlistedIds()
})

// Trigger: Keep the catalog search query in sync with the URL
watch(() => route.query.search, (val) => {
  catalog.setSearch(typeof val === 'string' ? val : '')
  applyFilters()
})
</script>
<template>
  <!-- Section: Shop Page -->
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Section: Breadcrumb -->
    <Breadcrumb :model="breadcrumbItems" class="mb-4" />
    <div class="flex gap-8">
      <!-- Section: Sidebar Filters -->
      <aside class="w-64 shrink-0 hidden lg:block space-y-6">
        <FilterSidebar
          v-if="!filtersLoading"
          :taxonomy-groups="taxonomyGroups"
          :option-types="optionTypes"
          :selected-taxon-ids="catalog.selectedTaxonIds"
          :selected-option-value-ids="catalog.selectedOptionValueIds"
          @toggle-taxon="(id) => { catalog.toggleTaxon(id); applyFilters() }"
          @toggle-option-value="(id) => { catalog.toggleOptionValue(id); applyFilters() }"
          @clear="catalog.clearFilters(); applyFilters()"
        />
      </aside>

      <!-- Section: Main Content -->
      <div class="flex-1 min-w-0">
        <!-- Section: Toolbar -->
        <div class="flex items-center justify-between mb-6">
          <p class="text-sm text-stone-500">{{ totalCount }} products</p>
          <div class="flex items-center gap-3">
            <SelectButton
              v-model="viewMode"
              :options="[
                { icon: 'pi pi-th-large', value: 'grid' },
                { icon: 'pi pi-list', value: 'list' }
              ]"
              option-label="icon"
              option-value="value"
            />
            <Select
              :model-value="catalog.sortField"
              :options="sortOptions"
              option-label="label"
              option-value="value"
              placeholder="Sort by"
              class="w-48"
              @update:model-value="(val: string) => { catalog.sortField = val; setSort(val ? [val] : []) }"
            />
          </div>
        </div>

        <!-- Section: Product Grid -->
        <ProductGrid
          :products="items"
          :loading="loading"
          :error="error"
          :loading-variant-id="quickAddLoading"
          :wishlisted-variant-ids="wishlist.wishlistedVariantIds"
          :view-mode="viewMode"
          @reload="refresh"
          @add-to-cart="quickAdd"
          @toggle-wishlist="(id) => wishlist.toggleWishlist(id)"
        />

        <!-- Section: Pagination -->
        <Paginator
          v-if="totalPages > 1"
          :rows="pageSize"
          :total-records="totalCount"
          :first="(page - 1) * pageSize"
          @page="(e: { page: number }) => setPage(e.page + 1)"
          class="mt-6"
        />
      </div>
    </div>
  </div>
</template>
