import apiClient from '@/shared/api/http/api.client'
import { IDENTITY } from '@/shared/api/constants'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'
import type { AdminUserSummary, CustomerSummary, CreateAdminUserRequest, UpdateAdminUserRequest } from '../../users/types/user.types'
import type { RoleSummary, CreateRoleRequest, UpdateRoleRequest } from '../../users/types/user.types'
import type { PermissionSummary } from '../../users/types/user.types'

export const identityApi = {
  users: {
    async list(params?: ServerQueryingParameters): Promise<ApiResult<AdminUserSummary[]>> {
      return apiClient.get(`${IDENTITY}/users`, { params })
    },
    async listCustomers(params?: ServerQueryingParameters): Promise<ApiResult<CustomerSummary[]>> {
      return apiClient.get(`${IDENTITY}/users`, { params: { ...params, role: 'Storefront.Customer' } })
    },
    async getById(id: string): Promise<ApiResult<AdminUserSummary>> {
      return apiClient.get(`${IDENTITY}/users/${id}`)
    },
    async create(data: CreateAdminUserRequest): Promise<ApiResult<AdminUserSummary>> {
      return apiClient.post(`${IDENTITY}/users`, data)
    },
    async update(id: string, data: UpdateAdminUserRequest): Promise<ApiResult<AdminUserSummary>> {
      return apiClient.put(`${IDENTITY}/users/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${IDENTITY}/users/${id}`)
    },
    async updateStatus(id: string, isActive: boolean): Promise<ApiResult<void>> {
      return apiClient.patch(`${IDENTITY}/users/${id}/status`, { isActive })
    },

    // Roles
    async getRoles(id: string): Promise<ApiResult<string[]>> {
      return apiClient.get(`${IDENTITY}/users/${id}/roles`)
    },
    async assignRole(id: string, roleName: string): Promise<ApiResult<void>> {
      return apiClient.post(`${IDENTITY}/users/${id}/roles/assign`, { roleName })
    },
    async revokeRole(id: string, roleName: string): Promise<ApiResult<void>> {
      return apiClient.post(`${IDENTITY}/users/${id}/roles/revoke`, { roleName })
    },
    async syncRoles(id: string, roleNames: string[]): Promise<ApiResult<void>> {
      return apiClient.patch(`${IDENTITY}/users/${id}/roles/sync`, { roleNames })
    },

    // Permissions
    async getPermissions(id: string): Promise<ApiResult<string[]>> {
      return apiClient.get(`${IDENTITY}/users/${id}/permissions`)
    },
    async assignPermission(id: string, permissionName: string): Promise<ApiResult<void>> {
      return apiClient.post(`${IDENTITY}/users/${id}/permissions/assign`, { permissionName })
    },
    async revokePermission(id: string, permissionName: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${IDENTITY}/users/${id}/permissions/revoke`, { data: { permissionName } })
    },
    async syncPermissions(id: string, permissionNames: string[]): Promise<ApiResult<void>> {
      return apiClient.put(`${IDENTITY}/users/${id}/permissions/sync`, { permissionNames })
    },
  },

  roles: {
    async list(params?: ServerQueryingParameters): Promise<ApiResult<RoleSummary[]>> {
      return apiClient.get(`${IDENTITY}/roles`, { params })
    },
    async getById(id: string): Promise<ApiResult<RoleSummary>> {
      return apiClient.get(`${IDENTITY}/roles/${id}`)
    },
    async create(data: CreateRoleRequest): Promise<ApiResult<RoleSummary>> {
      return apiClient.post(`${IDENTITY}/roles`, data)
    },
    async update(id: string, data: UpdateRoleRequest): Promise<ApiResult<RoleSummary>> {
      return apiClient.put(`${IDENTITY}/roles/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${IDENTITY}/roles/${id}`)
    },

    // Permissions
    async getPermissions(id: string): Promise<ApiResult<string[]>> {
      return apiClient.get(`${IDENTITY}/roles/${id}/permissions`)
    },
    async assignPermission(id: string, permissionName: string): Promise<ApiResult<void>> {
      return apiClient.put(`${IDENTITY}/roles/${id}/permissions/assign`, { permissionName })
    },
    async revokePermission(id: string, permissionName: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${IDENTITY}/roles/${id}/permissions/revoke`, { data: { permissionName } })
    },
    async syncPermissions(id: string, permissionNames: string[]): Promise<ApiResult<void>> {
      return apiClient.patch(`${IDENTITY}/roles/${id}/permissions/sync`, { permissionNames })
    },
  },

  permissions: {
    async list(params?: ServerQueryingParameters): Promise<ApiResult<PermissionSummary[]>> {
      return apiClient.get(`${IDENTITY}/permissions`, { params })
    },
  },
}
