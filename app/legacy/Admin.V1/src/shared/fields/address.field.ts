import { z } from 'zod'

export const addressFields = z.object({
  line1: z.string().min(1),
  line2: z.string().optional(),
  city: z.string().min(1),
  state: z.string().optional(),
  postalCode: z.string().optional(),
  countryCode: z.string().optional(),
  phone: z.string().optional(),
})

export type AddressFields = z.infer<typeof addressFields>
