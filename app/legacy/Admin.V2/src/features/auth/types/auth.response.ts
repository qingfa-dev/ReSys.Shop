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
