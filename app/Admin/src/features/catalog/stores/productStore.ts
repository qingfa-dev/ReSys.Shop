import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { ProductListItem } from '../types/product'
import { ProductApi } from '../services/productApi'

export const useProductStore = defineStore('products', () => {
  const activeProducts = ref<ProductListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return

    const result = await ProductApi.getProducts({
      status: 'Active',
      pageSize: 100,
      sortBy: 'name',
      sortDirection: 'asc',
    })

    if (result.isSuccess) {
      activeProducts.value = result.items
      loaded.value = true
    }
  }

  return { activeProducts, loaded, fetchActive }
})
