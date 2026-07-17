import apiClient from '@/shared/api/api.client';
import type { ApiResult } from '@/shared/api/api.types';
import type { CatalogSummary } from '../types/catalog-dashboard.types';

export const catalogDashboardService = {
  async getSummary(): Promise<ApiResult<CatalogSummary>> {
    return apiClient.get('/admin/dashboard/catalog-summary');
  }
};
