import apiClient from '@/common/api/http/api.client'
import { IDENTITY } from '@/common/api/constants'
import type { ServerPagedResult, ServerResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { RoleSummary } from '../types/role.response.type'
import type { CreateRoleRequest, UpdateRoleRequest, AssignRolePermissionRequest, RevokeRolePermissionRequest, SyncRolePermissionsRequest } from '../types/role.request.type'
export const roleRepository = {
  async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<RoleSummary>> {
    const res = await apiClient.get(`${IDENTITY}/roles`, { params })
    return res.data as ServerPagedResult<RoleSummary>
  },

  async getById(id: string): Promise<ServerResult<RoleSummary>> {
    const res = await apiClient.get(`${IDENTITY}/roles/${id}`)
    return res.data as ServerResult<RoleSummary>
  },

  async create(data: CreateRoleRequest): Promise<ServerResult<RoleSummary>> {
    const res = await apiClient.post(`${IDENTITY}/roles`, data)
    return res.data as ServerResult<RoleSummary>
  },

  async update(id: string, data: UpdateRoleRequest): Promise<ServerResult<RoleSummary>> {
    const res = await apiClient.put(`${IDENTITY}/roles/${id}`, data)
    return res.data as ServerResult<RoleSummary>
  },

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
