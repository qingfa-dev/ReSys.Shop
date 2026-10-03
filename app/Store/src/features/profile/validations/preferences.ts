import { z } from 'zod'

export const ProfilePreferencesSchema = z.object({
  theme: z.string().nullable(),
  language: z.string().nullable(),
  currency: z.string().nullable(),
  sizePreference: z.string().nullable(),
})
