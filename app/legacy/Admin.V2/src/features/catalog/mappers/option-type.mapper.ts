import type { CreateOptionTypeForm, UpdateOptionTypeForm } from '../schemas'
import type { CreateOptionTypeRequest, UpdateOptionTypeRequest } from '../types'

export class OptionTypeFormMapper {
  static toCreate(form: CreateOptionTypeForm): CreateOptionTypeRequest {
    return {
      name: form.name,
      presentation: form.presentation,
      filterable: form.filterable,
    }
  }

  static toUpdate(form: UpdateOptionTypeForm): UpdateOptionTypeRequest {
    return {
      name: form.name,
      presentation: form.presentation,
      filterable: form.filterable,
    }
  }
}
