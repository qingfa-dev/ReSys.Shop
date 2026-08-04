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

/**
 * Client-side view model for a single device/session row in the Sessions view.
 * NOTE: the backend exposes NO list-sessions endpoint — `GET /identity/auth/sessions`
 * returns only the current user's session payload (`SessionUser`). `deviceName` is
 * derived client-side from the user agent; `ipAddress` is not exposed by the API.
 */
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
