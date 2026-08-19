import { ref, computed, reactive } from 'vue'
import { ProductApi } from '../services/productApi'
import { on } from '@/shared/composables/useStoreEvents'
import { useFilters } from './useFilters'
import { HttpError } from '@/shared/api'
import type { StoreProductListItemResponse } from '../types'

// Module-level singleton state
const items = ref<StoreProductListItemResponse[]>([])
const loading = ref(false)
const error = ref<string | null>(null)
const page = ref(1)
const pageSize = ref(20)
const totalCount = ref(0)
const isInitialLoad = ref(true)
let _fetchTimer: ReturnType<typeof setTimeout> | null = null

// Compute: Total pages for pagination controls
const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))

async function fetchProducts(): Promise<void> {
  if (loading.value) return
  loading.value = true
  error.value = null
  try {
    const filters = useFilters()
    const result = await ProductApi.getProducts({
      pageNumber: page.value,
      pageSize: pageSize.value,
      search: filters.searchQuery || undefined,
      sort: [filters.sortField],
      taxonIds: filters.selectedTaxonIds,
      optionValueIds: filters.selectedOptionValueIds,
      minPrice: filters.minPrice ?? undefined,
      maxPrice: filters.maxPrice ?? undefined,
    })
    if (result.isSuccess) {
      items.value = result.items
      totalCount.value = result.totalCount
    } else {
      error.value = result.message ?? 'Failed to load products'
    }
  } catch (e) {
    error.value = e instanceof HttpError ? e.errors[0]?.message ?? 'Failed to load products' : 'Failed to load products'
  }
  loading.value = false
  isInitialLoad.value = false
}

function markStale(): void {
  page.value = 1
  if (_fetchTimer) clearTimeout(_fetchTimer)
  _fetchTimer = setTimeout(() => fetchProducts(), 300)
}

// Subscribe: Listen for filter changes to refetch products
on('filter:changed', () => markStale())

export function useProducts() {
  function nextPage(): void { if (page.value < totalPages.value) { page.value++; fetchProducts() } }
  function prevPage(): void { if (page.value > 1) { page.value--; fetchProducts() } }
  function goToPage(p: number): void { page.value = Math.max(1, Math.min(p, totalPages.value)); fetchProducts() }
  function refresh(): void { fetchProducts() }

  return reactive({
    items, loading, error, page, pageSize, totalCount, totalPages, isInitialLoad,
    fetch: fetchProducts, markStale, nextPage, prevPage, goToPage, refresh,
  })
}
