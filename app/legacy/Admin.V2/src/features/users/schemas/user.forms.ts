import { z } from 'zod'
import type { TFunction } from './user.fields'
import { UserFields } from './user.fields'

export class UserForms {
  private f: UserFields
  constructor(private t: TFunction) { this.f = new UserFields(t) }
  create() {
    return z.object({
      email: this.f.email(),
      userName: this.f.userName(),
      password: this.f.password(),
      firstName: this.f.firstName(),
      lastName: this.f.lastName(),
      phone: this.f.phone(),
      isActive: this.f.isActive(),
    })
  }
  update() {
    return z.object({
      email: this.f.email(),
      userName: this.f.userName(),
      password: this.f.password().optional(),
      firstName: this.f.firstName(),
      lastName: this.f.lastName(),
      phone: this.f.phone(),
      isActive: this.f.isActive(),
    })
  }
}
export type CreateUserForm = z.input<ReturnType<UserForms['create']>>
export type UpdateUserForm = z.input<ReturnType<UserForms['update']>>
