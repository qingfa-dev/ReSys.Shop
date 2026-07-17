import apiClient from '@/shared/api/http/api.client'
import type { AxiosResponse } from 'axios'
import type { SalesSummary, InventorySummary, CatalogSummary } from '../types/Report.Response.Type'

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
  fetchDashboard(): Promise<AxiosResponse<DashboardResponse>> {
    return apiClient.get('/dashboard')
  },
}
