export interface SalesSummary {
  total_revenue: number;
  order_count: number;
  average_order_value: number;
  revenue_trend_percentage: number;
  trend_history?: Array<{ date: string; revenue: number }>;
}

export interface InventorySummary {
  total_variants: number;
  out_of_stock_count: number;
  low_stock_count: number;
  stock_accuracy_percentage: number;
}

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

export interface ActivityItem {
  id: string;
  type: 'Order' | 'Stock';
  title: string;
  description: string;
  status: string;
  timestamp: string;
}

export interface RecentActivityResponse {
  items: ActivityItem[];
}

export interface DashboardQuery {
  from?: string;
  to?: string;
}
