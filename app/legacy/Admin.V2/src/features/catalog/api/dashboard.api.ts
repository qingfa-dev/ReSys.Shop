import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { CatalogDashboardResponse } from '../types'

export class CatalogDashboardApi {
  static async get(): Promise<Result<CatalogDashboardResponse>> {
    const res = await apiClient.get<Result<CatalogDashboardResponse>>('/catalog/dashboard')
    return res.data
  }
}
