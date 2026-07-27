import type { CreateRoleForm, UpdateRoleForm } from '../schemas'
import type { CreateRoleRequest, UpdateRoleRequest } from '../types'

export class RoleFormMapper {
  static toCreate(form: CreateRoleForm): CreateRoleRequest { return form }
  static toUpdate(form: UpdateRoleForm): UpdateRoleRequest { return form }
}
