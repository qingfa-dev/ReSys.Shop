import { ref } from 'vue'
import { ProductApi } from '../services/productApi'
import type { StoreProductListItemResponse } from '../types'

// Cache: Singleton shared across all components — avoids duplicate search state
let shared: ReturnType<typeof createSearch> | null = null

function createSearch() {
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

  async function search(): Promise<void> {
    // Guard: Skip search on empty query
    if (!query.value.trim()) { results.value = []; return }
    // Throttle: Debounce rapid keystrokes — 300ms delay between API calls
    if (debounceTimer) clearTimeout(debounceTimer)
    debounceTimer = setTimeout(async () => {
      loading.value = true
      error.value = null
      // Call: Catalog API — search products by name or description
      const result = await ProductApi.getProducts({ pageNumber: 1, pageSize: 5, search: query.value.trim() })
      if (result.isSuccess) results.value = result.items
      else error.value = result.message ?? 'Search failed'
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

export function useSearch() {
  // Cache: Return existing singleton or create new one
  if (!shared) shared = createSearch()
  return shared
}
