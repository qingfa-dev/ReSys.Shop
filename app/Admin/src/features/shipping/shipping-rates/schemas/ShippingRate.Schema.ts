import { z } from 'zod'

export const ShippingRateSchema = z.object({
  shippingMethodId: z.string().uuid('Invalid shipping method'),
  name: z.string().min(1).max(200),
  rate: z.number().min(0, 'Rate must be non-negative'),
  fromWeight: z.number().min(0).optional().nullable(),
  toWeight: z.number().min(0).optional().nullable(),
  fromTotal: z.number().min(0).optional().nullable(),
  toTotal: z.number().min(0).optional().nullable(),
})

export type ShippingRateParameters = z.infer<typeof ShippingRateSchema>
