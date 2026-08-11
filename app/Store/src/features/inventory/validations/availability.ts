import { z } from 'zod'

// Validate: Stock reservation request — quantity must be a positive integer.
export const ReserveStockRequestSchema = z.object({
  variantId: z.string(),
  stockLocationId: z.string(),
  quantity: z.number().int().positive(),
  orderId: z.string().nullable().optional(),
  ttlMinutes: z.number().int().positive().optional(),
  reason: z.string().nullable().optional(),
})

// Validate: Cart reservation response including expiration timestamp.
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

// Validate: Cart reservation status with remaining TTL for countdown display.
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
