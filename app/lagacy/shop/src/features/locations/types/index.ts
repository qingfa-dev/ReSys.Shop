export * from './schemas'
export * from './entity'
export * from './request'
export * from './response'

export interface Address {
  id: string
  firstName: string
  lastName: string
  address1: string
  address2?: string
  city: string
  state: string
  postalCode: string
  country: string
  phone?: string
  isDefault: boolean
}

export interface GeoLocation {
  latitude: number
  longitude: number
  country: string
  state: string
  city: string
}

export interface StoreLocation {
  id: string
  name: string
  address: string
  phone: string
  hours: string
  latitude: number
  longitude: number
}
