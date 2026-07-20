export interface RefreshTokenRequest {
  refreshToken: string
  rememberMe?: boolean
}

export interface AuthProfileResponse {
  id: string
  email: string
  fullName: string
  roles: string[]
  permissions?: string[]
}
