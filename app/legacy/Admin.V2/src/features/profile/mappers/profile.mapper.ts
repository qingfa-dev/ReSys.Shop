import type { UpdateProfileForm } from '../schemas'
import type { UpdateProfileRequest } from '../types'

export class ProfileFormMapper {
  static toUpdate(form: UpdateProfileForm): UpdateProfileRequest { return form }
}
