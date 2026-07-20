import { permissionRepository } from '../api/permission.api'
import type { PermissionSummary } from '../types/permission.response.type'
import type { ServerResult } from '@/shared/api/types/result.types'

export const permissionService = {
  async list(...args: Parameters<typeof permissionRepository.list>): Promise<ServerResult<PermissionSummary[]>> {
    return permissionRepository.list(...args)
  },
}
