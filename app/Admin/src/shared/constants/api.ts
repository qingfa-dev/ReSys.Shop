export const CATALOG = 'api/catalog'
export const IDENTITY = 'api/identity'
export const INVENTORY = 'api/inventory'
export const LOCATION = 'api/locations'
export const ORDERING = 'api/ordering'
export const PAYMENT = 'api/payment'
export const PROFILE = 'api/profiles'
export const SHIPPING = 'api/shipping'
export const DASHBOARD = 'api/dashboard'

export const API_MODULES = {
  CATALOG,
  IDENTITY,
  INVENTORY,
  LOCATION,
  ORDERING,
  PAYMENT,
  PROFILE,
  SHIPPING,
  DASHBOARD,
} as const

export type ApiModule = (typeof API_MODULES)[keyof typeof API_MODULES]
