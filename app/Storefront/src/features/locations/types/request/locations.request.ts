export interface CreateAddressRequest {
  firstName: string
  lastName: string
  address1: string
  address2?: string
  city: string
  state: string
  postalCode: string
  country: string
  phone?: string
  isDefault?: boolean
  instructions?: string
}

export interface UpdateAddressRequest {
  firstName?: string
  lastName?: string
  address1?: string
  address2?: string
  city?: string
  state?: string
  postalCode?: string
  country?: string
  phone?: string
  isDefault?: boolean
  instructions?: string
}
