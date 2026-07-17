import { defineStore } from 'pinia'
import { ref } from 'vue'
import { orderingDashboardService } from '../services/ordering-dashboard.service'
import type { OrderingDashboardResponse } from '../types/ordering-dashboard.types'

export const useOrderingDashboardStore = defineStore('ordering-dashboard', () => {
  const data = ref<OrderingDashboardResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchDashboard() {
    loading.value = true
    error.value = null
    try {
      const { data: response } = await orderingDashboardService.fetchDashboard()
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
