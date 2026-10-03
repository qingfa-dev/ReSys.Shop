import { z } from 'zod'

export const moneyFields = z.object({
  amount: z.number().min(0),
  currency: z.string().default('USD'),
})

export type MoneyFields = z.infer<typeof moneyFields>
