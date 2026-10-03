export interface RecentProduct {
  id: string;
  name: string;
  slug: string;
  created_at: string;
}

export interface CatalogSummary {
  total_products: number;
  active_products: number;
  total_variants: number;
  total_taxonomies: number;
  total_taxons: number;
  total_digital_products: number;
  recently_added: RecentProduct[];
}
