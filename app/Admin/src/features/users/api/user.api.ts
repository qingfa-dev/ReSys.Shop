import apiClient from '@/shared/api/http/api.client'
import { IDENTITY } from '@/shared/api/constants'
import type { ServerPagedResult, ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { AdminUserSummary, CustomerSummary } from '../types/User.Response.Type'
import type { CreateAdminUserRequest, UpdateAdminUserRequest, UpdateUserStatusRequest, AssignRoleRequest, SyncRolesRequest, AssignPermissionRequest, SyncPermissionsRequest } from '../types/User.Request.Type'

export const userRepository = {
  list: (params?: ServerQueryingParameters): Promise<ServerPagedResult<AdminUserSummary>> =>
    apiClient.get(`${IDENTITY}/users`, { params }).then(res => res.data as ServerPagedResult<AdminUserSummary>),

  listCustomers: (params?: ServerQueryingParameters): Promise<ServerPagedResult<CustomerSummary>> =>
    apiClient.get(`${IDENTITY}/users`, { params: { ...params, role: 'Storefront.Customer' } }).then(res => res.data as ServerPagedResult<CustomerSummary>),

  getById: (id: string): Promise<ServerResult<AdminUserSummary>> =>
    apiClient.get(`${IDENTITY}/users/${id}`).then(res => res.data as ServerResult<AdminUserSummary>),

  create: (data: CreateAdminUserRequest): Promise<ServerResult<AdminUserSummary>> =>
    apiClient.post(`${IDENTITY}/users`, data).then(res => res.data as ServerResult<AdminUserSummary>),

  update: (id: string, data: UpdateAdminUserRequest): Promise<ServerResult<AdminUserSummary>> =>
    apiClient.put(`${IDENTITY}/users/${id}`, data).then(res => res.data as ServerResult<AdminUserSummary>),

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${IDENTITY}/users/${id}`).then(res => res.data as ServerResult<void>),

  updateStatus: (id: string, data: UpdateUserStatusRequest): Promise<ServerResult<void>> =>
    apiClient.patch(`${IDENTITY}/users/${id}/status`, data).then(res => res.data as ServerResult<void>),

  getRoles: (id: string): Promise<ServerResult<string[]>> =>
    apiClient.get(`${IDENTITY}/users/${id}/roles`).then(res => res.data as ServerResult<string[]>),

  assignRole: (id: string, data: AssignRoleRequest): Promise<ServerResult<void>> =>
    apiClient.post(`${IDENTITY}/users/${id}/roles/assign`, data).then(res => res.data as ServerResult<void>),

  revokeRole: (id: string, data: AssignRoleRequest): Promise<ServerResult<void>> =>
    apiClient.post(`${IDENTITY}/users/${id}/roles/revoke`, data).then(res => res.data as ServerResult<void>),

  syncRoles: (id: string, data: SyncRolesRequest): Promise<ServerResult<void>> =>
    apiClient.patch(`${IDENTITY}/users/${id}/roles/sync`, data).then(res => res.data as ServerResult<void>),

  getPermissions: (id: string): Promise<ServerResult<string[]>> =>
    apiClient.get(`${IDENTITY}/users/${id}/permissions`).then(res => res.data as ServerResult<string[]>),

  assignPermission: (id: string, data: AssignPermissionRequest): Promise<ServerResult<void>> =>
    apiClient.post(`${IDENTITY}/users/${id}/permissions/assign`, data).then(res => res.data as ServerResult<void>),

  revokePermission: (id: string, data: AssignPermissionRequest): Promise<ServerResult<void>> =>
    apiClient.delete(`${IDENTITY}/users/${id}/permissions/revoke`, { data }).then(res => res.data as ServerResult<void>),

  syncPermissions: (id: string, data: SyncPermissionsRequest): Promise<ServerResult<void>> =>
    apiClient.put(`${IDENTITY}/users/${id}/permissions/sync`, data).then(res => res.data as ServerResult<void>),
}
