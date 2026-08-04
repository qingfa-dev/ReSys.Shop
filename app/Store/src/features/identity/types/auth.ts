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
  fullName: string
  email: string
  password: string
}

export interface SessionInfo {
  id: string
  deviceName: string
  ipAddress: string
  lastActivityAt: string
  isCurrent: boolean
}

/**
 * Shape of the GET /sessions response (current user's session payload).
 * The backend returns `id` (not `userId`); the store maps it onto `AuthUser`.
 */
export interface SessionUser {
  id: string
  userName: string
  email: string
  roles: string[]
  permissions: string[]
}
