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

export const StoreLocationFields = {
  Required: {
    id: z.string(),
    name: z.string(),
    address: z.string(),
    phone: z.string(),
    hours: z.string(),
    latitude: z.number(),
    longitude: z.number(),
  },
  Optional: {
    services: z.array(z.string()).optional(),
    parkingInfo: z.string().optional(),
  },
} as const

export const StoreLocationSchema = z.object({
  ...StoreLocationFields.Required,
  ...StoreLocationFields.Optional,
})

export type StoreLocationSchemaType = z.infer<typeof StoreLocationSchema>

export const GeoLocationSchema = z.object({
  latitude: z.number(),
  longitude: z.number(),
  country: z.string().optional(),
  state: z.string().optional(),
  city: z.string().optional(),
})

export type GeoLocationSchemaType = z.infer<typeof GeoLocationSchema>