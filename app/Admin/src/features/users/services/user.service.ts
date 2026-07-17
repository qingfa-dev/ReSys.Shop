import { identityApi } from '../../identity/services/identity.api'
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'
import type { AdminUserSummary, CustomerSummary, CreateAdminUserRequest, UpdateAdminUserRequest } from '../types/user.types'

export const userService = {
  list: identityApi.users.list,
  listCustomers: identityApi.users.listCustomers,
  getById: identityApi.users.getById,
  create: identityApi.users.create,
  update: identityApi.users.update,
  delete: identityApi.users.delete,
  getUserPermissions: identityApi.users.getPermissions,
  updateAdminStatus: identityApi.users.updateStatus,
  syncUserRoles: identityApi.users.syncRoles,
  unassignPermission: identityApi.users.revokePermission,
  assignPermission: async (_id: string, _permissionName: string) => {
    return { success: false, error: { detail: 'Not implemented — no backend route' } } as const
  },
  resetPassword: async (_id: string, _data: { new_password: string }) => {
    console.warn('resetPassword: no backend route exists. See spec/spec-design-admin-api-services.md')
    return { success: false, error: { detail: 'Not implemented — no backend route' } } as const
  },
  unlockAccount: async (_id: string) => {
    console.warn('unlockAccount: no backend route exists. See spec/spec-design-admin-api-services.md')
    return { success: false, error: { detail: 'Not implemented — no backend route' } } as const
  },
  verifyAccount: async (_id: string, _data: { verifyEmail?: boolean; verifyPhone?: boolean }) => {
    console.warn('verifyAccount: no backend route exists. See spec/spec-design-admin-api-services.md')
    return { success: false, error: { detail: 'Not implemented — no backend route' } } as const
  },
}
