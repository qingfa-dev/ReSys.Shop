import { z } from 'zod'
import { IDENTITY_FIELDS } from '../schemas/user.schema'

export const LoginRequestSchema = z.object({
  email: IDENTITY_FIELDS.email.Required,
  password: IDENTITY_FIELDS.password.Required,
  rememberMe: IDENTITY_FIELDS.rememberMe.Optional,
})

export type LoginRequest = z.infer<typeof LoginRequestSchema>

export const RegisterRequestSchema = z.object({
  email: IDENTITY_FIELDS.email.Required,
  password: IDENTITY_FIELDS.password.Required,
  firstName: IDENTITY_FIELDS.firstName.Required,
  lastName: IDENTITY_FIELDS.lastName.Required,
  phone: IDENTITY_FIELDS.phone.Optional,
})

export type RegisterRequest = z.infer<typeof RegisterRequestSchema>

export const RegisterFormSchema = RegisterRequestSchema.extend({
  confirmPassword: z.string().min(1, 'Please confirm your password'),
  agreeTerms: z.boolean().refine((val) => val === true, 'You must agree to the terms and conditions'),
}).refine((data) => data.password === data.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
})

export type RegisterFormData = z.infer<typeof RegisterFormSchema>

export const PasswordResetRequestSchema = z.object({
  email: IDENTITY_FIELDS.email.Required,
})

export type PasswordResetRequest = z.infer<typeof PasswordResetRequestSchema>

export const PasswordResetConfirmSchema = z.object({
  token: z.string().min(1, 'Token is required'),
  newPassword: IDENTITY_FIELDS.password.Required,
})

export type PasswordResetConfirm = z.infer<typeof PasswordResetConfirmSchema>

export const ChangePasswordRequestSchema = z.object({
  currentPassword: z.string().min(1, 'Current password is required'),
  newPassword: IDENTITY_FIELDS.password.Required,
})

export type ChangePasswordRequest = z.infer<typeof ChangePasswordRequestSchema>

export const UpdateProfileRequestSchema = z.object({
  firstName: IDENTITY_FIELDS.firstName.Optional,
  lastName: IDENTITY_FIELDS.lastName.Optional,
  phone: IDENTITY_FIELDS.phone.Optional,
  avatar: IDENTITY_FIELDS.avatar.Optional,
})

export type UpdateProfileRequest = z.infer<typeof UpdateProfileRequestSchema>
