import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { UserPermissionListResponse, UserRoleIdsRequest } from '../types'

export class UserPermissionApi {
  static async get(userId: string): Promise<Result<UserPermissionListResponse>> {
    const res = await apiClient.get<Result<UserPermissionListResponse>>(`/identity/users/${userId}/permissions`)
    return res.data
  }
  static async assign(userId: string, data: UserRoleIdsRequest): Promise<Result<void>> {
    const res = await apiClient.post<Result<void>>(`/identity/users/${userId}/permissions/assign`, data)
    return res.data
  }
  static async revoke(userId: string, data: UserRoleIdsRequest): Promise<Result<void>> {
    const res = await apiClient.post<Result<void>>(`/identity/users/${userId}/permissions/revoke`, data)
    return res.data
  }
  static async sync(userId: string, data: UserRoleIdsRequest): Promise<Result<void>> {
    const res = await apiClient.put<Result<void>>(`/identity/users/${userId}/permissions/sync`, data)
    return res.data
  }
}
