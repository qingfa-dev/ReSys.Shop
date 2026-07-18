import apiClient from '@/shared/api/http/api.client'
import { IDENTITY } from '@/shared/api/constants'
import type { ServerPagedResult, ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { RoleSummary } from '../types/Role.Response.Type'
import type { CreateRoleRequest, UpdateRoleRequest, AssignRolePermissionRequest, RevokeRolePermissionRequest, SyncRolePermissionsRequest } from '../types/Role.Request.Type'

export const roleRepository = {
  list: (params?: ServerQueryingParameters): Promise<ServerPagedResult<RoleSummary>> =>
    apiClient.get(`${IDENTITY}/roles`, { params }).then(res => res.data as ServerPagedResult<RoleSummary>),

  getById: (id: string): Promise<ServerResult<RoleSummary>> =>
    apiClient.get(`${IDENTITY}/roles/${id}`).then(res => res.data as ServerResult<RoleSummary>),

  create: (data: CreateRoleRequest): Promise<ServerResult<RoleSummary>> =>
    apiClient.post(`${IDENTITY}/roles`, data).then(res => res.data as ServerResult<RoleSummary>),

  update: (id: string, data: UpdateRoleRequest): Promise<ServerResult<RoleSummary>> =>
    apiClient.put(`${IDENTITY}/roles/${id}`, data).then(res => res.data as ServerResult<RoleSummary>),

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${IDENTITY}/roles/${id}`).then(res => res.data as ServerResult<void>),

  getPermissions: (id: string): Promise<ServerResult<string[]>> =>
    apiClient.get(`${IDENTITY}/roles/${id}/permissions`).then(res => res.data as ServerResult<string[]>),

  assignPermission: (id: string, data: AssignRolePermissionRequest): Promise<ServerResult<void>> =>
    apiClient.put(`${IDENTITY}/roles/${id}/permissions/assign`, data).then(res => res.data as ServerResult<void>),

  revokePermission: (id: string, data: RevokeRolePermissionRequest): Promise<ServerResult<void>> =>
    apiClient.delete(`${IDENTITY}/roles/${id}/permissions/revoke`, { data }).then(res => res.data as ServerResult<void>),

  syncPermissions: (id: string, data: SyncRolePermissionsRequest): Promise<ServerResult<void>> =>
    apiClient.patch(`${IDENTITY}/roles/${id}/permissions/sync`, data).then(res => res.data as ServerResult<void>),
}
