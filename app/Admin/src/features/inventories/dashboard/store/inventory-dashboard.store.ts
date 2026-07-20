import { defineStore } from 'pinia'
import { ref } from 'vue'
import { inventoryDashboardService } from '../api/inventory-dashboard.api'
import type { InventoryDashboardResponse } from '../types/inventory-dashboard.types'

export const useInventoryDashboardStore = defineStore('inventory-dashboard', () => {
  const data = ref<InventoryDashboardResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchDashboard() {
    loading.value = true
    error.value = null
    try {
      const { data: response } = await inventoryDashboardService.fetchDashboard()
      data.value = { ...response }
    } catch (e) {
      error.value = 'Failed to load dashboard data'
      data.value = null
    } finally {
      loading.value = false
    }
  }

  return { data, loading, error, fetchDashboard }
})
