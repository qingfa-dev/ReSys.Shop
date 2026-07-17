import { z } from 'zod'

export const AddressSchema = z.object({
  firstName: z.string().min(1, 'First name is required').max(100, 'First name must not exceed 100 characters'),
  lastName: z.string().min(1, 'Last name is required').max(100, 'Last name must not exceed 100 characters'),
  address1: z.string().min(1, 'Address is required').max(200, 'Address must not exceed 200 characters'),
  address2: z.string().max(200, 'Address must not exceed 200 characters').optional(),
  city: z.string().min(1, 'City is required').max(100, 'City must not exceed 100 characters'),
  zipCode: z.string().min(1, 'ZIP code is required').max(20, 'ZIP code must not exceed 20 characters'),
  countryCode: z.string().length(2, 'Country code must be 2 characters'),
  stateCode: z.string().max(10, 'State code must not exceed 10 characters').optional(),
  phone: z.string().max(30, 'Phone must not exceed 30 characters').optional(),
  company: z.string().max(100, 'Company must not exceed 100 characters').optional(),
})

export const LineItemSchema = z.object({
  variantId: z.string().uuid('Invalid variant'),
  quantity: z.number().int('Quantity must be a whole number').min(1, 'Quantity must be at least 1'),
})

export const OrderSchema = z.object({
  email: z.string().email('Invalid email format').min(1, 'Email is required'),
  currency: z.string().length(3, 'Currency must be a 3-letter code').default('USD'),
  lineItems: z.array(LineItemSchema).min(1, 'At least one item is required'),
  shippingAddress: AddressSchema.optional(),
  billingAddress: AddressSchema.optional(),
})

export type OrderParameters = z.infer<typeof OrderSchema>
