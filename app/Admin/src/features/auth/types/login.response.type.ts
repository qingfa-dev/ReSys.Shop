export interface AuthenticationResponse {
  accessToken: string
  accessTokenExpiresIn: number
  refreshToken: string
  refreshTokenExpiresIn: number
}

export interface UserProfile {
  id: string
  email: string
  fullName: string
  roles: string[]
}
