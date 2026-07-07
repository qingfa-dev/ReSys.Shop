import { identityApi } from '../../identity/services/identity.api'

export const permissionService = {
  list: identityApi.permissions.list,
  getById: identityApi.permissions.getById,
  create: identityApi.permissions.create,
  update: identityApi.permissions.update,
  delete: identityApi.permissions.delete,
  getPermissionSelect: identityApi.permissions.getSelect,
}
