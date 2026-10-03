import { z } from 'zod'

export function createInventoryUnitSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  stockItemId: z.string().uuid(t('inventory.validation.stock_item.invalid')).min(1, t('inventory.validation.stock_item.required')),
  serialNumber: z.string().max(100).optional().nullable(),
  state: z.number().int().min(0, t('inventory.validation.state.required')),
  orderId: z.string().uuid().optional().nullable(),
  shipmentId: z.string().uuid().optional().nullable(),
})
}

export type InventoryUnitParameters = z.infer<ReturnType<typeof createInventoryUnitSchema>>
