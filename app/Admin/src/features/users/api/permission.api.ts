import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { PermissionResponse } from '../types'

export class PermissionApi {
  static async getMany(): Promise<Result<PermissionResponse[]>> {
    const res = await apiClient.get<Result<PermissionResponse[]>>('/identity/permissions')
    return res.data
  }
}
