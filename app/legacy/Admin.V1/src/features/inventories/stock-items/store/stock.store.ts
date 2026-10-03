import { defineStore } from 'pinia'
import { ref } from 'vue'
import { stockRepository } from '../api/stock.api'
import type { StockItem } from '../types/stock-item.response'
import type { StockItemQuery } from '../types/stock-item.query'

export const useStockStore = defineStore('stock', () => {
  const items = ref<StockItem[]>([])
  const loading = ref(false)
  const totalRecords = ref(0)
  const query = ref<StockItemQuery>({ page: 1, pageSize: 10, search: '', sort: ['-countOnHand'] })

  async function fetchItems(params: StockItemQuery = {}) {
    loading.value = true
    query.value = { ...query.value, ...params }
    const result = await stockRepository.list(query.value)
    if (result.isSuccess) { items.value = result.items; totalRecords.value = result.totalCount || 0 }
    loading.value = false
    return result
  }

  return { items, loading, totalRecords, query, fetchItems }
})
