export interface RecentProductData {
  id: string
  name: string
  slug: string
  createdAtUtc: string
}

export interface CatalogDashboard {
  totalProducts: number
  activeProducts: number
  draftProducts: number
  totalVariants: number
  totalTaxonomies: number
  totalTaxons: number
  recentProducts: RecentProductData[]
}
