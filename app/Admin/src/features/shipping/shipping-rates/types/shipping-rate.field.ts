import { z } from 'zod'

export function createShippingRateSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
    shippingMethodId: z.string().uuid(t('shipping.validation.shipping_method.invalid')),
    name: z.string().min(1).max(200),
    rate: z.number().min(0, t('shipping.validation.rate.min')),
    fromWeight: z.number().min(0).nullable().optional(),
    toWeight: z.number().min(0).nullable().optional(),
    fromTotal: z.number().min(0).nullable().optional(),
    toTotal: z.number().min(0).nullable().optional(),
  })
}

export type ShippingRateParameters = z.infer<ReturnType<typeof createShippingRateSchema>>
