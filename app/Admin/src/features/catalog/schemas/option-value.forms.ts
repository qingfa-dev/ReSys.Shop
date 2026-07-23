import { z } from 'zod'
import { OptionValueFields } from './option-value.fields'
import type { TFunction } from './option-value.fields'

export class OptionValueForms {
  private f: OptionValueFields
  constructor(private t: TFunction) { this.f = new OptionValueFields(t) }

  create() {
    return z.object({
      name: this.f.name(),
      presentation: this.f.presentation(),
      position: this.f.position(),
    })
  }

  update() {
    return z.object({
      name: this.f.name(),
      presentation: this.f.presentation(),
      position: this.f.position(),
    })
  }
}

export type CreateOptionValueForm = z.input<ReturnType<OptionValueForms['create']>>
export type UpdateOptionValueForm = z.input<ReturnType<OptionValueForms['update']>>
