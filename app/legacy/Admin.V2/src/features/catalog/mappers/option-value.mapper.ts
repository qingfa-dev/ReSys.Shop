import type { CreateOptionValueForm, UpdateOptionValueForm } from '../schemas'
import type { OptionValueRequest } from '../types'

export class OptionValueFormMapper {
  static toCreate(form: CreateOptionValueForm): OptionValueRequest {
    return {
      name: form.name,
      presentation: form.presentation,
      position: form.position,
    }
  }

  static toUpdate(form: UpdateOptionValueForm): OptionValueRequest {
    return {
      name: form.name,
      presentation: form.presentation,
      position: form.position,
    }
  }
}
