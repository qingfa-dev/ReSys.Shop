import { get } from '@/shared/api/client'

import type { Result } from '@/shared/types'
import type { DashboardSummary } from '../types/dashboard'

export class DashboardApi {
  private static readonly BASE = 'api/admin/dashboard'

  static getDashboard(): Promise<Result<DashboardSummary>> {
    return get<Result<DashboardSummary>>(DashboardApi.BASE)
  }
}
