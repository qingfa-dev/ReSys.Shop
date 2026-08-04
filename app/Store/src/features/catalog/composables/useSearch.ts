import { ref } from 'vue'
import { getPagedProducts } from '../services/productApi'
import type { StoreProductListItemResponse } from '../types/product'

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

export function useSearch() {
  if (!shared) shared = createSearch()
  return shared
}