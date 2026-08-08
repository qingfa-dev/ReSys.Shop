import { z } from 'zod'

// Validate: Shipping method shape matches Module.Shipping storefront DTO.
export const ShippingMethodSchema = z.object({
  id: z.string(),
  name: z.string(),
  adminName: z.string().nullable(),
  code: z.string().nullable(),
  calculatorType: z.string(),
  position: z.number().int(),
})

// Validate: Shipping calculation request — both IDs required for cost computation.
export const CalculateShippingRequestSchema = z.object({
  orderId: z.string(),
  shippingMethodId: z.string(),
})

// Validate: Shipping calculation response including free-shipping flag.
export const ShippingCalculationSchema = z.object({
  shippingMethodId: z.string(),
  methodName: z.string(),
  cost: z.number(),
  currency: z.string(),
  isFreeShipping: z.boolean(),
})

// Validate: Shipping rate with weight thresholds and free-shipping cutoff.
export const ShippingRateSchema = z.object({
  id: z.string(),
  shippingMethodId: z.string(),
  name: z.string(),
  cost: z.number(),
  finalPrice: z.number(),
  deliveryRange: z.string().nullable(),
  minWeight: z.number().nullable(),
  maxWeight: z.number().nullable(),
  freeShippingThreshold: z.number().nullable(),
})
