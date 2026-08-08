import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { ProductApi } from '../services/productApi'
import { useCatalogStore } from './catalogStore'
import { on } from '@/shared/composables/useStoreEvents'
import type { StoreProductListItemResponse } from '../types'

export const useProductListStore = defineStore('productList', () => {
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

  async function fetch(): Promise<void> {
    // Guard: Prevent concurrent fetch requests
    if (loading.value) return
    loading.value = true
    error.value = null
    // Call: Catalog API — fetch products with catalog store's active filters
    const catalog = useCatalogStore()
    const result = await ProductApi.getProducts({
      pageNumber: page.value,
      pageSize: pageSize.value,
      search: catalog.searchQuery || undefined,
      sort: [catalog.sortField],
    })
    if (result.isSuccess) {
      items.value = result.items
      totalCount.value = result.totalCount
    } else {
      error.value = result.message ?? 'Failed to load products'
    }
    loading.value = false
    isInitialLoad.value = false
  }

  function markStale(): void {
    // Throttle: Debounce filter changes to avoid rapid API calls during user interaction
    page.value = 1
    if (_fetchTimer) clearTimeout(_fetchTimer)
    _fetchTimer = setTimeout(() => fetch(), 300)
  }

  function nextPage(): void { if (page.value < totalPages.value) { page.value++; fetch() } }
  function prevPage(): void { if (page.value > 1) { page.value--; fetch() } }
  function goToPage(p: number): void { page.value = Math.max(1, Math.min(p, totalPages.value)); fetch() }
  function refresh(): void { fetch() }

  function init(): void {
    // Subscribe: Listen for filter changes from catalogStore to refetch products
    on('filter:changed', () => markStale())
    fetch()
  }

  return {
    items, loading, error, page, pageSize, totalCount, totalPages, isInitialLoad,
    fetch, nextPage, prevPage, goToPage, refresh, init,
  }
})
