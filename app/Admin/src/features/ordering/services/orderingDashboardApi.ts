import { get } from '@/shared/api/client'

import type { Result } from '@/shared/types'
import type { OrderingDashboard } from '../types/orderingDashboard'

export class OrderingDashboardApi {
  private static readonly BASE = 'api/admin/ordering/dashboard'

  static getOrderingDashboard(): Promise<Result<OrderingDashboard>> {
    return get<Result<OrderingDashboard>>(OrderingDashboardApi.BASE)
  }
}
