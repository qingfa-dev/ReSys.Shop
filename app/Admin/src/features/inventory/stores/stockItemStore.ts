import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { StockItemListItem } from '../types/stockItem'
import { StockItemApi } from '../services/stockItemApi'

export const useStockItemStore = defineStore('stockItems', () => {
  const activeStockItems = ref<StockItemListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return
    const result = await StockItemApi.getStockItems({ pageSize: 100 })
    if (result.isSuccess) {
      activeStockItems.value = result.items
      loaded.value = true
    }
  }

  return { activeStockItems, loaded, fetchActive }
})
