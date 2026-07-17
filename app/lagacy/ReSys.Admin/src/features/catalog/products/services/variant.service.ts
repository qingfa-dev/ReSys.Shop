import apiClient from '@/shared/api/api.client';
import type { ApiResult, PagedList } from '@/shared/api/api.types';
import type { 
    VariantSummary, 
    VariantDetail, 
    CreateVariantRequest, 
    UpdateVariantRequest 
} from '../types/variant.types';

const BASE_URL = '/admin/catalog/variants';

export const variantService = {
    async listByProductId(productId: string): Promise<ApiResult<PagedList<VariantSummary>>> {
        return apiClient.get(`/admin/catalog/products/${productId}/variants`) as unknown as Promise<ApiResult<PagedList<VariantSummary>>>;
    },

    async getById(id: string): Promise<ApiResult<VariantDetail>> {
        return apiClient.get(`${BASE_URL}/${id}`) as unknown as Promise<ApiResult<VariantDetail>>;
    },

    async create(productId: string, data: CreateVariantRequest): Promise<ApiResult<VariantDetail>> {
        return apiClient.post(`/admin/catalog/products/${productId}/variants`, data) as unknown as Promise<ApiResult<VariantDetail>>;
    },

    async update(id: string, data: UpdateVariantRequest): Promise<ApiResult<VariantDetail>> {
        return apiClient.put(`${BASE_URL}/${id}`, data) as unknown as Promise<ApiResult<VariantDetail>>;
    },

    async delete(id: string): Promise<ApiResult<void>> {
        return apiClient.delete(`${BASE_URL}/${id}`) as unknown as Promise<ApiResult<void>>;
    },

    async setMaster(id: string): Promise<ApiResult<void>> {
        return apiClient.post(`${BASE_URL}/${id}/set-master`) as unknown as Promise<ApiResult<void>>;
    },

    async updateOptionValues(id: string, optionValueIds: string[]): Promise<ApiResult<void>> {
        return apiClient.put(`${BASE_URL}/${id}/option-values`, {
            option_value_ids: optionValueIds
        }) as unknown as Promise<ApiResult<void>>;
    }
};