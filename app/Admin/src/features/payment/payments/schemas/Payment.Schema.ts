import { z } from 'zod'

export const PaymentSchema = z.object({
  notes: z.string().max(500).optional().nullable(),
  transactionId: z.string().max(200).optional().nullable(),
})

export type PaymentParameters = z.infer<typeof PaymentSchema>
