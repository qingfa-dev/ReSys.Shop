import { z } from 'zod'

export const ORDER_FIELDS = {
  id: {
    Required: z.string().uuid('Invalid ID format'),
    Optional: z.string().uuid('Invalid ID format').optional(),
  },
  orderNumber: {
    Required: z.string().min(1, 'Order number is required'),
    Optional: z.string().optional(),
  },
  status: {
    Required: z.enum(['pending', 'processing', 'shipped', 'delivered', 'cancelled', 'refunded']),
    Optional: z.enum(['pending', 'processing', 'shipped', 'delivered', 'cancelled', 'refunded']).optional(),
  },
  quantity: {
    Required: z.number().int().positive('Quantity must be at least 1'),
    Optional: z.number().int().positive().optional(),
  },
  price: {
    Required: z.number().min(0),
    Optional: z.number().min(0).optional(),
  },
  firstName: {
    Required: z.string().min(1, 'First name is required'),
    Optional: z.string().optional(),
  },
  lastName: {
    Required: z.string().min(1, 'Last name is required'),
    Optional: z.string().optional(),
  },
  address1: {
    Required: z.string().min(1, 'Address is required'),
    Optional: z.string().optional(),
  },
  address2: {
    Optional: z.string().optional(),
  },
  city: {
    Required: z.string().min(1, 'City is required'),
    Optional: z.string().optional(),
  },
  state: {
    Required: z.string().min(1, 'State is required'),
    Optional: z.string().optional(),
  },
  postalCode: {
    Required: z.string().min(1, 'Postal code is required'),
    Optional: z.string().optional(),
  },
  country: {
    Required: z.string().min(1, 'Country is required'),
    Optional: z.string().optional(),
  },
  phone: {
    Optional: z.string().optional(),
  },
  isDefault: {
    Optional: z.boolean().optional(),
  },
  productId: {
    Required: z.string().uuid('Invalid product ID'),
    Optional: z.string().uuid('Invalid product ID').optional(),
  },
  productName: {
    Required: z.string().min(1),
    Optional: z.string().optional(),
  },
  productImage: {
    Required: z.string().url('Invalid image URL'),
    Optional: z.string().url().optional(),
  },
  variantId: {
    Optional: z.string().uuid().optional(),
  },
  variantName: {
    Optional: z.string().optional(),
  },
  subtotal: {
    Required: z.number().min(0),
    Optional: z.number().min(0).optional(),
  },
  tax: {
    Required: z.number().min(0),
    Optional: z.number().min(0).optional(),
  },
  shipping: {
    Required: z.number().min(0),
    Optional: z.number().min(0).optional(),
  },
  discount: {
    Required: z.number().min(0),
    Optional: z.number().min(0).optional(),
  },
  total: {
    Required: z.number().min(0),
    Optional: z.number().min(0).optional(),
  },
  currency: {
    Required: z.string().min(1),
    Optional: z.string().optional(),
  },
  trackingNumber: {
    Optional: z.string().optional(),
  },
  createdAt: {
    Required: z.string().datetime(),
    Optional: z.string().datetime().optional(),
  },
  updatedAt: {
    Required: z.string().datetime(),
    Optional: z.string().datetime().optional(),
  },
} as const

export const OrderItemFields = {
  Id: { Required: ORDER_FIELDS.id.Required, Optional: ORDER_FIELDS.id.Optional },
  ProductId: { Required: ORDER_FIELDS.productId.Required, Optional: ORDER_FIELDS.productId.Optional },
  ProductName: { Required: ORDER_FIELDS.productName.Required, Optional: ORDER_FIELDS.productName.Optional },
  ProductImage: { Required: ORDER_FIELDS.productImage.Required, Optional: ORDER_FIELDS.productImage.Optional },
  VariantName: { Optional: ORDER_FIELDS.variantName.Optional },
  Quantity: { Required: ORDER_FIELDS.quantity.Required, Optional: ORDER_FIELDS.quantity.Optional },
  Price: { Required: ORDER_FIELDS.price.Required, Optional: ORDER_FIELDS.price.Optional },
} as const

export const OrderItemSchema = z.object({
  id: ORDER_FIELDS.id.Required,
  productId: ORDER_FIELDS.productId.Required,
  productName: ORDER_FIELDS.productName.Required,
  productImage: ORDER_FIELDS.productImage.Required,
  variantName: ORDER_FIELDS.variantName.Optional,
  quantity: ORDER_FIELDS.quantity.Required,
  price: ORDER_FIELDS.price.Required,
})

export type OrderItem = z.infer<typeof OrderItemSchema>

export const AddressFields = {
  Id: { Required: ORDER_FIELDS.id.Required, Optional: ORDER_FIELDS.id.Optional },
  FirstName: { Required: ORDER_FIELDS.firstName.Required, Optional: ORDER_FIELDS.firstName.Optional },
  LastName: { Required: ORDER_FIELDS.lastName.Required, Optional: ORDER_FIELDS.lastName.Optional },
  Address1: { Required: ORDER_FIELDS.address1.Required, Optional: ORDER_FIELDS.address1.Optional },
  Address2: { Optional: ORDER_FIELDS.address2.Optional },
  City: { Required: ORDER_FIELDS.city.Required, Optional: ORDER_FIELDS.city.Optional },
  State: { Required: ORDER_FIELDS.state.Required, Optional: ORDER_FIELDS.state.Optional },
  PostalCode: { Required: ORDER_FIELDS.postalCode.Required, Optional: ORDER_FIELDS.postalCode.Optional },
  Country: { Required: ORDER_FIELDS.country.Required, Optional: ORDER_FIELDS.country.Optional },
  Phone: { Optional: ORDER_FIELDS.phone.Optional },
  IsDefault: { Optional: ORDER_FIELDS.isDefault.Optional },
} as const

export const AddressSchema = z.object({
  id: ORDER_FIELDS.id.Required,
  firstName: ORDER_FIELDS.firstName.Required,
  lastName: ORDER_FIELDS.lastName.Required,
  address1: ORDER_FIELDS.address1.Required,
  address2: ORDER_FIELDS.address2.Optional,
  city: ORDER_FIELDS.city.Required,
  state: ORDER_FIELDS.state.Required,
  postalCode: ORDER_FIELDS.postalCode.Required,
  country: ORDER_FIELDS.country.Required,
  phone: ORDER_FIELDS.phone.Optional,
  isDefault: ORDER_FIELDS.isDefault.Optional,
})

export type Address = z.infer<typeof AddressSchema>

export const OrderSchema = z.object({
  id: ORDER_FIELDS.id.Required,
  orderNumber: ORDER_FIELDS.orderNumber.Required,
  status: ORDER_FIELDS.status.Required,
  items: z.array(OrderItemSchema),
  shippingAddress: AddressSchema,
  billingAddress: AddressSchema,
  subtotal: ORDER_FIELDS.subtotal.Required,
  tax: ORDER_FIELDS.tax.Required,
  shipping: ORDER_FIELDS.shipping.Required,
  discount: ORDER_FIELDS.discount.Required,
  total: ORDER_FIELDS.total.Required,
  currency: ORDER_FIELDS.currency.Required,
  createdAt: ORDER_FIELDS.createdAt.Required,
  updatedAt: ORDER_FIELDS.updatedAt.Required,
  trackingNumber: ORDER_FIELDS.trackingNumber.Optional,
})

export type Order = z.infer<typeof OrderSchema>

export const CartItemFields = {
  Id: { Required: ORDER_FIELDS.id.Required, Optional: ORDER_FIELDS.id.Optional },
  ProductId: { Required: ORDER_FIELDS.productId.Required, Optional: ORDER_FIELDS.productId.Optional },
  ProductName: { Required: ORDER_FIELDS.productName.Required, Optional: ORDER_FIELDS.productName.Optional },
  ProductImage: { Required: ORDER_FIELDS.productImage.Required, Optional: ORDER_FIELDS.productImage.Optional },
  VariantId: { Optional: ORDER_FIELDS.variantId.Optional },
  VariantName: { Optional: ORDER_FIELDS.variantName.Optional },
  Quantity: { Required: ORDER_FIELDS.quantity.Required, Optional: ORDER_FIELDS.quantity.Optional },
  Price: { Required: ORDER_FIELDS.price.Required, Optional: ORDER_FIELDS.price.Optional },
  CompareAtPrice: { Optional: ORDER_FIELDS.price.Optional },
} as const

export const CartItemSchema = z.object({
  id: z.string().optional(),              // cart item id (backend may not return one)
  productId: z.string().optional(),       // retained for the storefront cart link (backend supplies it via variant.productId)
  variantId: z.string().uuid(),
  variantName: z.string().optional(),
  sku: z.string().optional(),
  productName: z.string().optional(),      // Task 2b enrichment
  productImage: z.string().nullable().optional(), // Task 2b enrichment
  quantity: z.number().int().positive(),
  price: z.number().min(0),
  compareAtPrice: z.number().min(0).nullable().optional(),
})

export type CartItem = z.infer<typeof CartItemSchema>

export const CheckoutSchema = z.object({
  paymentIntentId: z.string().min(1, 'Payment intent id is required'),
})

export type CheckoutRequest = z.infer<typeof CheckoutSchema>

export const OrderStatusSchema = z.enum(['pending', 'processing', 'shipped', 'delivered', 'cancelled', 'refunded'])
export type OrderStatus = z.infer<typeof OrderStatusSchema>

export const ShippingMethodSchema = z.object({
  id: z.string().uuid(),
  name: z.string().min(1),
  price: z.number().min(0),
  estimatedDays: z.number().int().positive().optional(),
  carrier: z.string().optional(),
})

export type ShippingMethod = z.infer<typeof ShippingMethodSchema>

export const PaymentMethodSchema = z.object({
  id: z.string().uuid(),
  name: z.string().min(1),
  type: z.enum(['card', 'paypal', 'bank']),
  lastFour: z.string().optional(),
  isDefault: z.boolean().optional(),
})

export type PaymentMethod = z.infer<typeof PaymentMethodSchema>

export const CartSchema = z.object({
  items: z.array(CartItemSchema),
  subtotal: z.number().min(0),
  discount: z.number().min(0),
  total: z.number().min(0),
  tax: z.number().min(0).optional(),
  shipping: z.number().min(0).optional(),
})

export type Cart = z.infer<typeof CartSchema>

export type OrderSchemaType = Order
export type OrderItemSchemaType = OrderItem
export type AddressSchemaType = Address
export type CartItemSchemaType = CartItem
export type CheckoutRequestSchemaType = CheckoutRequest
export type CartSchemaType = Cart