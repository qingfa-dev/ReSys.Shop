import apiClient from '@/shared/api/http/api.client';
import type { ApiResult } from '@/shared/api/types/api.types';

export interface GlobalSearchResult {
    type: 'Product' | 'Order' | 'User';
    id: string;
    title: string;
    subtitle: string;
    route_name: string;
}

export interface GlobalSearchResponse {
    results: GlobalSearchResult[];
}

export const searchService = {
    async search(query: string, limit: number = 10): Promise<ApiResult<GlobalSearchResponse>> {
        return apiClient.get('/admin/search', { params: { q: query, limit } });
    }
};
