import { z } from 'zod'
import { TaxonomyFields } from './taxonomy.fields'
import type { TFunction } from './taxonomy.fields'

export class TaxonomyForms {
  private f: TaxonomyFields
  constructor(private t: TFunction) { this.f = new TaxonomyFields(t) }

  create() {
    return z.object({ name: this.f.name(), presentation: this.f.presentation() })
  }

  update() {
    return z.object({ name: this.f.name(), presentation: this.f.presentation() })
  }
}

export type CreateTaxonomyForm = z.input<ReturnType<TaxonomyForms['create']>>
export type UpdateTaxonomyForm = z.input<ReturnType<TaxonomyForms['update']>>
