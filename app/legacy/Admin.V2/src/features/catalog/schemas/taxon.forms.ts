import { z } from 'zod'
import { TaxonFields } from './taxon.fields'
import type { TFunction } from './taxon.fields'

export class TaxonForms {
  private f: TaxonFields
  constructor(private t: TFunction) { this.f = new TaxonFields(t) }

  create() {
    return z.object({
      name: this.f.name(),
      presentation: this.f.presentation(),
      slug: this.f.slug(),
      description: this.f.description(),
      position: this.f.position(),
    })
  }

  update() {
    return z.object({
      name: this.f.name(),
      presentation: this.f.presentation(),
      slug: this.f.slug(),
      description: this.f.description(),
      position: this.f.position(),
    })
  }
}

export type CreateTaxonForm = z.input<ReturnType<TaxonForms['create']>>
export type UpdateTaxonForm = z.input<ReturnType<TaxonForms['update']>>
