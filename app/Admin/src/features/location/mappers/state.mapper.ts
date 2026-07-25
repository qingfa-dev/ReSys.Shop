import type { CreateStateForm, UpdateStateForm } from '../schemas'
import type { CreateStateRequest, UpdateStateRequest } from '../types'

export class StateFormMapper {
  static toCreate(form: CreateStateForm): CreateStateRequest { return form }
  static toUpdate(form: UpdateStateForm): UpdateStateRequest { return form }
}
