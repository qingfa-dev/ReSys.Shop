import { z } from 'zod'

export const AvailabilityEntrySchema = z.object({
  stockLocationId: z.string(),
  locationName: z.string(),
  countOnHand: z.number().int(),
  reservedCount: z.number().int(),
  availableCount: z.number().int(),
  backorderable: z.boolean(),
  available: z.boolean(),
})

export const ReserveStockRequestSchema = z.object({
  variantId: z.string(),
  stockLocationId: z.string(),
  quantity: z.number().int().positive(),
  orderId: z.string().nullable().optional(),
  ttlMinutes: z.number().int().positive().optional(),
  reason: z.string().nullable().optional(),
})

export const CartReservationSchema = z.object({
  id: z.string(),
  variantId: z.string(),
  stockLocationId: z.string().nullable(),
  orderId: z.string().nullable(),
  quantity: z.number().int(),
  state: z.string(),
  expiresAtUtc: z.string(),
  reason: z.string().nullable(),
  createdAtUtc: z.string(),
  modifiedAtUtc: z.string().nullable(),
})

export const CartReservationStatusSchema = z.object({
  id: z.string(),
  variantId: z.string(),
  stockLocationId: z.string().nullable(),
  orderId: z.string().nullable(),
  quantity: z.number().int(),
  state: z.string(),
  expiresAtUtc: z.string(),
  reason: z.string().nullable(),
  createdAtUtc: z.string(),
  remainingSeconds: z.number().int(),
})
