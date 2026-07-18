import { z } from 'zod'

export function createShippingRateSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
    shippingMethodId: z.string().uuid(t('shipping.validation.shipping_method.invalid')),
    name: z.string().min(1).max(200),
    rate: z.number().min(0, t('shipping.validation.rate.min')),
  })
}

export type ShippingRateParameters = z.infer<ReturnType<typeof createShippingRateSchema>>
