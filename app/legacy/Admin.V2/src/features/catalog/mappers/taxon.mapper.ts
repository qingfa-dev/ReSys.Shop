import type { CreateTaxonForm, UpdateTaxonForm } from '../schemas'
import type { TaxonRequest } from '../types'

export class TaxonFormMapper {
  static toCreate(form: CreateTaxonForm): TaxonRequest {
    return {
      name: form.name,
      presentation: form.presentation,
      slug: form.slug,
      description: form.description ?? null,
      position: form.position,
    }
  }

  static toUpdate(form: UpdateTaxonForm): TaxonRequest {
    return {
      name: form.name,
      presentation: form.presentation,
      slug: form.slug,
      description: form.description ?? null,
      position: form.position,
    }
  }
}
