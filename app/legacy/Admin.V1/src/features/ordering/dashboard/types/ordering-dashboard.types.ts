export interface RecentOrder {
  id: string
  number: string
  total: number
  status: string
  createdAtUtc: string
}

export interface OrderStatusBreakdown {
  draft: number
  placed: number
  canceled: number
  expired: number
}

export interface OrderingDashboardResponse {
  totalOrders: number
  pendingFulfillment: number
  todayOrders: number
  averageOrderValue: number
  totalRevenue: number
  recentOrders: RecentOrder[]
  statusBreakdown: OrderStatusBreakdown
}
