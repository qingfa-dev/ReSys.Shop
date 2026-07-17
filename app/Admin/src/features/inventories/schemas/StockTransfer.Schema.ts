import { z } from 'zod'

export const StockTransferSchema = z.object({
  sourceLocationId: z.string().uuid('Invalid source location').min(1, 'Source location is required'),
  destinationLocationId: z.string().uuid('Invalid destination location').min(1, 'Destination location is required'),
  reason: z.string().max(500, 'Reason must not exceed 500 characters').optional(),
  items: z.array(z.object({
    variantId: z.string().uuid('Invalid variant'),
    quantity: z.number().int().min(1, 'Quantity must be at least 1'),
  })).min(1, 'At least one item is required'),
})

export type StockTransferParameters = z.infer<typeof StockTransferSchema>
