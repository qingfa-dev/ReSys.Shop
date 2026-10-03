import { z } from 'zod'

export const orderCurrency = z.string()
  .min(1, 'Currency is required.')
  .max(3, 'Currency must be a 3-letter ISO code.')

export const orderEmail = z.string()
  .email('A valid email address is required.')
  .optional()

export const orderSpecialInstructions = z.string()
  .optional()

export const orderBillAddressId = z.string()
  .optional()

export const orderShipAddressId = z.string()
  .optional()

export const orderShippingMethodId = z.string()
  .optional()

export const orderStatus = z.enum(['Draft', 'Placed', 'Canceled', 'Expired'], {
  message: 'Status must be a valid order status.',
})

export const orderLineItemQuantity = z.number()
  .int()
  .min(1, 'Quantity must be at least 1.')
  .max(999, 'Quantity cannot exceed 999.')

export const orderLineItemPrice = z.number()
  .min(0, 'Price must be greater than or equal to 0.')

export const addLineItemSchema = z.object({
  variantId: z.string()
    .min(1, 'Variant is required.'),
  quantity: orderLineItemQuantity,
  price: orderLineItemPrice,
})

export const orderSchema = z.object({
  currency: orderCurrency,
  email: orderEmail,
  specialInstructions: orderSpecialInstructions,
  billAddressId: orderBillAddressId,
  shipAddressId: orderShipAddressId,
  shippingMethodId: orderShippingMethodId,
})

export type OrderForm = z.infer<typeof orderSchema>

export const updateOrderStatusSchema = z.object({
  status: orderStatus,
})

export type UpdateOrderStatusForm = z.infer<typeof updateOrderStatusSchema>
