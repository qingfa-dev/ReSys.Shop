import { defineStore } from 'pinia'
import { ref } from 'vue'
import { transferService } from '../services/transfer.service'
import type { StockTransfer } from '../types/stock-transfer.response.type'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'

export const useTransferStore = defineStore('transfer', () => {
  const items = ref<StockTransfer[]>([])
  const loading = ref(false)
  const totalRecords = ref(0)
  const query = ref<ServerQueryingParameters>({ page: 1, pageSize: 10, sort: ['-createdAtUtc'] })

  async function fetchItems(params: ServerQueryingParameters = {}) {
    loading.value = true
    query.value = { ...query.value, ...params }
    const result = await transferService.listTransfers(query.value)
    if (result.isSuccess) { items.value = result.items; totalRecords.value = result.totalCount || 0 }
    loading.value = false
    return result
  }

  return { items, loading, totalRecords, query, fetchItems }
})
