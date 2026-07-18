export interface AddressDetail {
  id: string
  userId: string
  address1: string
  address2: string | null
  city: string
  stateProvince: string
  postalCode: string
  country: string
  isDefault: boolean
}
