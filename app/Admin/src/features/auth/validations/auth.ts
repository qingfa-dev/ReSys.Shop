import { z } from 'zod'

export const emailField = z.string().min(1, 'Email is required').email('Invalid email address')
export const credentialField = z.string().min(1, 'Email or username is required')
export const passwordField = z.string().min(1, 'Password is required')
export const newPasswordField = z.string().min(8, 'Password must be at least 8 characters')
export const userIdField = z.string().min(1, 'User ID is required')
export const tokenField = z.string().min(1, 'Reset token is required')

export const loginSchema = z.object({
  credential: credentialField,
  password: passwordField,
})

export const forgotPasswordSchema = z.object({
  email: emailField,
})

export const resetPasswordSchema = z.object({
  email: emailField,
  userId: userIdField,
  token: tokenField,
  newPassword: newPasswordField,
})

export type LoginFormValues = z.infer<typeof loginSchema>
export type ForgotPasswordFormValues = z.infer<typeof forgotPasswordSchema>
export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>
