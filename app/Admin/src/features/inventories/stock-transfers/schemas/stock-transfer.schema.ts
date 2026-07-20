import { z } from 'zod'

export function createStockTransferSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  sourceLocationId: z.string().uuid(t('inventory.validation.source_location.invalid')).min(1, t('inventory.validation.source_location.required')),
  destinationLocationId: z.string().uuid(t('inventory.validation.destination_location.invalid')).min(1, t('inventory.validation.destination_location.required')),
  reason: z.string().max(500, t('inventory.validation.reason.max_length')).optional(),
  items: z.array(z.object({
    variantId: z.string().uuid(t('inventory.validation.variant.invalid')),
    quantity: z.number().int().min(1, t('inventory.validation.quantity.min_one')),
  })).min(1, t('ordering.validation.items.min_one')),
})
}

export type StockTransferParameters = z.infer<ReturnType<typeof createStockTransferSchema>>
