import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { OrderingDashboardResponse } from '../types'

export class OrderingDashboardApi {
  static async get(): Promise<Result<OrderingDashboardResponse>> {
    const res = await apiClient.get<Result<OrderingDashboardResponse>>('/ordering/dashboard')
    return res.data
  }
}
