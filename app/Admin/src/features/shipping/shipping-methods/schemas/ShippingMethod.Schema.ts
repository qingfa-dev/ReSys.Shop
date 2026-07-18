import { z } from 'zod'

export const ShippingMethodSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  description: z.string().max(500).optional().nullable(),
  carrier: z.string().min(1, 'Carrier is required').max(100),
  isActive: z.boolean().default(true),
  displayOrder: z.number().int().min(0).default(0),
})

export type ShippingMethodParameters = z.infer<typeof ShippingMethodSchema>
