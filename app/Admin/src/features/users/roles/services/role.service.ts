import { roleRepository } from '../api/role.api'
import type { RoleSummary } from '../types/role.response.type'
import type { ServerPagedResult, ServerResult } from '@/common/api/types/result.types'

export const roleService = {
  async list(...args: Parameters<typeof roleRepository.list>): Promise<ServerPagedResult<RoleSummary>> {
    return roleRepository.list(...args)
  },

  async getById(...args: Parameters<typeof roleRepository.getById>): Promise<ServerResult<RoleSummary>> {
    return roleRepository.getById(...args)
  },

  async create(...args: Parameters<typeof roleRepository.create>): Promise<ServerResult<RoleSummary>> {
    return roleRepository.create(...args)
  },

  async update(...args: Parameters<typeof roleRepository.update>): Promise<ServerResult<RoleSummary>> {
    return roleRepository.update(...args)
  },

  delete: roleRepository.delete,
  getPermissions: roleRepository.getPermissions,
  assignPermission: roleRepository.assignPermission,
  revokePermission: roleRepository.revokePermission,
  syncPermissions: roleRepository.syncPermissions,
}
