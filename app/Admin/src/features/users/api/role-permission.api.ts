import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { UserPermissionListResponse, UserPermissionIdsRequest } from '../types'

export class RolePermissionApi {
  static async get(roleId: string): Promise<Result<UserPermissionListResponse>> {
    const res = await apiClient.get<Result<UserPermissionListResponse>>(`/identity/roles/${roleId}/permissions`)
    return res.data
  }
  static async assign(roleId: string, data: UserPermissionIdsRequest): Promise<Result<void>> {
    const res = await apiClient.put<Result<void>>(`/identity/roles/${roleId}/permissions/assign`, data)
    return res.data
  }
  static async revoke(roleId: string, data: UserPermissionIdsRequest): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/identity/roles/${roleId}/permissions/revoke`, { data })
    return res.data
  }
  static async sync(roleId: string, data: UserPermissionIdsRequest): Promise<Result<void>> {
    const res = await apiClient.patch<Result<void>>(`/identity/roles/${roleId}/permissions/sync`, data)
    return res.data
  }
}
