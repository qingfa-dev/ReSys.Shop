import { defineStore } from 'pinia'
import { ref } from 'vue'
import { shippingRateService } from '../services/shipping-rate.service'
import type { ShippingRateListItem, ShippingRateDetail } from '../types/ShippingRate.Response.Type'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export const useShippingRateStore = defineStore('shippingRate', () => {
  const items = ref<ShippingRateListItem[]>([])
  const current = ref<ShippingRateDetail | null>(null)
  const loading = ref(false)
  const totalRecords = ref(0)
  const query = ref<ServerQueryingParameters>({ page: 1, pageSize: 20 })

  async function fetchItems(params?: ServerQueryingParameters) {
    loading.value = true
    query.value = { ...query.value, ...params }
    const result = await shippingRateService.list(query.value)
    if (result.isSuccess) { items.value = result.items; totalRecords.value = result.totalCount }
    loading.value = false
    return result
  }

  async function fetchById(id: string) {
    const result = await shippingRateService.getById(id)
    if (result.isSuccess) current.value = result.value
    return result
  }

  return { items, current, loading, totalRecords, query, fetchItems, fetchById }
})
