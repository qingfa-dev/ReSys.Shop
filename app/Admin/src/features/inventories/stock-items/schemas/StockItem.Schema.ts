import { z } from 'zod'

export function createStockAdjustmentSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  quantity: z.number().int(t('inventory.validation.quantity.whole')),
  type: z.number().int(t('inventory.validation.type.required')).min(0, t('inventory.validation.type.invalid')),
  reason: z.string().max(500, t('inventory.validation.reason.max_length')).optional(),
  reference: z.string().max(100, t('inventory.validation.reference.max_length')).optional(),
})
}

export type StockAdjustmentParameters = z.infer<ReturnType<typeof createStockAdjustmentSchema>>
