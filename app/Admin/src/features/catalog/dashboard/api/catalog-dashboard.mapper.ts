import type { CatalogDashboardResponse, RecentProduct } from '../types/catalog-dashboard.types'

export const CatalogDashboardMapper = {
  toRecentProduct(dto: Record<string, unknown>): RecentProduct {
    return {
      id: String(dto.id ?? ''),
      name: String(dto.name ?? ''),
      slug: String(dto.slug ?? ''),
      createdAtUtc: String(dto.createdAtUtc ?? ''),
    }
  },

  toDashboard(dto: Record<string, unknown>): CatalogDashboardResponse {
    return {
      totalProducts: Number(dto.totalProducts ?? 0),
      activeProducts: Number(dto.activeProducts ?? 0),
      draftProducts: Number(dto.draftProducts ?? 0),
      totalVariants: Number(dto.totalVariants ?? 0),
      totalTaxonomies: Number(dto.totalTaxonomies ?? 0),
      totalTaxons: Number(dto.totalTaxons ?? 0),
      recentProducts: ((dto.recentProducts as Record<string, unknown>[]) ?? []).map((r) => this.toRecentProduct(r)),
    }
  },
}
