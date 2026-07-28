export interface LoginRequest {
  credential: string
  password: string
}

export interface ForgotPasswordRequest {
  email: string
}

export interface LogoutRequest {
  refreshToken?: string
  revokeAll?: boolean
}

export interface ResetPasswordRequest {
  email: string
  userId: string
  token: string
  newPassword: string
}

export interface TokenPair {
  accessToken: string
  accessTokenExpiresIn: number
  refreshToken: string
  refreshTokenExpiresIn: number
}

export interface SessionInfo {
  id: string
  roles: string[]
  permissions: string[]
}

export interface AuthUser {
  userId: string
  roles: string[]
  permissions: string[]
  isAuthenticated: boolean
}
