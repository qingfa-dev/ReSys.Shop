import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { ReportsData } from '../types'

// Stub — no backend endpoint exists yet for reports.
// Replace `/dashboard` with `/reports/dashboard` when the backend endpoint is added.
export class DashboardApi {
  static async get(): Promise<Result<ReportsData>> {
    const res = await apiClient.get<Result<ReportsData>>('/dashboard')
    return res.data
  }
}
