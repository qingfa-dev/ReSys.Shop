import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { ShippingMethodListItem } from '../types/shippingMethod'
import { ShippingMethodApi } from '../services/shippingMethodApi'

export const useShippingMethodStore = defineStore('shippingMethods', () => {
  const activeShippingMethods = ref<ShippingMethodListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return
    const result = await ShippingMethodApi.getShippingMethods({ pageSize: 100, sortBy: 'name', sortDirection: 'asc' })
    if (result.isSuccess) {
      activeShippingMethods.value = result.items
      loaded.value = true
    }
  }

  return { activeShippingMethods, loaded, fetchActive }
})
