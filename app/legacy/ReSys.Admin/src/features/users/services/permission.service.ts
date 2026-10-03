import apiClient from '@/shared/api/api.client';
import type { ApiResult, PagedList } from '@/shared/api/api.types';
import type { PermissionSummary } from '../types/user.types';

export const permissionService = {
    async listPermissions(params: any): Promise<ApiResult<PagedList<PermissionSummary>>> {
        return apiClient.get('/admin/identity/permissions', { params });
    },

    async getPermissionSelect(params: any): Promise<ApiResult<any[]>> {
        return apiClient.get('/admin/identity/permissions/select', { params });
    }
};
