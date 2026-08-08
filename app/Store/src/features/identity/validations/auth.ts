import { z } from 'zod'

export const LoginRequestSchema = z.object({
  credential: z.string().min(1),
  password: z.string().min(1),
})

export const RegisterRequestSchema = z.object({
  fullName: z.string().min(1).max(200),
  email: z.string().email(),
  password: z.string().min(8),
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

export const ForgotPasswordSchema = z.object({ email: z.string().email() })
export const ResetPasswordSchema = z.object({ token: z.string(), newPassword: z.string().min(8) })
export const ChangePasswordSchema = z.object({ currentPassword: z.string(), newPassword: z.string().min(8) })
export const EmailSchema = z.object({ email: z.string().email() })

export const LoginFormSchema = z.object({ credential: z.string().min(1), password: z.string().min(1) })
export type LoginForm = z.infer<typeof LoginFormSchema>
export const RegisterFormSchema = z.object({
  fullName: z.string().min(1).max(200),
  email: z.string().email(),
  password: z.string().min(8),
  confirmPassword: z.string(),
}).refine(d => d.password === d.confirmPassword, { message: 'Passwords do not match', path: ['confirmPassword'] })
export type RegisterForm = z.infer<typeof RegisterFormSchema>
