import { z } from 'zod'

export function createPaymentMethodSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  name: z.string().min(1, t('catalog.validation.name.required')).max(200),
  description: z.string().max(500).optional().nullable(),
  provider: z.string().min(1, t('payment.validation.provider.required')).max(100),
  isActive: z.boolean().default(true),
  displayOrder: z.number().int().min(0).default(0),
})
}

export type PaymentMethodParameters = z.infer<ReturnType<typeof createPaymentMethodSchema>>
