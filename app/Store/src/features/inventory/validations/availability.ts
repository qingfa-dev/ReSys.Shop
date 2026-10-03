import { z } from 'zod'

// Validate: Stock reservation request — quantity must be a positive integer.
// Backend derives orderId/cartToken server-side from the X-Cart-Token header.
export const ReserveStockRequestSchema = z.object({
  variantId: z.string(),
  stockLocationId: z.string(),
  quantity: z.number().int().positive(),
  ttlMinutes: z.number().int().positive().optional(),
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
  modifiedAtUtc: z.string().nullable().optional(),
  remainingSeconds: z.number().int(),
})
