import { z } from 'zod'

export const StockMovementSchema = z.object({
  stockItemId: z.string().uuid('Invalid stock item').min(1, 'Stock item is required'),
  quantity: z.number().int('Quantity must be a whole number'),
  reason: z.string().max(500).optional(),
  reference: z.string().max(100).optional(),
})

export type StockMovementParameters = z.infer<typeof StockMovementSchema>
