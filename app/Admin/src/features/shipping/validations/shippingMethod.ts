import { z } from 'zod'

export const shippingMethodName = z.string()
  .min(1, 'Name is required.')
  .max(255, 'Name must not exceed 255 characters.')

export const shippingMethodCode = z.string()
  .max(50, 'Code must not exceed 50 characters.')
  .optional()

export const shippingMethodTrackingUrl = z.string()
  .optional()

export const shippingMethodAdminName = z.string()
  .optional()

export const shippingMethodCalculatorType = z.string()
  .min(1, 'Calculator type is required.')
  .max(100, 'Calculator type must not exceed 100 characters.')

export const shippingMethodTaxCategoryId = z.string()
  .optional()

export const shippingMethodPosition = z.number()
  .int()
  .min(0, 'Position must be at least 0.')

export const shippingMethodAvailableToUsers = z.boolean()

export const shippingMethodSchema = z.object({
  name: shippingMethodName,
  code: shippingMethodCode,
  trackingUrl: shippingMethodTrackingUrl,
  adminName: shippingMethodAdminName,
  calculatorType: shippingMethodCalculatorType,
  taxCategoryId: shippingMethodTaxCategoryId,
  position: shippingMethodPosition,
  availableToUsers: shippingMethodAvailableToUsers,
})

export type ShippingMethodForm = z.infer<typeof shippingMethodSchema>
