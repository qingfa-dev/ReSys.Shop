import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { DashboardSummary } from '../types/dashboard'
import { DashboardApi } from '../services/dashboardApi'

export const useDashboardStore = defineStore('dashboard', () => {
  const summary = ref<DashboardSummary | null>(null)
  const loaded = ref(false)

  async function fetchDashboard(): Promise<void> {
    if (loaded.value) return
    const result = await DashboardApi.getDashboard()
    if (result.isSuccess) {
      summary.value = result.value
      loaded.value = true
    }
  }

  return { summary, loaded, fetchDashboard }
})
