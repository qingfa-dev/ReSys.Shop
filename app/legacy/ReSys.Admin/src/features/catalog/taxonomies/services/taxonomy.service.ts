import apiClient from '@/shared/api/api.client';
import type { PagedList, ApiResult } from '@/shared/api/api.types';
import type { 
  TaxonomyListItem, 
  TaxonomyDetail, 
  CreateTaxonomyRequest, 
  UpdateTaxonomyRequest, 
  TaxonomyQuery 
} from '../types/taxonomy.types';

export const taxonomyService = {
  async list(params: TaxonomyQuery): Promise<ApiResult<TaxonomyListItem[]>> {
    return apiClient.get('/admin/catalog/taxonomies', { params });
  },

  async getById(id: string): Promise<ApiResult<TaxonomyDetail>> {
    return apiClient.get(`/admin/catalog/taxonomies/${id}`);
  },

  async create(data: CreateTaxonomyRequest): Promise<ApiResult<TaxonomyDetail>> {
    return apiClient.post('/admin/catalog/taxonomies', data);
  },

  async update(id: string, data: UpdateTaxonomyRequest): Promise<ApiResult<TaxonomyDetail>> {
    return apiClient.put(`/admin/catalog/taxonomies/${id}`, data);
  },

  async delete(id: string): Promise<ApiResult<void>> {
    return apiClient.delete(`/admin/catalog/taxonomies/${id}`);
  },

  // Legacy alias
  async getList(params: TaxonomyQuery): Promise<ApiResult<TaxonomyListItem[]>> {
    return this.list(params);
  }
};
