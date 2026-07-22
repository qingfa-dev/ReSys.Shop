import { z } from 'zod'
import { AuthFields } from './auth.fields'
import type { TFunction } from './auth.fields'

export class AuthForms {
  private f: AuthFields
  private t: TFunction

  constructor(t: TFunction) {
    this.t = t
    this.f = new AuthFields(t)
  }

  login() {
    return z.object({
      credential: this.f.credential(),
      password: z.string().min(1, this.t('auth.validation.password.required')),
    })
  }

  register() {
    return z
      .object({
        email: this.f.email(),
        userName: this.f.userName(),
        password: this.f.password(),
        confirmPassword: this.f.confirmPassword(),
        firstName: this.f.firstName(),
        lastName: this.f.lastName(),
        phone: this.f.phone(),
        acceptTerm: this.f.acceptTerm(),
      })
      .refine((data) => data.password === data.confirmPassword, {
        message: this.t('auth.validation.password.mismatch'),
        path: ['confirmPassword'],
      })
  }

  forgotPassword() {
    return z.object({
      email: this.f.email(),
    })
  }

  resetPassword() {
    return z
      .object({
        password: this.f.password(),
        confirmPassword: this.f.confirmPassword(),
      })
      .refine((data) => data.password === data.confirmPassword, {
        message: this.t('auth.validation.password.mismatch'),
        path: ['confirmPassword'],
      })
  }

  resetPasswordRequest() {
    return z.object({
      email: this.f.email(),
      userId: z.string(),
      token: z.string(),
      newPassword: this.f.password(),
    })
  }

  changePassword() {
    return z
      .object({
        currentPassword: this.f.currentPassword(),
        newPassword: this.f.password(),
        confirmPassword: this.f.confirmPassword(),
      })
      .refine((data) => data.newPassword === data.confirmPassword, {
        message: this.t('auth.validation.password.mismatch'),
        path: ['confirmPassword'],
      })
  }
}

export type LoginForm = z.input<ReturnType<AuthForms['login']>>
export type RegisterForm = z.input<ReturnType<AuthForms['register']>>
export type ForgotPasswordForm = z.input<ReturnType<AuthForms['forgotPassword']>>
export type ResetPasswordForm = z.input<ReturnType<AuthForms['resetPassword']>>
export type ChangePasswordForm = z.input<ReturnType<AuthForms['changePassword']>>
export type ResetPasswordRequest = z.input<ReturnType<AuthForms['resetPasswordRequest']>>
