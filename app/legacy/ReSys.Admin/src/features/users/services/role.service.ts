import apiClient from '@/shared/api/api.client';
import type { ApiResult, PagedList } from '@/shared/api/api.types';
import type {
    RoleSummary,
    CreateRoleRequest,
    UpdateRoleRequest
} from '../types/user.types'; // Assuming types are centrally located or will be moved

// Types imported from user.types.ts

export const roleService = {
    async listRoles(params: any): Promise<ApiResult<PagedList<RoleSummary>>> {
        return apiClient.get('/admin/identity/roles', { params });
    },

    async getRole(id: string): Promise<ApiResult<RoleSummary>> {
        return apiClient.get(`/admin/identity/roles/${id}`);
    },

    async createRole(data: CreateRoleRequest): Promise<ApiResult<RoleSummary>> {
        return apiClient.post('/admin/identity/roles', data);
    },

    async updateRole(id: string, data: UpdateRoleRequest): Promise<ApiResult<RoleSummary>> {
        return apiClient.put(`/admin/identity/roles/${id}`, data);
    },

    async deleteRole(id: string): Promise<ApiResult<void>> {
        return apiClient.delete(`/admin/identity/roles/${id}`);
    },

    async getUsersInRole(roleName: string, params: any): Promise<ApiResult<PagedList<any>>> {
        return apiClient.get(`/admin/identity/roles/${roleName}/users`, { params });
    },

    async assignPermission(id: string, permissionName: string): Promise<ApiResult<void>> {
        return apiClient.post(`/admin/identity/roles/${id}/permissions`, { permissionName });
    },

    async syncPermissions(id: string, permissionNames: string[]): Promise<ApiResult<void>> {
        return apiClient.put(`/admin/identity/roles/${id}/permissions`, { permissionNames });
    },

    async unassignPermission(id: string, permissionName: string): Promise<ApiResult<void>> {
        return apiClient.delete(`/admin/identity/roles/${id}/permissions/${permissionName}`);
    }
};
