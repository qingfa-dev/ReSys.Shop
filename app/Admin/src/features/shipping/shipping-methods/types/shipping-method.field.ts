import { z } from 'zod'

export function createShippingMethodSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  name: z.string().min(1, t('catalog.validation.name.required')).max(200),
  description: z.string().max(500).optional().nullable(),
  carrier: z.string().min(1, t('shipping.validation.carrier.required')).max(100),
  isActive: z.boolean().default(true),
  displayOrder: z.number().int().min(0).default(0),
})
}

export type ShippingMethodParameters = z.infer<ReturnType<typeof createShippingMethodSchema>>
