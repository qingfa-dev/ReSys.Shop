import { z } from 'zod'

export function createFulfillmentSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  trackingNumber: z.string().max(100, t('ordering.validation.tracking_number.max_length')).optional(),
  stockLocationId: z.string().uuid(t('ordering.validation.stock_location.invalid')).min(1, t('ordering.validation.stock_location.required')),
  inventoryUnitIds: z.array(z.string().uuid(t('ordering.validation.inventory_unit.invalid'))).min(1, t('ordering.validation.units.min_one')),
})
}

export type FulfillmentParameters = z.infer<ReturnType<typeof createFulfillmentSchema>>
