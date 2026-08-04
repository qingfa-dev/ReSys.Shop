import apiClient from '@/common/api/http/api.client'
import { CatalogDashboardMapper } from './catalog-dashboard.mapper'
import type { CatalogDashboardResponse } from '../types/catalog-dashboard.types'

export const catalogDashboardService = {
  async fetchDashboard(): Promise<CatalogDashboardResponse> {
    const res = await apiClient.get('/catalog/dashboard')
    return CatalogDashboardMapper.toDashboard(res.data as Record<string, unknown>)
  },
}
