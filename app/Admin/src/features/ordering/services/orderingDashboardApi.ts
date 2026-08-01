import { get } from '@/shared/api/client'
import { ORDERING } from '@/shared/constants/api'
import type { Result } from '@/shared/types'
import type { OrderingDashboard } from '../types/orderingDashboard'

export class OrderingDashboardApi {
  private static readonly BASE = `${ORDERING}/dashboard`

  static getOrderingDashboard(): Promise<Result<OrderingDashboard>> {
    return get<Result<OrderingDashboard>>(OrderingDashboardApi.BASE)
  }
}
