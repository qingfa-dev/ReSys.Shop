import type {
  LoginForm,
  RegisterForm,
  ForgotPasswordForm,
  ChangePasswordForm,
  ResetPasswordRequest,
} from '../schemas'

export type LoginRequest = LoginForm

export type RegisterRequest = Omit<RegisterForm, 'confirmPassword'>

export type ForgotPasswordRequest = ForgotPasswordForm

export type { ResetPasswordRequest }

export type ChangePasswordRequest = { email: string } & Omit<ChangePasswordForm, 'confirmPassword'>
