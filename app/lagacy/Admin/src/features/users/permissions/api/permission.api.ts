import apiClient from '@/shared/api/http/api.client'
import { IDENTITY } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { PermissionSummary } from '../types/permission.response.type'
export const permissionRepository = {
  async list(params?: ServerQueryingParameters): Promise<ServerResult<PermissionSummary[]>> {
    const res = await apiClient.get(`${IDENTITY}/permissions`, { params })
    return res.data as ServerResult<PermissionSummary[]>
  },
}
