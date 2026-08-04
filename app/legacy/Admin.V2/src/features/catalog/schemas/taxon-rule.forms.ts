import { z } from 'zod'
import { TaxonRuleFields } from './taxon-rule.fields'
import type { TFunction } from './taxon-rule.fields'

export class TaxonRuleForms {
  private f: TaxonRuleFields
  constructor(private t: TFunction) { this.f = new TaxonRuleFields(t) }

  create() {
    return z.object({
      type: this.f.type(),
      matchPolicy: this.f.matchPolicy(),
      value: this.f.value(),
    })
  }

  update() {
    return this.create()
  }
}

export type TaxonRuleForm = z.input<ReturnType<TaxonRuleForms['create']>>
