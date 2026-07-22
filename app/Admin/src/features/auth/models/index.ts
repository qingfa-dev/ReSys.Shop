import { z } from 'zod'
type TFunction = (key: string) => string

export function createLoginSchema(t: TFunction) {
  return z.object({
    credential: z.string().min(1, t('auth.validation.credential.required')),
    password: z.string().min(1, t('auth.validation.password.required')),
  })
}

export function createRegisterSchema(t: TFunction) {
  return z
    .object({
      email: z.string().email(t('auth.validation.email.invalid')),
      userName: z.string().min(3, t('auth.validation.userName.minLength')).max(50, t('auth.validation.userName.maxLength')),
      password: z
        .string()
        .min(8, t('auth.validation.password.minLength'))
        .regex(/[A-Z]/, t('auth.validation.password.uppercase'))
        .regex(/[a-z]/, t('auth.validation.password.lowercase'))
        .regex(/[0-9]/, t('auth.validation.password.digit'))
        .regex(/[^A-Za-z0-9]/, t('auth.validation.password.special')),
      confirmPassword: z.string(),
      firstName: z.string().min(1, t('auth.validation.firstName.required')),
      lastName: z.string().optional(),
      phone: z.string().optional(),
      acceptTerm: z.literal(true, {
        errorMap: () => ({ message: t('auth.validation.acceptTerms.required') }),
      }),
    })
    .refine((data) => data.password === data.confirmPassword, {
      message: t('auth.validation.password.mismatch'),
      path: ['confirmPassword'],
    })
}

export function createForgotPasswordSchema(t: TFunction) {
  return z.object({
    email: z.string().email(t('auth.validation.email.invalid')),
  })
}

export function createResetPasswordSchema(t: TFunction) {
  return z
    .object({
      password: z
        .string()
        .min(8, t('auth.validation.password.minLength'))
        .regex(/[A-Z]/, t('auth.validation.password.uppercase'))
        .regex(/[a-z]/, t('auth.validation.password.lowercase'))
        .regex(/[0-9]/, t('auth.validation.password.digit'))
        .regex(/[^A-Za-z0-9]/, t('auth.validation.password.special')),
      confirmPassword: z.string(),
    })
    .refine((data) => data.password === data.confirmPassword, {
      message: t('auth.validation.password.mismatch'),
      path: ['confirmPassword'],
    })
}

export function createChangePasswordSchema(t: TFunction) {
  return z
    .object({
      currentPassword: z.string().min(1, t('auth.validation.currentPassword.required')),
      newPassword: z
        .string()
        .min(8, t('auth.validation.password.minLength'))
        .regex(/[A-Z]/, t('auth.validation.password.uppercase'))
        .regex(/[a-z]/, t('auth.validation.password.lowercase'))
        .regex(/[0-9]/, t('auth.validation.password.digit'))
        .regex(/[^A-Za-z0-9]/, t('auth.validation.password.special')),
      confirmPassword: z.string(),
    })
    .refine((data) => data.newPassword === data.confirmPassword, {
      message: t('auth.validation.password.mismatch'),
      path: ['confirmPassword'],
    })
}

export type LoginSchema = ReturnType<typeof createLoginSchema>
export type RegisterSchema = ReturnType<typeof createRegisterSchema>
export type ForgotPasswordSchema = ReturnType<typeof createForgotPasswordSchema>
export type ResetPasswordSchema = ReturnType<typeof createResetPasswordSchema>
export type ChangePasswordSchema = ReturnType<typeof createChangePasswordSchema>
