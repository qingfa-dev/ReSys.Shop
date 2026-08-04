import { ref } from 'vue'
import { debounce } from '@/shared/utils/debounce'
import { ProductApi } from '../services/productApi'
import type { ProductListItem } from '../types/product'

const PAGE_SIZE = 25

export function useProductOptions() {
  const options = ref<ProductListItem[]>([])
  const loading = ref(false)
  const search = ref('')
  const selectedId = ref<string | null>(null)
  const loadedFor = ref<string | null>(null)
  let requestSeq = 0

  async function fetchOptions(term: string): Promise<void> {
    const seq = ++requestSeq
    loading.value = true
    try {
      const result = await ProductApi.getProducts({
        search: term,
        page: 1,
        pageSize: PAGE_SIZE,
        sortBy: 'name',
      })
      if (seq !== requestSeq) return
      if (result.isSuccess) {
        options.value = result.items
        loadedFor.value = term
      }
    } finally {
      if (seq === requestSeq) loading.value = false
    }
  }

  const searchProducts = debounce(async (term: string) => {
    search.value = term
    if (loadedFor.value === term) return
    await fetchOptions(term)
  }, 300)

  async function loadInitial(): Promise<void> {
    await fetchOptions('')
  }

  return { options, loading, search, selectedId, searchProducts, loadInitial }
}
