import { createModuleApi, apiClient } from '@/shared/api'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'
import type { AdminUserSummary, CustomerSummary, CreateAdminUserRequest, UpdateAdminUserRequest } from '../../users/types/user.types'
import type { RoleSummary, CreateRoleRequest, UpdateRoleRequest } from '../../users/types/user.types'
import type { PermissionSummary } from '../../users/types/user.types'
import { IDENTITY } from '@/shared/api/constants'

export const identityApi = {
  users: {
    ...createModuleApi<AdminUserSummary>({ basePath: IDENTITY + '/users' }),

    async listAdmins(params?: ServerQueryingParameters): Promise<ApiResult<AdminUserSummary[]>> {
      return apiClient.get(`${IDENTITY}/users`, { params })
    },
    async getAdminDetail(id: string): Promise<ApiResult<AdminUserSummary>> {
      return apiClient.get(`${IDENTITY}/users/${id}`)
    },
    async createAdmin(data: CreateAdminUserRequest): Promise<ApiResult<AdminUserSummary>> {
      return apiClient.post(`${IDENTITY}/users`, data)
    },
    async updateAdmin(id: string, data: UpdateAdminUserRequest): Promise<ApiResult<AdminUserSummary>> {
      return apiClient.put(`${IDENTITY}/users/${id}`, data)
    },
    async deleteAdmin(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${IDENTITY}/users/${id}`)
    },

    updateStatus(id: string, isActive: boolean): Promise<ApiResult<void>> {
      return apiClient.patch(`${IDENTITY}/users/${id}/status`, { isActive })
    },
    updateStaffProfile(id: string, data: Record<string, unknown>): Promise<ApiResult<void>> {
      return apiClient.put(`${IDENTITY}/users/${id}/staff-profile`, data)
    },
    resetPassword(id: string, data: Record<string, unknown>): Promise<ApiResult<void>> {
      return apiClient.post(`${IDENTITY}/users/${id}/reset-password`, data)
    },
    unlockAccount(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`${IDENTITY}/users/${id}/unlock`)
    },
    verifyAccount(id: string, data: Record<string, unknown>): Promise<ApiResult<void>> {
      return apiClient.post(`${IDENTITY}/users/${id}/verify`, data)
    },
    getRoles(id: string): Promise<ApiResult<string[]>> {
      return apiClient.get(`${IDENTITY}/users/${id}/roles`)
    },
    assignRole(id: string, roleName: string): Promise<ApiResult<void>> {
      return apiClient.post(`${IDENTITY}/users/${id}/roles`, { roleName })
    },
    unassignRole(id: string, roleName: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${IDENTITY}/users/${id}/roles/${roleName}`)
    },
    syncRoles(id: string, roleNames: string[]): Promise<ApiResult<void>> {
      return apiClient.put(`${IDENTITY}/users/${id}/roles`, { roleNames })
    },
    getPermissions(id: string): Promise<ApiResult<string[]>> {
      return apiClient.get(`${IDENTITY}/users/${id}/permissions`)
    },
    assignPermission(id: string, permissionName: string): Promise<ApiResult<void>> {
      return apiClient.post(`${IDENTITY}/users/${id}/permissions`, { permissionName })
    },
    unassignPermission(id: string, permissionName: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${IDENTITY}/users/${id}/permissions/${permissionName}`)
    },
    async listCustomers(params: ServerQueryingParameters): Promise<ApiResult<CustomerSummary[]>> {
      const searchParams = { ...params, role: 'Storefront.Customer' }
      return apiClient.get(`${IDENTITY}/users`, { params: searchParams })
    },
  },

  roles: {
    ...createModuleApi<RoleSummary>({ basePath: IDENTITY + '/roles' }),

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

    getUsersInRole(roleName: string, params?: ServerQueryingParameters): Promise<ApiResult<any[]>> {
      return apiClient.get(`${IDENTITY}/roles/${roleName}/users`, { params })
    },
    assignPermission(id: string, permissionName: string): Promise<ApiResult<void>> {
      return apiClient.post(`${IDENTITY}/roles/${id}/permissions`, { permissionName })
    },
    syncPermissions(id: string, permissionNames: string[]): Promise<ApiResult<void>> {
      return apiClient.put(`${IDENTITY}/roles/${id}/permissions`, { permissionNames })
    },
    unassignPermission(id: string, permissionName: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${IDENTITY}/roles/${id}/permissions/${permissionName}`)
    },
  },

  permissions: {
    ...createModuleApi<PermissionSummary>({ basePath: IDENTITY + '/permissions' }),

    getSelect(params?: ServerQueryingParameters): Promise<ApiResult<any[]>> {
      return apiClient.get(`${IDENTITY}/permissions/select`, { params })
    },
  },
}
