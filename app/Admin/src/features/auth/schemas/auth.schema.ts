import { z } from 'zod'

export const LoginSchema = z.object({
  credential: z.string().min(1, 'Email or Username is required'),
  password: z.string().min(1, 'Password is required'),
  rememberMe: z.boolean().optional(),
})

export const ChangePasswordSchema = z.object({
  current_password: z.string().min(1, 'Current password is required'),
  new_password: z.string().min(6, 'New password must be at least 6 characters'),
  confirm_new_password: z.string().min(1, 'Please confirm your new password'),
}).refine(data => data.new_password === data.confirm_new_password, {
  message: 'Passwords do not match',
  path: ['confirm_new_password'],
})
