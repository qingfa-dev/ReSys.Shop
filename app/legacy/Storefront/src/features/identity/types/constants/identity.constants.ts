export const IDENTITY_ENDPOINTS = {
  LOGIN: '/api/storefront/identity/auth/login/password',
  REGISTER: '/api/storefront/identity/auth/register',
  LOGOUT: '/api/storefront/identity/auth/logout',
  REFRESH: '/api/storefront/identity/auth/sessions/refresh',
  FORGOT_PASSWORD: '/api/storefront/identity/passwords/forgot',
  RESET_PASSWORD: '/api/storefront/identity/passwords/reset',
  CHANGE_PASSWORD: '/api/storefront/identity/passwords/change',
} as const

export const USER_ROLES = {
  CUSTOMER: 'customer',
  ADMIN: 'admin',
} as const

export type UserRole = typeof USER_ROLES[keyof typeof USER_ROLES]

export const TOKEN_EXPIRY_SECONDS = 3600

export const REFRESH_TOKEN_EXPIRY_SECONDS = 604800