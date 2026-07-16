import { apiClient } from '@/shared/api/http/api-client'
import type { AxiosResponse } from 'axios'

export interface DashboardResponse {
  sales: {
    totalRevenue: number
    orderCount: number
    averageOrderValue: number
    revenueTrendPercentage: number
    trendHistory: Array<{ date: string; revenue: number }>
  }
  inventory: {
    totalVariants: number
    outOfStockCount: number
    lowStockCount: number
    stockAccuracyPercentage: number
  }
  catalog: {
    totalProducts: number
    activeProducts: number
    totalVariants: number
    totalTaxonomies: number
    totalTaxons: number
    recentlyAdded: Array<{ id: string; name: string; slug: string; createdAtUtc: string }>
  }
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
