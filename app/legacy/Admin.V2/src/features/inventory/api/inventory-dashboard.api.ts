import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { InventoryDashboardResponse } from '../types'

export class InventoryDashboardApi {
  static async get(): Promise<Result<InventoryDashboardResponse>> {
    const res = await apiClient.get<Result<InventoryDashboardResponse>>('/inventory/dashboard')
    return res.data
  }
}
