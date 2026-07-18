import { z } from 'zod'

export function createProfileSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  firstName: z.string().min(1, t('ordering.validation.first_name.required')).max(100, t('profile.validation.first_name.max_length')),
  lastName: z.string().min(1, t('ordering.validation.last_name.required')).max(100, t('profile.validation.last_name.max_length')),
  phoneNumber: z.string().max(30, t('profile.validation.phone.max_length')).optional(),
  dateOfBirth: z.string().optional(),
  gender: z.string().max(50).optional(),
  bio: z.string().max(1000, t('profile.validation.bio.max_length')).optional(),
  avatarUrl: z.string().url(t('profile.validation.url.invalid')).optional().nullable(),
  acceptsEmailMarketing: z.boolean().optional(),
})
}

export type ProfileParameters = z.infer<ReturnType<typeof createProfileSchema>>
