import { z } from 'zod'

export const TaxonRuleSchema = z.object({
  type: z.string().min(1, 'Rule type is required'),
  value: z.string().min(1, 'Value is required'),
  matchPolicy: z.string().min(1, 'Match policy is required'),
})

export type TaxonRuleParameters = z.infer<typeof TaxonRuleSchema>
