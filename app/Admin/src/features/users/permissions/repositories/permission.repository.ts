import apiClient from '@/shared/api/http/api.client'
import { IDENTITY } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { PermissionSummary } from '../types/Permission.Response.Type'

export const permissionRepository = {
  list: (params?: ServerQueryingParameters): Promise<ServerResult<PermissionSummary[]>> =>
    apiClient.get(`${IDENTITY}/permissions`, { params }).then(res => res.data as ServerResult<PermissionSummary[]>),
}
