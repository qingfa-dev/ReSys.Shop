import { userRepository } from '../api/user.api'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { AdminUserSummaryModel } from '../types/user.model.type'

export const userService = {
  async list(...args: Parameters<typeof userRepository.list>): Promise<ServerPagedResult<AdminUserSummaryModel>> {
    return userRepository.list(...args)
  },

  listCustomers: userRepository.listCustomers,

  async getById(...args: Parameters<typeof userRepository.getById>): Promise<ServerResult<AdminUserSummaryModel>> {
    return userRepository.getById(...args)
  },

  async create(...args: Parameters<typeof userRepository.create>): Promise<ServerResult<AdminUserSummaryModel>> {
    return userRepository.create(...args)
  },

  async update(...args: Parameters<typeof userRepository.update>): Promise<ServerResult<AdminUserSummaryModel>> {
    return userRepository.update(...args)
  },

  delete: userRepository.delete,
  getUserPermissions: userRepository.getPermissions,
  updateAdminStatus: userRepository.updateStatus,
  syncUserRoles: userRepository.syncRoles,
  unassignPermission: userRepository.revokePermission,
  assignPermission: async (_id: string, _permissionName: string): Promise<ServerResult<void>> => {
    return { isSuccess: false, statusCode: 501, errors: [{ code: 'not_implemented', message: 'Not implemented — no backend route', type: 0, metadata: null }], message: 'Not implemented — no backend route', metadata: null, value: undefined }
  },
  resetPassword: async (_id: string, _data: { new_password: string }): Promise<ServerResult<void>> => {
    console.warn('resetPassword: no backend route exists. See spec/spec-design-admin-api-services.md')
    return { isSuccess: false, statusCode: 501, errors: [{ code: 'not_implemented', message: 'Not implemented — no backend route', type: 0, metadata: null }], message: 'Not implemented — no backend route', metadata: null, value: undefined }
  },
  unlockAccount: async (_id: string): Promise<ServerResult<void>> => {
    console.warn('unlockAccount: no backend route exists. See spec/spec-design-admin-api-services.md')
    return { isSuccess: false, statusCode: 501, errors: [{ code: 'not_implemented', message: 'Not implemented — no backend route', type: 0, metadata: null }], message: 'Not implemented — no backend route', metadata: null, value: undefined }
  },
  verifyAccount: async (_id: string, _data: { verifyEmail?: boolean; verifyPhone?: boolean }): Promise<ServerResult<void>> => {
    console.warn('verifyAccount: no backend route exists. See spec/spec-design-admin-api-services.md')
    return { isSuccess: false, statusCode: 501, errors: [{ code: 'not_implemented', message: 'Not implemented — no backend route', type: 0, metadata: null }], message: 'Not implemented — no backend route', metadata: null, value: undefined }
  },
}
