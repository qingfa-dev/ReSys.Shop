import { z } from 'zod'

export const stockItemStockLocationId = z.string()
  .min(1, 'Stock location is required.')

export const stockItemVariantId = z.string()
  .min(1, 'Variant is required.')

export const stockItemCountOnHand = z.number()
  .int()
  .min(0, 'Count on hand must be greater than or equal to 0.')

export const stockItemBackorderable = z.boolean()

export const stockItemSchema = z.object({
  stockLocationId: stockItemStockLocationId,
  variantId: stockItemVariantId,
  countOnHand: stockItemCountOnHand,
  backorderable: stockItemBackorderable,
})

export type StockItemForm = z.infer<typeof stockItemSchema>
