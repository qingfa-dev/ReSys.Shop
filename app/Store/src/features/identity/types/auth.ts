export interface TokenPair {
  accessToken: string
  accessTokenExpiresIn: number
  refreshToken: string
  refreshTokenExpiresIn: number
}

export interface AuthUser {
  userId: string
  userName: string
  email: string
  roles: string[]
  permissions: string[]
  isAuthenticated: boolean
}

export interface LoginRequest {
  credential: string
  password: string
}

export interface RegisterRequest {
  email: string
  userName: string
  password: string
  firstName: string
  lastName: string
  phone?: string
  acceptTerm: boolean
}

export interface SessionUser {
  id: string
  userName: string
  email: string
  roles: string[]
  permissions: string[]
}

export interface SessionInfo {
  id: string
  deviceName: string
  ipAddress: string
  lastActivityAt: string
  isCurrent: boolean
}
