import { z } from 'zod'

export function createUserSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  email: z.string().email(t('ordering.validation.email.invalid')).min(1, t('ordering.validation.email.required')).max(255, t('users.validation.email.max_length')),
  firstName: z.string().min(1, t('ordering.validation.first_name.required')).max(100, t('profile.validation.first_name.max_length')),
  lastName: z.string().min(1, t('ordering.validation.last_name.required')).max(100, t('profile.validation.last_name.max_length')),
  role: z.array(z.string()).min(1, t('users.validation.roles.min_one')),
  password: z.string().min(6, t('users.validation.password.min_length')).max(128, t('auth.validation.password.max_length')).optional(),
  phoneNumber: z.string().max(30, t('profile.validation.phone.max_length')).optional(),
  emailConfirmed: z.boolean().optional(),
  isActive: z.boolean().optional().default(true),
})
}

export type UserParameters = z.infer<ReturnType<typeof createUserSchema>>
