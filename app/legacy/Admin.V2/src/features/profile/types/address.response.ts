export interface AddressResponse {
  id: string
  firstName: string
  lastName: string
  address1: string
  address2?: string | null
  city: string
  state?: string | null
  postalCode: string
  country: string
  phone?: string | null
  isDefault: boolean
  createdAt: string
  updatedAt: string
}
