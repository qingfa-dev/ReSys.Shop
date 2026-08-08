import { z } from 'zod'

export const CartLineItemSchema = z.object({
  id: z.string(),
  variantId: z.string(),
  variantName: z.string(),
  sku: z.string(),
  productName: z.string().nullable(),
  productImageUrl: z.string().nullable(),
  quantity: z.number().int().min(0),
  price: z.number().min(0),
  total: z.number().min(0),
})

export const CartResponseSchema = z.object({
  id: z.string(),
  itemTotal: z.number().min(0),
  total: z.number().min(0),
  currency: z.string(),
  itemCount: z.number().int().min(0),
  checkoutState: z.string(),
  items: z.array(CartLineItemSchema),
})

export const AddCartItemRequestSchema = z.object({
  variantId: z.string().min(1),
  quantity: z.number().int().min(1),
})

export const UpdateCartItemRequestSchema = z.object({
  quantity: z.number().int().min(1),
})
