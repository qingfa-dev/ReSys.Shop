export const IDENTITY_ENDPOINTS = {
  LOGIN: '/api/store/identity/auth/login/password',
  REGISTER: '/api/store/identity/auth/register',
  LOGOUT: '/api/store/identity/auth/logout',
  REFRESH: '/api/store/identity/auth/sessions/refresh',
  FORGOT_PASSWORD: '/api/store/identity/passwords/forgot',
  RESET_PASSWORD: '/api/store/identity/passwords/reset',
  CHANGE_PASSWORD: '/api/store/identity/passwords/change',
} as const

export const USER_ROLES = {
  CUSTOMER: 'customer',
  ADMIN: 'admin',
} as const

export type UserRole = typeof USER_ROLES[keyof typeof USER_ROLES]

export const TOKEN_EXPIRY_SECONDS = 3600

export const REFRESH_TOKEN_EXPIRY_SECONDS = 604800