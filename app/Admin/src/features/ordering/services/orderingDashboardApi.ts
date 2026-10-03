import { get } from '@/shared/api/client'

import type { Result } from '@/shared/types'
import type { OrderingDashboard } from '../types/orderingDashboard'

export class OrderingDashboardApi {
  static getOrderingDashboard(): Promise<Result<OrderingDashboard>> {
    return get<Result<OrderingDashboard>>('/api/admin/ordering/dashboard')
  }
}
