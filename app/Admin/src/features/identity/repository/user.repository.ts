import apiClient from '@/shared/api/http/api.client'
import { IDENTITY } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { AdminUserSummary, CustomerSummary } from '../../users/types/User.Response.Type'
import type { CreateAdminUserRequest, UpdateAdminUserRequest } from '../../users/types/User.Request.Type'

export const userRepository = {
  list: (params?: ServerQueryingParameters): Promise<ServerResult<AdminUserSummary[]>> =>
    apiClient.get(`${IDENTITY}/users`, { params }).then(res => res.data as ServerResult<AdminUserSummary[]>),

  listCustomers: (params?: ServerQueryingParameters): Promise<ServerResult<CustomerSummary[]>> =>
    apiClient.get(`${IDENTITY}/users`, { params: { ...params, role: 'Storefront.Customer' } }).then(res => res.data as ServerResult<CustomerSummary[]>),

  getById: (id: string): Promise<ServerResult<AdminUserSummary>> =>
    apiClient.get(`${IDENTITY}/users/${id}`).then(res => res.data as ServerResult<AdminUserSummary>),

  create: (data: CreateAdminUserRequest): Promise<ServerResult<AdminUserSummary>> =>
    apiClient.post(`${IDENTITY}/users`, data).then(res => res.data as ServerResult<AdminUserSummary>),

  update: (id: string, data: UpdateAdminUserRequest): Promise<ServerResult<AdminUserSummary>> =>
    apiClient.put(`${IDENTITY}/users/${id}`, data).then(res => res.data as ServerResult<AdminUserSummary>),

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${IDENTITY}/users/${id}`).then(res => res.data as ServerResult<void>),

  updateStatus: (id: string, isActive: boolean): Promise<ServerResult<void>> =>
    apiClient.patch(`${IDENTITY}/users/${id}/status`, { isActive }).then(res => res.data as ServerResult<void>),

  getRoles: (id: string): Promise<ServerResult<string[]>> =>
    apiClient.get(`${IDENTITY}/users/${id}/roles`).then(res => res.data as ServerResult<string[]>),

  assignRole: (id: string, roleName: string): Promise<ServerResult<void>> =>
    apiClient.post(`${IDENTITY}/users/${id}/roles/assign`, { roleName }).then(res => res.data as ServerResult<void>),

  revokeRole: (id: string, roleName: string): Promise<ServerResult<void>> =>
    apiClient.post(`${IDENTITY}/users/${id}/roles/revoke`, { roleName }).then(res => res.data as ServerResult<void>),

  syncRoles: (id: string, roleNames: string[]): Promise<ServerResult<void>> =>
    apiClient.patch(`${IDENTITY}/users/${id}/roles/sync`, { roleNames }).then(res => res.data as ServerResult<void>),

  getPermissions: (id: string): Promise<ServerResult<string[]>> =>
    apiClient.get(`${IDENTITY}/users/${id}/permissions`).then(res => res.data as ServerResult<string[]>),

  assignPermission: (id: string, permissionName: string): Promise<ServerResult<void>> =>
    apiClient.post(`${IDENTITY}/users/${id}/permissions/assign`, { permissionName }).then(res => res.data as ServerResult<void>),

  revokePermission: (id: string, permissionName: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${IDENTITY}/users/${id}/permissions/revoke`, { data: { permissionName } }).then(res => res.data as ServerResult<void>),

  syncPermissions: (id: string, permissionNames: string[]): Promise<ServerResult<void>> =>
    apiClient.put(`${IDENTITY}/users/${id}/permissions/sync`, { permissionNames }).then(res => res.data as ServerResult<void>),
}
