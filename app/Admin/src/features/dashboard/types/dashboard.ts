export interface TrendPoint {
  date: string
  revenue: number
}

export interface SalesSummaryData {
  totalRevenue: number
  orderCount: number
  averageOrderValue: number
  revenueTrendPercentage: number
  trendHistory: TrendPoint[]
}

export interface InventorySummaryData {
  totalVariants: number
  outOfStockCount: number
  lowStockCount: number
  stockAccuracyPercentage: number
}

export interface RecentProductData {
  id: string
  name: string
  slug: string
  createdAtUtc: string
}

export interface CatalogSummaryData {
  totalProducts: number
  activeProducts: number
  totalVariants: number
  totalTaxonomies: number
  totalTaxons: number
  recentlyAdded: RecentProductData[]
}

export interface ActivityItemData {
  id: string
  type: string
  title: string
  description: string
  status: string
  timestamp: string
}

export interface DashboardSummary {
  sales: SalesSummaryData
  inventory: InventorySummaryData
  catalog: CatalogSummaryData
  recentActivities: ActivityItemData[]
}
