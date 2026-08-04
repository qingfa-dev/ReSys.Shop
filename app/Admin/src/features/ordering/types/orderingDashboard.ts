export interface RecentOrderData {
  id: string
  number: string
  total: number
  status: string
  createdAtUtc: string
}

export interface OrderStatusBreakdownData {
  draft: number
  placed: number
  canceled: number
  expired: number
}

export interface OrderingDashboard {
  totalOrders: number
  pendingFulfillment: number
  todayOrders: number
  averageOrderValue: number
  totalRevenue: number
  recentOrders: RecentOrderData[]
  statusBreakdown: OrderStatusBreakdownData
}
