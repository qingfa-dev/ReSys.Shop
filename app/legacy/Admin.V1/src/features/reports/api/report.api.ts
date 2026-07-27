import apiClient from '@/common/api/http/api.client'
import type { ServerResult } from '@/common/api/types/result.types'
import type { SalesSummary, InventorySummary, CatalogSummary, RecentActivityResponse } from '../types/report.response'
interface ReportEndpoints {
  getSalesSummary(params?: Record<string, unknown>): Promise<ServerResult<SalesSummary>>
  getInventorySummary(): Promise<ServerResult<InventorySummary>>
  getCatalogSummary(): Promise<ServerResult<CatalogSummary>>
  getRecentActivity(): Promise<ServerResult<RecentActivityResponse>>
  fetchDashboard(): Promise<{
    data: {
      sales: SalesSummary
      inventory: InventorySummary
      catalog: CatalogSummary
      recentActivities: Array<{ id: string; type: string; title: string; description: string; status: string; timestamp: string }>
    }
  }>
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
  async fetchDashboard() {
    const [sales, inventory, catalog, activity] = await Promise.all([
      this.getSalesSummary(),
      this.getInventorySummary(),
      this.getCatalogSummary(),
      this.getRecentActivity(),
    ])
    return {
      data: {
        sales: sales.value!,
        inventory: inventory.value!,
        catalog: catalog.value!,
        recentActivities: (activity.value?.items ?? []).map((item: any) => ({
          id: item.id,
          type: item.type,
          title: item.title,
          description: item.description,
          status: item.status,
          timestamp: item.timestamp,
        })),
      },
    }
  },
}
