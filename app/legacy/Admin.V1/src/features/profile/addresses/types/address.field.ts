import { z } from 'zod'

export function createAddressSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  address1: z.string().min(1, t('inventory.validation.address.required')).max(200),
  address2: z.string().max(200).optional().nullable(),
  city: z.string().min(1, t('inventory.validation.city.required')).max(100),
  stateProvince: z.string().min(1, t('profile.validation.state_province.required')).max(100),
  postalCode: z.string().min(1, t('profile.validation.postal_code.required')).max(20),
  country: z.string().min(1, t('location.validation.country.required')).max(100),
  isDefault: z.boolean().default(false),
})
}

export type AddressParameters = z.infer<ReturnType<typeof createAddressSchema>>
