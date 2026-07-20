import apiClient from '@/common/api/http/api.client'
import type { AxiosResponse } from 'axios'

export interface CatalogDashboardResponse {
  totalProducts: number
  activeProducts: number
  draftProducts: number
  totalVariants: number
  totalTaxonomies: number
  totalTaxons: number
  recentProducts: Array<{
    id: string
    name: string
    slug: string
    createdAtUtc: string
  }>
}

export const catalogDashboardService = {
  fetchDashboard(): Promise<AxiosResponse<CatalogDashboardResponse>> {
    return apiClient.get('/catalog/dashboard')
  },
}
