import apiClient from '@/shared/api/api.client';
import type { ApiResult, PagedList } from '@/shared/api/api.types';
import type {
  AdminUserSummary,
  CustomerSummary,
  UserSearchParams,
  CreateAdminUserRequest,
  UpdateAdminUserRequest,
  UpdateStaffProfileRequest,
  ResetPasswordRequest,
  VerifyUserRequest
} from '../types/user.types';

export const userService = {
  // Admin Users
  async listAdmins(params: UserSearchParams): Promise<ApiResult<AdminUserSummary[]>> {
    return apiClient.get('/admin/identity/users', { params });
  },

  async getAdminDetail(id: string): Promise<ApiResult<AdminUserSummary>> {
    return apiClient.get(`/admin/identity/users/${id}`);
  },

  async createAdmin(data: CreateAdminUserRequest): Promise<ApiResult<AdminUserSummary>> {
    return apiClient.post('/admin/identity/users', data);
  },

  async updateAdmin(id: string, data: UpdateAdminUserRequest): Promise<ApiResult<AdminUserSummary>> {
    return apiClient.put(`/admin/identity/users/${id}`, data);
  },

  async deleteAdmin(id: string): Promise<ApiResult<void>> {
    return apiClient.delete(`/admin/identity/users/${id}`);
  },

  async updateAdminStatus(id: string, isActive: boolean): Promise<ApiResult<void>> {
    return apiClient.patch(`/admin/identity/users/${id}/status`, { is_active: isActive });
  },

  async updateStaffProfile(id: string, data: UpdateStaffProfileRequest): Promise<ApiResult<void>> {
    return apiClient.put(`/admin/identity/users/${id}/staff-profile`, data);
  },

  // Security Management
  async resetPassword(id: string, data: ResetPasswordRequest): Promise<ApiResult<void>> {
    return apiClient.post(`/admin/identity/users/${id}/reset-password`, data);
  },

  async unlockAccount(id: string): Promise<ApiResult<void>> {
    return apiClient.post(`/admin/identity/users/${id}/unlock`);
  },

  async verifyAccount(id: string, data: VerifyUserRequest): Promise<ApiResult<void>> {
    return apiClient.post(`/admin/identity/users/${id}/verify`, data);
  },

  // Role Management for User
  async getUserRoles(id: string): Promise<ApiResult<string[]>> {
    return apiClient.get(`/admin/identity/users/${id}/roles`);
  },

  async assignRole(id: string, roleName: string): Promise<ApiResult<void>> {
    return apiClient.post(`/admin/identity/users/${id}/roles`, { roleName });
  },

  async unassignRole(id: string, roleName: string): Promise<ApiResult<void>> {
    return apiClient.delete(`/admin/identity/users/${id}/roles/${roleName}`);
  },

  async syncUserRoles(id: string, roleNames: string[]): Promise<ApiResult<void>> {
    return apiClient.put(`/admin/identity/users/${id}/roles`, { roleNames });
  },

  // Direct Permissions
  async getUserPermissions(id: string): Promise<ApiResult<string[]>> {
    return apiClient.get(`/admin/identity/users/${id}/permissions`);
  },

  async assignPermission(id: string, permissionName: string): Promise<ApiResult<void>> {
    return apiClient.post(`/admin/identity/users/${id}/permissions`, { permissionName });
  },

  async unassignPermission(id: string, permissionName: string): Promise<ApiResult<void>> {
    return apiClient.delete(`/admin/identity/users/${id}/permissions/${permissionName}`);
  },

  // Customers (Reusing user endpoint with filter)
  async listCustomers(params: UserSearchParams): Promise<ApiResult<CustomerSummary[]>> {
    // Force role filter for customers
    const searchParams = { ...params, role: 'Storefront.Customer' };
    return apiClient.get('/admin/identity/users', { params: searchParams });
  }
};