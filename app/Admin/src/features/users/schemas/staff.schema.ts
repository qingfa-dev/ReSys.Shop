import { z } from 'zod'

export function createStaffSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
    email: z.string().email(t('users.validation.email.invalid')).min(1, t('users.validation.email.required')).max(255, t('users.validation.email.max_length')),
    displayName: z.string().min(1, t('users.validation.display_name.required')).max(200, t('users.validation.display_name.max_length')),
    roleIds: z.array(z.string()).min(1, t('users.validation.roles.min_one')),
    isActive: z.boolean().optional().default(true),
    password: z.string().optional(),
  })
}

export type StaffParameters = z.infer<ReturnType<typeof createStaffSchema>>
