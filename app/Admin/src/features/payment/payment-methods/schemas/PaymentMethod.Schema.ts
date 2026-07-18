import { z } from 'zod'

export const PaymentMethodSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  description: z.string().max(500).optional().nullable(),
  provider: z.string().min(1, 'Provider is required').max(100),
  isActive: z.boolean().default(true),
  displayOrder: z.number().int().min(0).default(0),
})

export type PaymentMethodParameters = z.infer<typeof PaymentMethodSchema>
