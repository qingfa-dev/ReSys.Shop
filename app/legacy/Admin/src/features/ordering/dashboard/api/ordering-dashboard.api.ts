import apiClient from '@/common/api/http/api.client'
import type { AxiosResponse } from 'axios'
import type { OrderingDashboardResponse } from '../types/ordering-dashboard.types'

export const orderingDashboardService = {
  fetchDashboard(): Promise<AxiosResponse<OrderingDashboardResponse>> {
    return apiClient.get('/ordering/dashboard')
  },
}
