import { z } from 'zod'

export function createChangePasswordSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z
    .object({
      currentPassword: z
        .string()
        .min(1, t('auth.validation.current_password.required'))
        .max(128, t('auth.validation.password.max_length', { max: 128 })),
      newPassword: z
        .string()
        .min(6, t('auth.validation.new_password.min_length'))
        .max(128, t('auth.validation.new_password.max_length', { max: 128 })),
      confirmNewPassword: z
        .string()
        .min(1, t('auth.validation.confirm_password.required')),
    })
    .refine((data) => data.newPassword === data.confirmNewPassword, {
      message: t('auth.validation.passwords.must_match'),
      path: ['confirmNewPassword'],
    })
}

export type ChangePasswordParameters = z.infer<ReturnType<typeof createChangePasswordSchema>>
