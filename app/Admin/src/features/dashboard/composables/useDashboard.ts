import { ref } from 'vue'
import type { Ref } from 'vue'
import type { Result } from '@/shared/types'
import type { DashboardSummary } from '../types/dashboard'
import { DashboardApi } from '../services/dashboardApi'

export interface UseDashboardState {
  summary: Ref<DashboardSummary | null>
  loading: Ref<boolean>
  error: Ref<string | null>
  fetchDashboard: () => Promise<Result<DashboardSummary>>
}

export function useDashboard() {
  const summary = ref<DashboardSummary | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchDashboard(): Promise<Result<DashboardSummary>> {
    loading.value = true
    error.value = null
    const result = await DashboardApi.getDashboard()
    loading.value = false
    if (result.isSuccess) {
      summary.value = result.value
    } else {
      error.value = result.message ?? result.errors[0]?.message ?? 'Request failed.'
    }
    return result
  }

  return { summary, loading, error, fetchDashboard }
}
