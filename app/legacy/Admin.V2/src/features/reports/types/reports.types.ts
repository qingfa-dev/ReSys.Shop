export interface SalesTrend {
  month: string
  sales: number
  orders: number
}

export interface RevenueByCategory {
  category: string
  revenue: number
  percentage: number
}

export interface OrderStatusStats {
  status: string
  count: number
}

export interface ReportsData {
  totalRevenue: number
  totalOrders: number
  totalCustomers: number
  averageOrderValue: number
  revenueDelta: number
  ordersDelta: number
  customersDelta: number
  aovDelta: number
  salesTrends: SalesTrend[]
  revenueByCategory: RevenueByCategory[]
  orderStatusStats: OrderStatusStats[]
}
