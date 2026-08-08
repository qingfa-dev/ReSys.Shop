import { z } from 'zod'

// Validate: Address type limited to canonical Shipping/Billing/Other values
export const AddressTypeSchema = z.enum(['Shipping', 'Billing', 'Other'])

// Validate: Full address entity shape from server
export const AddressSchema = z.object({
  id: z.string(),
  userId: z.string(),
  addressType: AddressTypeSchema,
  firstName: z.string(),
  lastName: z.string().nullable(),
  address1: z.string(),
  address2: z.string().nullable(),
  city: z.string(),
  zipCode: z.string().nullable(),
  phone: z.string().nullable(),
  label: z.string().nullable(),
  isDefault: z.boolean(),
  countryName: z.string(),
  stateProvince: z.string().nullable(),
  countryCode: z.string().nullable(),
  stateCode: z.string().nullable(),
})

// Enforce: Required fields for create/update; optional fields have max-length constraints
export const AddressInputSchema = z.object({
  addressType: AddressTypeSchema,
  firstName: z.string().min(1).max(200),
  lastName: z.string().max(200).optional(),
  address1: z.string().min(1).max(500),
  address2: z.string().max(500).optional(),
  city: z.string().min(1).max(200),
  zipCode: z.string().max(20).optional(),
  phone: z.string().max(30).optional(),
  label: z.string().max(100).optional(),
  isDefault: z.boolean(),
  countryName: z.string().min(1).max(200),
  stateProvince: z.string().max(200).optional(),
  countryCode: z.string().max(10).optional(),
  stateCode: z.string().max(10).optional(),
})
