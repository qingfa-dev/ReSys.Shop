export const IDENTITY_ENDPOINTS = {
  LOGIN: '/identity/auth/login',
  REGISTER: '/identity/auth/register',
  REFRESH: '/identity/auth/refresh',
  LOGOUT: '/identity/auth/logout',
  ME: '/identity/users/me',
  CHANGE_PASSWORD: '/identity/users/change-password',
  FORGOT_PASSWORD: '/identity/auth/forgot-password',
  RESET_PASSWORD: '/identity/auth/reset-password',
  VERIFY_EMAIL: '/identity/auth/verify-email',
} as const

export const USER_ROLES = {
  CUSTOMER: 'customer',
  ADMIN: 'admin',
} as const

export type UserRole = typeof USER_ROLES[keyof typeof USER_ROLES]

export const TOKEN_EXPIRY_SECONDS = 3600

export const REFRESH_TOKEN_EXPIRY_SECONDS = 604800