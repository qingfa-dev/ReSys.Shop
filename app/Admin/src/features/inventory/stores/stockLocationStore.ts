import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { StockLocationListItem } from '../types/stockLocation'
import { StockLocationApi } from '../services/stockLocationApi'

export const useStockLocationStore = defineStore('stockLocations', () => {
  const activeStockLocations = ref<StockLocationListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return
    const result = await StockLocationApi.getStockLocations({
      pageSize: 100,
      sortBy: 'name',
      sortDirection: 'asc',
    })
    if (result.isSuccess) {
      activeStockLocations.value = result.items
      loaded.value = true
    }
  }

  return { activeStockLocations, loaded, fetchActive }
})
