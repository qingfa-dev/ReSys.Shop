import { z } from 'zod'

export const addressType = z.enum(['Shipping', 'Billing', 'Other'], {
  message: 'Address type must be Shipping, Billing, or Other.',
})

export const addressFirstName = z.string()
  .min(1, 'First name is required.')
  .max(100, 'First name cannot exceed 100 characters.')

export const addressLastName = z.string()
  .max(100, 'Last name cannot exceed 100 characters.')
  .optional()

export const addressAddress1 = z.string()
  .min(1, 'Address line 1 is required.')
  .max(200, 'Address line 1 cannot exceed 200 characters.')

export const addressAddress2 = z.string()
  .max(200, 'Address line 2 cannot exceed 200 characters.')
  .optional()

export const addressCity = z.string()
  .min(1, 'City is required.')
  .max(100, 'City cannot exceed 100 characters.')

export const addressZipCode = z.string()
  .max(20, 'Zip code cannot exceed 20 characters.')
  .optional()

export const addressPhone = z.string()
  .max(20, 'Phone cannot exceed 20 characters.')
  .optional()

export const addressLabel = z.string()
  .max(50, 'Label cannot exceed 50 characters.')
  .optional()

export const addressCountryName = z.string()
  .min(1, 'Country is required.')
  .max(100, 'Country cannot exceed 100 characters.')

export const addressSchema = z.object({
  userId: z.string()
    .min(1, 'User is required.'),
  addressType,
  firstName: addressFirstName,
  lastName: addressLastName,
  address1: addressAddress1,
  address2: addressAddress2,
  city: addressCity,
  zipCode: addressZipCode,
  phone: addressPhone,
  label: addressLabel,
  isDefault: z.boolean(),
  countryName: addressCountryName,
})

export type AddressForm = z.infer<typeof addressSchema>
