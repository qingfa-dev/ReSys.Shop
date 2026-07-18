export interface RefreshTokenRequest {
  refreshToken: string
  rememberMe?: boolean
}

export interface UpdateProfileRequest {
  email?: string
  fullName?: string
  phone?: string
}

export interface AuthProfileResponse {
  id: string
  email: string
  fullName: string
  roles: string[]
}
