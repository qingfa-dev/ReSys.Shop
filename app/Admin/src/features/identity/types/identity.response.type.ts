export interface UserSessionInfo {
  id: string
  email: string
  fullName: string
  roles: string[]
  permissions: string[]
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: UserSessionInfo
}
