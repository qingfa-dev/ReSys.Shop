import apiClient from '@/common/api/http/api.client'
import type { AxiosResponse } from 'axios'
import type { InventoryDashboardResponse } from '../types/inventory-dashboard.types'

export const inventoryDashboardService = {
  fetchDashboard(): Promise<AxiosResponse<InventoryDashboardResponse>> {
    return apiClient.get('/inventory/dashboard')
  },
}
