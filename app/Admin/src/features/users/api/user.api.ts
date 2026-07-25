import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { UserResponse, CreateUserRequest, UpdateUserRequest, ToggleUserStatusRequest } from '../types'

export class UserApi {
  static getMany(query: ListQuery): Promise<PagedResult<UserResponse>> {
    return getPagedList<UserResponse>('/identity/users', query)
  }
  static async get(id: string): Promise<Result<UserResponse>> {
    const res = await apiClient.get<Result<UserResponse>>(`/identity/users/${id}`)
    return res.data
  }
  static async create(data: CreateUserRequest): Promise<Result<UserResponse>> {
    const res = await apiClient.post<Result<UserResponse>>('/identity/users', data)
    return res.data
  }
  static async update(id: string, data: UpdateUserRequest): Promise<Result<UserResponse>> {
    const res = await apiClient.put<Result<UserResponse>>(`/identity/users/${id}`, data)
    return res.data
  }
  static async delete(id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/identity/users/${id}`)
    return res.data
  }
  static async toggleStatus(id: string, data: ToggleUserStatusRequest): Promise<Result<void>> {
    const res = await apiClient.patch<Result<void>>(`/identity/users/${id}/status`, data)
    return res.data
  }
}
