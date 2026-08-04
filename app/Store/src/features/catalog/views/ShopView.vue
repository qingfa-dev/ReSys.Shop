<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useCatalogStore } from '../stores/catalogStore'
import { getTaxonomyTree } from '../services/taxonApi'
import { getOptionTypes } from '../services/optionTypeApi'
import { ENDPOINTS } from '@/shared/constants/api'
import ProductGrid from '../components/ProductGrid.vue'
import CategoryTree from '../components/CategoryTree.vue'
import FilterSidebar from '../components/FilterSidebar.vue'
import type { StoreProductListItemResponse } from '../types/product'
import type { StoreTaxonomyTreeResponse } from '../types/taxon'
import type { StoreOptionTypeResponse } from '../types/optionType'

const route = useRoute()
const catalog = useCatalogStore()

// Map: Build paged query URL from catalogStore filter state
const query = usePagedQuery<StoreProductListItemResponse>(
  () => {
    const params = new URLSearchParams()
    if (catalog.searchQuery) params.append('search', catalog.searchQuery)
    if (catalog.selectedTaxonId) params.append('taxonId', catalog.selectedTaxonId)
    catalog.selectedOptionValueIds.forEach(id => params.append('optionValueId', id))
    if (catalog.minPrice != null) params.append('minPrice', String(catalog.minPrice))
    if (catalog.maxPrice != null) params.append('maxPrice', String(catalog.maxPrice))
    const qs = params.toString()
    return qs ? `${ENDPOINTS.products}?${qs}` : ENDPOINTS.products
  },
  { defaultPageSize: 20, defaultSort: catalog.sortField ? [catalog.sortField] : [], immediate: false },
)
const { items, loading, error, totalCount, totalPages, page, pageSize, refresh, setPage, setSort } = query

// State: Taxonomy tree and filters
const taxonomyTree = ref<StoreTaxonomyTreeResponse | null>(null)
const treeError = ref<string | null>(null)
const optionTypes = ref<StoreOptionTypeResponse[]>([])
const treeLoading = ref(true)
const filtersLoading = ref(true)

// Trigger: Reset to the first page and re-fetch after a filter change
function applyFilters(): void {
  page.value = 1
  refresh()
}

// Map: Sort dropdown options using the querying sort DSL
const sortOptions = [
  { label: 'Newest', value: '-createdAtUtc' },
  { label: 'Price: Low-High', value: 'Variants.Prices.Amount' },
  { label: 'Price: High-Low', value: '-Variants.Prices.Amount' },
]

// Trigger: Load taxonomy, option filters, and initial products on mount
onMounted(async () => {
  // Sync: Initialize search query from the URL
  const searchParam = route.query.search
  if (typeof searchParam === 'string') catalog.setSearch(searchParam)

  // Load: Fetch taxonomy tree and option types in parallel
  const [treeResult, otResult] = await Promise.all([
    getTaxonomyTree('00000000-0000-0000-0000-000000000001'), // TODO: wire real taxonomy id
    getOptionTypes({ pageNumber: 1, pageSize: 50 }),
  ])
  if (treeResult.isSuccess) taxonomyTree.value = treeResult.value
  else treeError.value = treeResult.message ?? 'Categories are unavailable.'
  if (otResult.isSuccess) optionTypes.value = otResult.items
  treeLoading.value = false
  filtersLoading.value = false

  // Trigger: Initial products fetch
  refresh()
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
    <div class="flex gap-8">
      <!-- Section: Sidebar Filters -->
      <aside class="w-64 shrink-0 hidden lg:block space-y-6">
        <!-- Section: Category Tree -->
        <div v-if="treeLoading" class="space-y-2">
          <Skeleton width="80%" height="1rem" v-for="i in 5" :key="i" />
        </div>
        <CategoryTree
          v-else-if="taxonomyTree"
          :nodes="taxonomyTree.nodes"
          @select="(id) => { catalog.setTaxon(id); applyFilters() }"
        />
        <p v-else class="text-sm text-gray-400">{{ treeError ?? 'Categories are unavailable.' }}</p>

        <!-- Section: Option Filters -->
        <FilterSidebar
          v-if="!filtersLoading"
          :option-types="optionTypes"
          :selected-ids="catalog.selectedOptionValueIds"
          @toggle="(id) => { catalog.toggleOptionValue(id); applyFilters() }"
          @clear="catalog.clearFilters(); applyFilters()"
        />
      </aside>

      <!-- Section: Main Content -->
      <div class="flex-1 min-w-0">
        <!-- Section: Toolbar -->
        <div class="flex items-center justify-between mb-6">
          <p class="text-sm text-gray-500">{{ totalCount }} products</p>
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

        <!-- Section: Product Grid -->
        <ProductGrid
          :products="items"
          :loading="loading"
          :error="error"
          @reload="refresh"
          @add-to-cart="(id) => { /* wired in Phase 4 */ }"
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
