import { z } from 'zod'

export const StockAdjustmentSchema = z.object({
  quantity: z.number().int('Quantity must be a whole number'),
  type: z.number().int('Type is required').min(0, 'Invalid adjustment type'),
  reason: z.string().max(500, 'Reason must not exceed 500 characters').optional(),
  reference: z.string().max(100, 'Reference must not exceed 100 characters').optional(),
})

export type StockAdjustmentParameters = z.infer<typeof StockAdjustmentSchema>
