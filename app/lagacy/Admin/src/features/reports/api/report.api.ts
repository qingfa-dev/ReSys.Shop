import apiClient from '@/shared/api/http/api.client'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { SalesSummary, InventorySummary, CatalogSummary, RecentActivityResponse } from '../types/report.response.type'
interface ReportEndpoints {
  getSalesSummary(params?: Record<string, unknown>): Promise<ServerResult<SalesSummary>>
  getInventorySummary(): Promise<ServerResult<InventorySummary>>
  getCatalogSummary(): Promise<ServerResult<CatalogSummary>>
  getRecentActivity(): Promise<ServerResult<RecentActivityResponse>>
}

export const reportApi: ReportEndpoints = {
  async getSalesSummary(params) {
    return apiClient.get('/reports/sales', { params }).then(res => res.data as ServerResult<SalesSummary>)
  },
  async getInventorySummary() {
    return apiClient.get('/reports/inventory').then(res => res.data as ServerResult<InventorySummary>)
  },
  async getCatalogSummary() {
    return apiClient.get('/reports/catalog').then(res => res.data as ServerResult<CatalogSummary>)
  },
  async getRecentActivity() {
    return apiClient.get('/reports/activity').then(res => res.data as ServerResult<RecentActivityResponse>)
  },
}
