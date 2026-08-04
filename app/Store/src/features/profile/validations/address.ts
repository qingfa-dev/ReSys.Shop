import { z } from 'zod'

// Address form schema — mirrors the backend's required address fields
// (firstName, address1, city, countryName required by Address.Validator). Country/state
// are selected via the location cascade outside the Form; country is validated manually
// in the AddressForm submit handler.
export const addressSchema = z.object({
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string(),
  address1: z.string().min(1, 'Street address is required'),
  address2: z.string(),
  city: z.string().min(1, 'City is required'),
  zipCode: z.string(),
  phone: z.string(),
  label: z.string(),
  addressType: z.enum(['Shipping', 'Billing', 'Other']),
  isDefault: z.boolean(),
})

export type AddressFormValues = z.infer<typeof addressSchema>
