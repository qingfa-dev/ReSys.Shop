import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { PermissionResponse } from '../types'

export class PermissionApi {
  static getMany(query: ListQuery): Promise<PagedResult<PermissionResponse>> {
    return getPagedList<PermissionResponse>('/identity/permissions', query)
  }
  static async getPermissions(): Promise<Result<PermissionResponse[]>> {
    const res = await apiClient.get<Result<PermissionResponse[]>>('/identity/permissions')
    return res.data
  }
}
