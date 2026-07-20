import { defineStore } from 'pinia'
import { ref } from 'vue'
import { shippingMethodService } from '../services/shipping-method.service'
import type { ShippingMethodListItemModel, ShippingMethodDetailModel } from '../types/shipping-method.model.type'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'

export const useShippingMethodStore = defineStore('shippingMethod', () => {
  const items = ref<ShippingMethodListItemModel[]>([])
  const current = ref<ShippingMethodDetailModel | null>(null)
  const loading = ref(false)
  const totalRecords = ref(0)
  const query = ref<ServerQueryingParameters>({ page: 1, pageSize: 20, sort: ['name'] })

  async function fetchItems(params?: ServerQueryingParameters) {
    loading.value = true
    query.value = { ...query.value, ...params }
    const result = await shippingMethodService.list(query.value)
    if (result.isSuccess) { items.value = result.items; totalRecords.value = result.totalCount }
    loading.value = false
    return result
  }

  async function fetchById(id: string) {
    const result = await shippingMethodService.getById(id)
    if (result.isSuccess) current.value = result.value
    return result
  }

  return { items, current, loading, totalRecords, query, fetchItems, fetchById }
})
