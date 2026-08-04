import { z } from 'zod'

export function createPaymentMethodSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  name: z.string().min(1, t('catalog.validation.name.required')).max(200),
  code: z.string().min(1, t('payment.validation.code.required')).max(100),
  description: z.string().max(500).optional().nullable(),
  isActive: z.boolean().default(true),
  displayOrder: z.number().int().min(0).default(0),
})
}

export type PaymentMethodParameters = z.infer<ReturnType<typeof createPaymentMethodSchema>>
