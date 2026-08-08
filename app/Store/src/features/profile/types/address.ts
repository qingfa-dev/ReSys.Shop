export type AddressType = 'Shipping' | 'Billing' | 'Other'

export interface Address {
  id: string
  userId: string
  addressType: AddressType
  firstName: string
  lastName: string | null
  address1: string
  address2: string | null
  city: string
  zipCode: string | null
  phone: string | null
  label: string | null
  isDefault: boolean
  countryName: string
  stateProvince: string | null
  countryCode: string | null
  stateCode: string | null
}

export interface AddressInput {
  addressType: AddressType
  firstName: string
  lastName?: string
  address1: string
  address2?: string
  city: string
  zipCode?: string
  phone?: string
  label?: string
  isDefault: boolean
  countryName: string
  stateProvince?: string
  countryCode?: string
  stateCode?: string
}
