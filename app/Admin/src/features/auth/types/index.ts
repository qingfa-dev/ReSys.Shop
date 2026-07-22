export interface LoginRequest {
  credential: string
  password: string
}

export interface RegisterRequest {
  email: string
  userName: string
  password: string
  firstName: string
  lastName?: string
  phone?: string
  acceptTerm: boolean
}

export interface ForgotPasswordRequest {
  email: string
}

export interface ResetPasswordRequest {
  email: string
  userId: string
  token: string
  newPassword: string
}

export interface ChangePasswordRequest {
  email: string
  currentPassword: string
  newPassword: string
}

export interface TokenResponse {
  accessToken: string
  accessTokenExpiresIn: number
  refreshToken: string
  refreshTokenExpiresIn: number
}

export interface RegisterResponse {
  userId: string
  email: string
  message: string
}

export interface SessionResponse {
  id: string
  roles: string[]
  permissions: string[]
}

export interface LoginForm {
  credential: string
  password: string
}

export interface RegisterForm {
  email: string
  userName: string
  password: string
  confirmPassword: string
  firstName: string
  lastName: string
  phone: string
  acceptTerm: boolean
}

export interface ForgotPasswordForm {
  email: string
}

export interface ResetPasswordForm {
  password: string
  confirmPassword: string
}

export interface ChangePasswordForm {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}
