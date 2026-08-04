import { z } from 'zod'

export const UserPreferencesFields = {
  Required: {
    currency: z.string(),
    language: z.string(),
    timezone: z.string(),
    newsletter: z.boolean(),
  },
  Optional: {},
} as const

export const UserPreferencesSchema = z.object({
  ...UserPreferencesFields.Required,
  notifications: z.object({
    email: z.boolean(),
    sms: z.boolean(),
    push: z.boolean(),
  }).optional(),
})

export type UserPreferencesSchemaType = z.infer<typeof UserPreferencesSchema>

export const AccountSettingsFields = {
  Required: {
    email: z.string().email(),
    firstName: z.string(),
    lastName: z.string(),
  },
  Optional: {
    phone: z.string().optional(),
    avatar: z.string().url().optional(),
  },
} as const

export const AccountSettingsSchema = z.object({
  ...AccountSettingsFields.Required,
  ...AccountSettingsFields.Optional,
})

export type AccountSettingsSchemaType = z.infer<typeof AccountSettingsSchema>