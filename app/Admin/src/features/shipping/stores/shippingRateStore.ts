import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { ShippingRateListItem } from '../types/shippingRate'
import { ShippingRateApi } from '../services/shippingRateApi'

export const useShippingRateStore = defineStore('shippingRates', () => {
  const activeShippingRates = ref<ShippingRateListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return
    const result = await ShippingRateApi.getShippingRates({ pageSize: 100, sortBy: 'name', sortDirection: 'asc' })
    if (result.isSuccess) {
      activeShippingRates.value = result.items
      loaded.value = true
    }
  }

  return { activeShippingRates, loaded, fetchActive }
})
