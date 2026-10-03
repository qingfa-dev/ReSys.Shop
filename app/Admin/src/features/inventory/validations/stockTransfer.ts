import { z } from 'zod'

export const stockTransferVariantId = z.string()
  .min(1, 'Variant is required.')

export const stockTransferQuantity = z.number()
  .int()
  .positive('Quantity must be greater than 0.')

export const stockTransferItemSchema = z.object({
  variantId: stockTransferVariantId,
  quantity: stockTransferQuantity,
})

export const stockTransferItems = z.array(stockTransferItemSchema)
  .min(1, 'At least one transfer item is required.')

export const stockTransferSourceLocationId = z.string()
  .min(1, 'Source location is required.')

export const stockTransferDestinationLocationId = z.string()
  .min(1, 'Destination location is required.')

export const stockTransferSchema = z.object({
  sourceLocationId: stockTransferSourceLocationId,
  destinationLocationId: stockTransferDestinationLocationId,
  items: stockTransferItems,
}).refine((d) => d.destinationLocationId !== d.sourceLocationId, {
  message: 'Source and destination locations must differ.',
  path: ['destinationLocationId'],
})

export type StockTransferForm = z.infer<typeof stockTransferSchema>
