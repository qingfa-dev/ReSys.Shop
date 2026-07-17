import { z } from 'zod'

export const FulfillmentSchema = z.object({
  trackingNumber: z.string().max(100, 'Tracking number must not exceed 100 characters').optional(),
  stockLocationId: z.string().uuid('Invalid stock location').min(1, 'Stock location is required'),
  inventoryUnitIds: z.array(z.string().uuid('Invalid inventory unit')).min(1, 'At least one unit must be selected'),
})

export type FulfillmentParameters = z.infer<typeof FulfillmentSchema>
