import type {
  LoginForm,
  RegisterForm,
  ForgotPasswordForm,
  ResetPasswordForm,
  ChangePasswordForm,
} from '../schemas'
import type {
  LoginRequest,
  RegisterRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  ChangePasswordRequest,
} from '../types'

export class AuthRequestMapper {
  static toLogin(form: LoginForm): LoginRequest {
    return form
  }

  static toRegister(form: RegisterForm): RegisterRequest {
    return {
      email: form.email,
      userName: form.userName,
      password: form.password,
      firstName: form.firstName,
      lastName: form.lastName || undefined,
      phone: form.phone || undefined,
      acceptTerm: form.acceptTerm,
    }
  }

  static toForgotPassword(form: ForgotPasswordForm): ForgotPasswordRequest {
    return form
  }

  static toResetPassword(
    form: ResetPasswordForm,
    query: { email: string; userId: string; token: string },
  ): ResetPasswordRequest {
    return {
      email: query.email,
      userId: query.userId,
      token: query.token,
      newPassword: form.password,
    }
  }

  static toChangePassword(form: ChangePasswordForm, email: string): ChangePasswordRequest {
    return {
      email,
      currentPassword: form.currentPassword,
      newPassword: form.newPassword,
    }
  }
}
