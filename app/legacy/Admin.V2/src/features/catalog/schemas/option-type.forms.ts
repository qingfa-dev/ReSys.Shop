import { z } from 'zod'
import { OptionTypeFields } from './option-type.fields'
import type { TFunction } from './option-type.fields'

export class OptionTypeForms {
  private f: OptionTypeFields
  constructor(private t: TFunction) { this.f = new OptionTypeFields(t) }

  create() {
    return z.object({
      name: this.f.name(),
      presentation: this.f.presentation(),
      filterable: this.f.filterable(),
    })
  }

  update() {
    return z.object({
      name: this.f.name(),
      presentation: this.f.presentation(),
      filterable: this.f.filterable(),
    })
  }
}

export type CreateOptionTypeForm = z.input<ReturnType<OptionTypeForms['create']>>
export type UpdateOptionTypeForm = z.input<ReturnType<OptionTypeForms['update']>>
