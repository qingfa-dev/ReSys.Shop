import apiClient from '@/shared/api/api.client';
import type { ApiResult } from '@/shared/api/api.types';
import type {
  SalesSummary,
  InventorySummary,
  CatalogSummary,
  RecentActivityResponse,
  DashboardQuery
} from '../types/report.types';

export const reportService = {
  async getSalesSummary(params: DashboardQuery = {}): Promise<ApiResult<SalesSummary>> {
    return apiClient.get('admin/dashboard/sales-summary', { params });
  },

  async getInventorySummary(): Promise<ApiResult<InventorySummary>> {
    return apiClient.get('admin/dashboard/inventory-summary');
  },

  async getCatalogSummary(): Promise<ApiResult<CatalogSummary>> {
    return apiClient.get('admin/dashboard/catalog-summary');
  },

  async getRecentActivity(limit: number = 10): Promise<ApiResult<RecentActivityResponse>> {
    return apiClient.get('admin/dashboard/recent-activity', { params: { limit } });
  }
};
