import { z } from 'zod'
import type { TFunction } from './role.fields'
import { RoleFields } from './role.fields'

export class RoleForms {
  private f: RoleFields
  constructor(private t: TFunction) { this.f = new RoleFields(t) }
  create() { return z.object({ name: this.f.name(), description: this.f.description() }) }
  update() { return this.create() }
}
export type CreateRoleForm = z.input<ReturnType<RoleForms['create']>>
export type UpdateRoleForm = z.input<ReturnType<RoleForms['update']>>
