import { z } from 'zod'

export const shippingRateName = z.string()
  .min(1, 'Name is required.')
  .max(255, 'Name must not exceed 255 characters.')

export const shippingRateCost = z.number()
  .positive('Cost must be greater than 0.')

export const shippingRateShippingMethodId = z.string()
  .min(1, 'Shipping method is required.')

export const shippingRateDeliveryRange = z.string()
  .max(100, 'Delivery range must not exceed 100 characters.')
  .optional()

export const shippingRateMinWeight = z.number()
  .min(0, 'Min weight must be at least 0.')
  .optional()

export const shippingRateMaxWeight = z.number()
  .min(0, 'Max weight must be at least 0.')
  .optional()

export const shippingRateFreeShippingThreshold = z.number()
  .min(0, 'Free shipping threshold must be at least 0.')
  .optional()

export const shippingRateSchema = z.object({
  name: shippingRateName,
  cost: shippingRateCost,
  shippingMethodId: shippingRateShippingMethodId,
  deliveryRange: shippingRateDeliveryRange,
  minWeight: shippingRateMinWeight,
  maxWeight: shippingRateMaxWeight,
  freeShippingThreshold: shippingRateFreeShippingThreshold,
})

export type ShippingRateForm = z.infer<typeof shippingRateSchema>
