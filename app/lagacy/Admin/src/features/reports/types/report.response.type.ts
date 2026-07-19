export interface SalesSummary {
  totalSales: number; totalOrders: number; averageOrderValue: number
  salesGrowth: number; salesTarget: number; targetProgress: number
  dailySales: Array<{ date: string; revenue: number }>
  weeklySales: Array<{ date: string; revenue: number }>
  monthlySales: Array<{ date: string; revenue: number }>
}

export interface InventorySummary {
  totalVariants: number; outOfStockCount: number; lowStockCount: number; stockAccuracyPercentage: number
}

export interface RecentProduct { id: string; name: string; slug: string; createdAtUtc: string }

export interface CatalogSummary {
  totalProducts: number; activeProducts: number; totalVariants: number
  totalTaxonomies: number; totalTaxons: number; recentlyAdded: RecentProduct[]
}

export interface ActivityItem {
  id: string; type: 'Order' | 'Stock'; title: string; description: string; status: string; timestamp: string
}

export interface RecentActivityResponse { items: ActivityItem[] }
