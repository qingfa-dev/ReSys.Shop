export interface RecentProduct {
  id: string;
  name: string;
  slug: string;
  createdAtUtc: string;
}

export interface CatalogSummary {
  totalProducts: number;
  activeProducts: number;
  totalVariants: number;
  totalTaxonomies: number;
  totalTaxons: number;
  totalDigitalProducts: number;
  recentlyAdded: RecentProduct[];
}
