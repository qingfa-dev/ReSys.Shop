import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { UserRoleListResponse, UserRoleIdsRequest } from '../types'

export class UserRoleApi {
  static async get(userId: string): Promise<Result<UserRoleListResponse>> {
    const res = await apiClient.get<Result<UserRoleListResponse>>(`/identity/users/${userId}/roles`)
    return res.data
  }
  static async assign(userId: string, data: UserRoleIdsRequest): Promise<Result<void>> {
    const res = await apiClient.post<Result<void>>(`/identity/users/${userId}/roles/assign`, data)
    return res.data
  }
  static async revoke(userId: string, data: UserRoleIdsRequest): Promise<Result<void>> {
    const res = await apiClient.post<Result<void>>(`/identity/users/${userId}/roles/revoke`, data)
    return res.data
  }
  static async sync(userId: string, data: UserRoleIdsRequest): Promise<Result<void>> {
    const res = await apiClient.patch<Result<void>>(`/identity/users/${userId}/roles/sync`, data)
    return res.data
  }
}
