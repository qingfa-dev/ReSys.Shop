import apiClient from '@/common/api/http/api.client';
import type { ServerResult } from '@/common/api/types/result.types';

export interface GlobalSearchResult {
    type: 'Product' | 'Order' | 'User';
    id: string;
    title: string;
    subtitle: string;
    routeName: string;
}

export interface GlobalSearchResponse {
    results: GlobalSearchResult[];
}

export const searchService = {
    async search(query: string, limit: number = 10): Promise<ServerResult<GlobalSearchResponse>> {
        return apiClient.get('/admin/search', { params: { q: query, limit } }).then(res => res.data as ServerResult<GlobalSearchResponse>);
    }
};
