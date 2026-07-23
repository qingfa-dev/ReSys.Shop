import type { CreateTaxonomyForm, UpdateTaxonomyForm } from '../schemas'
import type { CreateTaxonomyRequest, UpdateTaxonomyRequest } from '../types'

export class TaxonomyFormMapper {
  static toCreate(form: CreateTaxonomyForm): CreateTaxonomyRequest {
    return {
      name: form.name,
      presentation: form.presentation,
    }
  }

  static toUpdate(form: UpdateTaxonomyForm): UpdateTaxonomyRequest {
    return {
      name: form.name,
      presentation: form.presentation,
    }
  }
}
