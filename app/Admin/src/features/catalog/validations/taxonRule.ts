import { z } from 'zod'

export const taxonRuleType = z.string()
  .min(1, 'Rule type is required.')

export const taxonRuleMatchPolicy = z.string()
  .min(1, 'Match policy is required.')

export const taxonRuleValue = z.string()
  .min(1, 'Value is required.')
  .max(255, 'Value must not exceed 255 characters.')

export const taxonRuleSchema = z.object({
  type: taxonRuleType,
  matchPolicy: taxonRuleMatchPolicy,
  value: taxonRuleValue,
})

export type TaxonRuleForm = z.infer<typeof taxonRuleSchema>
