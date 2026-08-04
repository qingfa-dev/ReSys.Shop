import { z } from 'zod'

export const AddressFields = {
  Required: {
    id: z.string(),
    firstName: z.string(),
    lastName: z.string(),
    address1: z.string(),
    city: z.string(),
    state: z.string(),
    postalCode: z.string(),
    country: z.string(),
    isDefault: z.boolean(),
  },
  Optional: {
    address2: z.string().optional(),
    phone: z.string().optional(),
    instructions: z.string().optional(),
  },
} as const

export const AddressSchema = z.object({
  ...AddressFields.Required,
  ...AddressFields.Optional,
})

export type AddressSchemaType = z.infer<typeof AddressSchema>
