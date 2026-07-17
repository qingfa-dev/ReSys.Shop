import { defineStore } from 'pinia'
import { ref } from 'vue'
import { orderingDashboardService } from '../services/ordering-dashboard.service'
import type { OrderingDashboardResponse } from '../types/ordering-dashboard.types'

export const useOrderingDashboardStore = defineStore('ordering-dashboard', () => {
  const data = ref<OrderingDashboardResponse | null>(null)
  const loading = ref(false)

  async function fetchDashboard() {
    loading.value = true
    try {
      const { data: response } = await orderingDashboardService.fetchDashboard()
      data.value = { ...response }
    } finally {
      loading.value = false
    }
  }

  return { data, loading, fetchDashboard }
})
