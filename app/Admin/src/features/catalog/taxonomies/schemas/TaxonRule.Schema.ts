import { z } from 'zod'

export function createTaxonRuleSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  type: z.string().min(1, t('catalog.validation.rule_type.required')),
  value: z.string().min(1, t('catalog.validation.value.required')),
  matchPolicy: z.string().min(1, t('catalog.validation.match_policy.required')),
})
}

export type TaxonRuleParameters = z.infer<ReturnType<typeof createTaxonRuleSchema>>
