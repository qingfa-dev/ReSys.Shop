import { z } from 'zod'

export const ChangePasswordSchema = z
  .object({
    currentPassword: z
      .string()
      .min(1, 'Current password is required')
      .max(128, 'Password must not exceed 128 characters'),
    newPassword: z
      .string()
      .min(6, 'New password must be at least 6 characters')
      .max(128, 'New password must not exceed 128 characters'),
    confirmNewPassword: z
      .string()
      .min(1, 'Please confirm your new password'),
  })
  .refine((data) => data.newPassword === data.confirmNewPassword, {
    message: 'Passwords do not match',
    path: ['confirmNewPassword'],
  })

export type ChangePasswordParameters = z.infer<typeof ChangePasswordSchema>
