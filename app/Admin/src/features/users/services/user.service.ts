import { userRepository } from '../../identity/repository/user.repository'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { AdminUserSummary, CustomerSummary } from '../types/user.domain.types'
import type { CreateAdminUserRequest, UpdateAdminUserRequest } from '../types/user.request.types'

export const userService = {
  list: userRepository.list,
  listCustomers: userRepository.listCustomers,
  getById: userRepository.getById,
  create: userRepository.create,
  update: userRepository.update,
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
