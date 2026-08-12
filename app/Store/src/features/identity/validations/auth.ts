import { z } from 'zod'
import { zodMessages } from '@/shared/validations/messages'

export const LoginRequestSchema = z.object({
  credential: z.string().min(1),
  password: z.string().min(1),
})

export const RegisterRequestSchema = z.object({
  email: z.string().email(),
  userName: z.string().min(3).max(32),
  password: z.string().min(12),
  firstName: z.string().min(1).max(50),
  lastName: z.string().min(1).max(50),
  phone: z.string().max(15).optional(),
  acceptTerm: z.literal(true),
})

export const TokenPairSchema = z.object({
  accessToken: z.string(),
  accessTokenExpiresIn: z.number(),
  refreshToken: z.string(),
  refreshTokenExpiresIn: z.number(),
})

export const SessionUserSchema = z.object({
  id: z.string(),
  userName: z.string(),
  email: z.string(),
  roles: z.array(z.string()),
  permissions: z.array(z.string()),
})

export const SessionInfoSchema = z.object({
  id: z.string(),
  deviceName: z.string(),
  ipAddress: z.string(),
  lastActivityAt: z.string(),
  isCurrent: z.boolean(),
})

export const ForgotPasswordSchema = z.object({ email: z.string().email(zodMessages.email) })
export const ResetPasswordSchema = z.object({ token: z.string(), newPassword: z.string().min(8, zodMessages.minLength('New password', 8)) })
export const ChangePasswordSchema = z.object({ currentPassword: z.string().min(1, zodMessages.required('Current password')), newPassword: z.string().min(8, zodMessages.minLength('New password', 8)) })
export const EmailSchema = z.object({ email: z.string().email() })

export const LoginFormSchema = z.object({ credential: z.string().min(1, zodMessages.required('Email or username')), password: z.string().min(1, zodMessages.required('Password')) })
export type LoginForm = z.infer<typeof LoginFormSchema>
export const RegisterFormSchema = z.object({
  firstName: z.string().min(1, zodMessages.required('First name')).max(50, zodMessages.maxLength('First name', 50)),
  lastName: z.string().min(1, zodMessages.required('Last name')).max(50, zodMessages.maxLength('Last name', 50)),
  email: z.string().email(zodMessages.email),
  password: z.string().min(12, zodMessages.passwordRules),
  confirmPassword: z.string(),
}).refine(d => d.password === d.confirmPassword, { message: zodMessages.passwordsMatch, path: ['confirmPassword'] })
export type RegisterForm = z.infer<typeof RegisterFormSchema>

export const ResetPasswordFormSchema = ResetPasswordSchema.extend({ confirmPassword: z.string() }).refine(d => d.confirmPassword === d.newPassword, { message: zodMessages.passwordsMatch, path: ['confirmPassword'] })
export type ResetPasswordForm = z.infer<typeof ResetPasswordFormSchema>

export const ChangePasswordFormSchema = ChangePasswordSchema.extend({ confirmPassword: z.string() }).refine(d => d.confirmPassword === d.newPassword, { message: zodMessages.passwordsMatch, path: ['confirmPassword'] })
export type ChangePasswordForm = z.infer<typeof ChangePasswordFormSchema>
