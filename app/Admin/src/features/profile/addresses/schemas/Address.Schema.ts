import { z } from 'zod'

export const AddressSchema = z.object({
  address1: z.string().min(1, 'Address is required').max(200),
  address2: z.string().max(200).optional().nullable(),
  city: z.string().min(1, 'City is required').max(100),
  stateProvince: z.string().min(1, 'State/Province is required').max(100),
  postalCode: z.string().min(1, 'Postal code is required').max(20),
  country: z.string().min(1, 'Country is required').max(100),
  isDefault: z.boolean().default(false),
})

export type AddressParameters = z.infer<typeof AddressSchema>
