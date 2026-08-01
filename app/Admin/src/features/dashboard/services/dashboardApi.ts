import { get } from '@/shared/api/client'
import { DASHBOARD } from '@/shared/constants/api'
import type { Result } from '@/shared/types'
import type { DashboardSummary } from '../types/dashboard'

export class DashboardApi {
  private static readonly BASE = DASHBOARD

  static getDashboard(): Promise<Result<DashboardSummary>> {
    return get<Result<DashboardSummary>>(DashboardApi.BASE)
  }
}
