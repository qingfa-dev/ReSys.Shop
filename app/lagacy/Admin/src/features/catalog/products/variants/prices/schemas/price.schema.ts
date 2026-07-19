import { z } from 'zod'

export function createPriceSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  amount: z.number().min(0, t('catalog.validation.price.min')),
  currency: z.string().length(3, t('catalog.validation.currency.length')).default('USD'),
})
}

export type PriceParameters = z.infer<ReturnType<typeof createPriceSchema>>
