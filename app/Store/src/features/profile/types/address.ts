// Context: Address type discriminator for shipping vs billing vs other
export type AddressType = 'Shipping' | 'Billing' | 'Other'

// Context: Full address entity returned by the address API
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

// Context: Address input payload for create and update operations
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
