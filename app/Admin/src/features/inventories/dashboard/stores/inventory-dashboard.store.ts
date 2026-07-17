import { defineStore } from 'pinia'
import { ref } from 'vue'
import { inventoryDashboardService } from '../services/inventory-dashboard.service'
import type { InventoryDashboardResponse } from '../types/inventory-dashboard.types'

export const useInventoryDashboardStore = defineStore('inventory-dashboard', () => {
  const data = ref<InventoryDashboardResponse | null>(null)
  const loading = ref(false)

  async function fetchDashboard() {
    loading.value = true
    try {
      const { data: response } = await inventoryDashboardService.fetchDashboard()
      data.value = { ...response }
    } finally {
      loading.value = false
    }
  }

  return { data, loading, fetchDashboard }
})
