import apiClient from '@/common/api/http/api.client'
import { IDENTITY } from '@/common/api/constants'
import type { ServerResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { PermissionSummary } from '../types/permission.response.type'
export const permissionRepository = {
  async list(params?: ServerQueryingParameters): Promise<ServerResult<PermissionSummary[]>> {
    const res = await apiClient.get(`${IDENTITY}/permissions`, { params })
    return res.data as ServerResult<PermissionSummary[]>
  },
}
