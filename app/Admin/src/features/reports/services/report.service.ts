import { reportApi } from '../api/report.api'
import type { SalesSummary, InventorySummary, CatalogSummary } from '../types/report.response.type'

export interface DashboardResponse {
  sales: SalesSummary
  inventory: InventorySummary
  catalog: CatalogSummary
  recentActivities: Array<{
    id: string
    type: string
    title: string
    description: string
    status: string
    timestamp: string
  }>
}

export const reportService = {
  async fetchDashboard() {
    const [sales, inventory, catalog, activity] = await Promise.all([
      reportApi.getSalesSummary(),
      reportApi.getInventorySummary(),
      reportApi.getCatalogSummary(),
      reportApi.getRecentActivity(),
    ])

    return {
      data: {
        sales: sales.value,
        inventory: inventory.value,
        catalog: catalog.value,
        recentActivities: (activity.value?.items ?? []).map(item => ({
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
