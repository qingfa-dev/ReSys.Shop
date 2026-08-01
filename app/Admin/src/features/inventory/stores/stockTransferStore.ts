import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { StockTransferListItem } from '../types/stockTransfer'
import { StockTransferApi } from '../services/stockTransferApi'

export const useStockTransferStore = defineStore('stockTransfers', () => {
  const activeStockTransfers = ref<StockTransferListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return
    const result = await StockTransferApi.getStockTransfers({
      pageSize: 100,
      sortBy: 'createdAtUtc',
      sortDirection: 'desc',
    })
    if (result.isSuccess) {
      activeStockTransfers.value = result.items
      loaded.value = true
    }
  }

  return { activeStockTransfers, loaded, fetchActive }
})
