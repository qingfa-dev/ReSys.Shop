import { defineStore } from 'pinia'
import { ref } from 'vue'
import { locationService } from '../services/location.service'
import type { StockLocation } from '../types/stock-location.response.type'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'

export const useLocationStore = defineStore('location', () => {
  const items = ref<StockLocation[]>([])
  const loading = ref(false)
  const totalRecords = ref(0)
  const query = ref<ServerQueryingParameters>({ page: 1, pageSize: 20, sort: ['name'] })

  async function fetchItems(params: ServerQueryingParameters = {}) {
    loading.value = true
    query.value = { ...query.value, ...params }
    const result = await locationService.listLocations(query.value)
    if (result.isSuccess) { items.value = result.items; totalRecords.value = result.totalCount || 0 }
    loading.value = false
    return result
  }

  return { items, loading, totalRecords, query, fetchItems }
})
