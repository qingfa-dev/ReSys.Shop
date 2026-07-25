import type { CreateUserForm, UpdateUserForm } from '../schemas'
import type { CreateUserRequest, UpdateUserRequest } from '../types'

export class UserFormMapper {
  static toCreate(form: CreateUserForm): CreateUserRequest { return form }
  static toUpdate(form: UpdateUserForm): UpdateUserRequest {
    const { password: _, ...rest } = form as any
    return rest
  }
}
