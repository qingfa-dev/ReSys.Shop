import type { OrderResponse } from './order.response'

export interface OrderingDashboardResponse {
  totalOrders: number
  pendingOrders: number
  completedOrders: number
  cancelledOrders: number
  totalRevenue: number
  todayRevenue: number
  recentOrders: OrderResponse[]
}
