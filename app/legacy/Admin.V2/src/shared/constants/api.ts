export const CATALOG = 'catalog'
export const IDENTITY = 'identity'
export const INVENTORY = 'inventory'
export const LOCATION = 'location'
export const ORDERING = 'ordering'
export const PAYMENT = 'payment'
export const PROFILE = 'profile'
export const SHIPPING = 'shipping'
export const USERS = 'users'

export const API_MODULES = {
  CATALOG,
  IDENTITY,
  INVENTORY,
  LOCATION,
  ORDERING,
  PAYMENT,
  PROFILE,
  SHIPPING,
  USERS,
} as const

export type ApiModule = (typeof API_MODULES)[keyof typeof API_MODULES]
