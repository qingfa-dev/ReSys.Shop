import { z } from 'zod'

export function createStockMovementSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  stockItemId: z.string().uuid(t('inventory.validation.stock_item.invalid')).min(1, t('inventory.validation.stock_item.required')),
  quantity: z.number().int(t('inventory.validation.quantity.whole')),
  reason: z.string().max(500).optional(),
  reference: z.string().max(100).optional(),
})
}

export type StockMovementParameters = z.infer<ReturnType<typeof createStockMovementSchema>>
