import apiClient from '@/shared/api/http/api.client'
import { IDENTITY } from '@/shared/api/constants'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { AdminUserSummary, CustomerSummary } from '../../users/types/User.Response.Type'
import type { RoleSummary } from '../../users/roles/types/Role.Response.Type'
import type { PermissionSummary } from '../../users/permissions/types/Permission.Response.Type'
import type { CreateAdminUserRequest, UpdateAdminUserRequest } from '../../users/types/User.Request.Type'
import type { CreateRoleRequest, UpdateRoleRequest } from '../../users/roles/types/Role.Request.Type'

export const identityApi = {
  users: {
    async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<AdminUserSummary>> {
      return apiClient.get(`${IDENTITY}/users`, { params })
    },
    async listCustomers(params?: ServerQueryingParameters): Promise<ServerPagedResult<CustomerSummary>> {
      return apiClient.get(`${IDENTITY}/users`, { params: { ...params, role: 'Storefront.Customer' } })
    },
    async getById(id: string): Promise<ServerResult<AdminUserSummary>> {
      return apiClient.get(`${IDENTITY}/users/${id}`)
    },
    async create(data: CreateAdminUserRequest): Promise<ServerResult<AdminUserSummary>> {
      return apiClient.post(`${IDENTITY}/users`, data)
    },
    async update(id: string, data: UpdateAdminUserRequest): Promise<ServerResult<AdminUserSummary>> {
      return apiClient.put(`${IDENTITY}/users/${id}`, data)
    },
    async delete(id: string): Promise<ServerResult<void>> {
      return apiClient.delete(`${IDENTITY}/users/${id}`)
    },
    async updateStatus(id: string, isActive: boolean): Promise<ServerResult<void>> {
      return apiClient.patch(`${IDENTITY}/users/${id}/status`, { isActive })
    },

    // Roles
    async getRoles(id: string): Promise<ServerResult<string[]>> {
      return apiClient.get(`${IDENTITY}/users/${id}/roles`)
    },
    async assignRole(id: string, roleName: string): Promise<ServerResult<void>> {
      return apiClient.post(`${IDENTITY}/users/${id}/roles/assign`, { roleName })
    },
    async revokeRole(id: string, roleName: string): Promise<ServerResult<void>> {
      return apiClient.post(`${IDENTITY}/users/${id}/roles/revoke`, { roleName })
    },
    async syncRoles(id: string, roleNames: string[]): Promise<ServerResult<void>> {
      return apiClient.patch(`${IDENTITY}/users/${id}/roles/sync`, { roleNames })
    },

    // Permissions
    async getPermissions(id: string): Promise<ServerResult<string[]>> {
      return apiClient.get(`${IDENTITY}/users/${id}/permissions`)
    },
    async assignPermission(id: string, permissionName: string): Promise<ServerResult<void>> {
      return apiClient.post(`${IDENTITY}/users/${id}/permissions/assign`, { permissionName })
    },
    async revokePermission(id: string, permissionName: string): Promise<ServerResult<void>> {
      return apiClient.delete(`${IDENTITY}/users/${id}/permissions/revoke`, { data: { permissionName } })
    },
    async syncPermissions(id: string, permissionNames: string[]): Promise<ServerResult<void>> {
      return apiClient.put(`${IDENTITY}/users/${id}/permissions/sync`, { permissionNames })
    },
  },

  roles: {
    async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<RoleSummary>> {
      return apiClient.get(`${IDENTITY}/roles`, { params })
    },
    async getById(id: string): Promise<ServerResult<RoleSummary>> {
      return apiClient.get(`${IDENTITY}/roles/${id}`)
    },
    async create(data: CreateRoleRequest): Promise<ServerResult<RoleSummary>> {
      return apiClient.post(`${IDENTITY}/roles`, data)
    },
    async update(id: string, data: UpdateRoleRequest): Promise<ServerResult<RoleSummary>> {
      return apiClient.put(`${IDENTITY}/roles/${id}`, data)
    },
    async delete(id: string): Promise<ServerResult<void>> {
      return apiClient.delete(`${IDENTITY}/roles/${id}`)
    },

    // Permissions
    async getPermissions(id: string): Promise<ServerResult<string[]>> {
      return apiClient.get(`${IDENTITY}/roles/${id}/permissions`)
    },
    async assignPermission(id: string, permissionName: string): Promise<ServerResult<void>> {
      return apiClient.put(`${IDENTITY}/roles/${id}/permissions/assign`, { permissionName })
    },
    async revokePermission(id: string, permissionName: string): Promise<ServerResult<void>> {
      return apiClient.delete(`${IDENTITY}/roles/${id}/permissions/revoke`, { data: { permissionName } })
    },
    async syncPermissions(id: string, permissionNames: string[]): Promise<ServerResult<void>> {
      return apiClient.patch(`${IDENTITY}/roles/${id}/permissions/sync`, { permissionNames })
    },
  },

  permissions: {
    async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<PermissionSummary>> {
      return apiClient.get(`${IDENTITY}/permissions`, { params })
    },
  },
}
