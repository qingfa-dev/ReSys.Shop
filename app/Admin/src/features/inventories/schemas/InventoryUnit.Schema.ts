import { z } from 'zod'

export const InventoryUnitSchema = z.object({
  stockItemId: z.string().uuid('Invalid stock item').min(1, 'Stock item is required'),
  serialNumber: z.string().max(100).optional().nullable(),
  state: z.number().int().min(0, 'State is required'),
  orderId: z.string().uuid().optional().nullable(),
  shipmentId: z.string().uuid().optional().nullable(),
})

export type InventoryUnitParameters = z.infer<typeof InventoryUnitSchema>
