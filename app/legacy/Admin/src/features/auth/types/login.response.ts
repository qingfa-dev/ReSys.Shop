export interface LoginResponse {
  accessToken: string
  accessTokenExpiresIn: number
  refreshToken: string
  refreshTokenExpiresIn: number
}

export interface AuthSessionResponse {
  id: string
  roles: string[]
  permissions: string[]
}

export interface UserProfile {
  id: string
  email: string
  fullName: string
  roles: string[]
}
