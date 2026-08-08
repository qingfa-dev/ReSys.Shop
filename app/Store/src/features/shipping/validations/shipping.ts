import { z } from 'zod'

export const ShippingMethodSchema = z.object({
  id: z.string(),
  name: z.string(),
  adminName: z.string().nullable(),
  code: z.string().nullable(),
  calculatorType: z.string(),
  position: z.number().int(),
})

export const CalculateShippingRequestSchema = z.object({
  orderId: z.string(),
  shippingMethodId: z.string(),
})

export const ShippingCalculationSchema = z.object({
  shippingMethodId: z.string(),
  methodName: z.string(),
  cost: z.number(),
  currency: z.string(),
  isFreeShipping: z.boolean(),
})

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
