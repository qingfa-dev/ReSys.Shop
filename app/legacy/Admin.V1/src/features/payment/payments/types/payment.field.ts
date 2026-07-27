import { z } from 'zod'

export function createPaymentSchema(_t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  notes: z.string().max(500).optional().nullable(),
  transactionId: z.string().max(200).optional().nullable(),
})
}

export type PaymentParameters = z.infer<ReturnType<typeof createPaymentSchema>>
