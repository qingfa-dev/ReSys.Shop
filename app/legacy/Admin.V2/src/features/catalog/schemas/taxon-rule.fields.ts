import { z } from 'zod'

export type TFunction = (key: string) => string

export class TaxonRuleFields {
  constructor(private t: TFunction) {}

  type() { return z.string().min(1, this.t('catalog.validation.required')) }
  matchPolicy() { return z.enum(['All', 'Any'], { errorMap: () => ({ message: this.t('catalog.validation.match_policy.invalid') }) }) }
  value() { return z.string().min(1, this.t('catalog.validation.required')) }
}
