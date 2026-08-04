export interface RecentProductData {
  id: string
  name: string
  slug: string
  createdAtUtc: string
}

export interface CatalogDashboardResponse {
  totalProducts: number
  activeProducts: number
  draftProducts: number
  totalVariants: number
  totalTaxonomies: number
  totalTaxons: number
  recentProducts: RecentProductData[]
}
