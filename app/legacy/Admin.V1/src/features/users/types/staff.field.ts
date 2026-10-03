import { z } from 'zod'

export function createStaffSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
    email: z.string().email(t('users.validation.email.invalid')).min(1, t('users.validation.email.required')).max(255, t('users.validation.email.max_length')),
    firstName: z.string().min(1, t('users.validation.first_name.required')).max(100, { message: t('users.validation.first_name.max_length', { max: 100 }) }),
    lastName: z.string().min(1, t('users.validation.last_name.required')).max(100, { message: t('users.validation.last_name.max_length', { max: 100 }) }),
    roleIds: z.array(z.string()).min(1, t('users.validation.roles.min_one')),
    isActive: z.boolean().optional().default(true),
    password: z.string().optional(),
  })
}

export type StaffParameters = z.infer<ReturnType<typeof createStaffSchema>>
