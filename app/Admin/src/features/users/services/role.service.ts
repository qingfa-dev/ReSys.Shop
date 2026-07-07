import { identityApi } from '../../identity/services/identity.api'

export const roleService = {
  list: identityApi.roles.list,
  getById: identityApi.roles.getById,
  create: identityApi.roles.create,
  update: identityApi.roles.update,
  delete: identityApi.roles.delete,
  getUsersInRole: identityApi.roles.getUsersInRole,
  assignPermission: identityApi.roles.assignPermission,
  syncPermissions: identityApi.roles.syncPermissions,
  unassignPermission: identityApi.roles.unassignPermission,
}
