import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { OrderListItem } from '../types/order'
import { OrderApi } from '../services/orderApi'

export const useOrderStore = defineStore('orders', () => {
  const activeOrders = ref<OrderListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return
    const result = await OrderApi.getOrders({ pageSize: 100, sortBy: 'createdAtUtc', sortDirection: 'desc' })
    if (result.isSuccess) {
      activeOrders.value = result.items
      loaded.value = true
    }
  }

  return { activeOrders, loaded, fetchActive }
})
