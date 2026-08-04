import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { RoleResponse, CreateRoleRequest, UpdateRoleRequest } from '../types'

export class RoleApi {
  static getMany(query: ListQuery): Promise<PagedResult<RoleResponse>> {
    return getPagedList<RoleResponse>('/identity/roles', query)
  }
  static async get(id: string): Promise<Result<RoleResponse>> {
    const res = await apiClient.get<Result<RoleResponse>>(`/identity/roles/${id}`)
    return res.data
  }
  static async create(data: CreateRoleRequest): Promise<Result<RoleResponse>> {
    const res = await apiClient.post<Result<RoleResponse>>('/identity/roles', data)
    return res.data
  }
  static async update(id: string, data: UpdateRoleRequest): Promise<Result<RoleResponse>> {
    const res = await apiClient.put<Result<RoleResponse>>(`/identity/roles/${id}`, data)
    return res.data
  }
  static async delete(id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/identity/roles/${id}`)
    return res.data
  }
}
