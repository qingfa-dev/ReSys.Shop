import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { ReportsData } from '../types'

export class DashboardApi {
  static async get(): Promise<Result<ReportsData>> {
    const res = await apiClient.get<Result<ReportsData>>('/api/dashboard')
    return res.data
  }
}
