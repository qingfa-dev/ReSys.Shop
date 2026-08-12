import { get } from '@/shared/api/client'

import type { Result } from '@/shared/types'
import type { DashboardSummary } from '../types/dashboard'

export class DashboardApi {
  static getDashboard(): Promise<Result<DashboardSummary>> {
    return get<Result<DashboardSummary>>('/api/admin/dashboard')
  }
}
