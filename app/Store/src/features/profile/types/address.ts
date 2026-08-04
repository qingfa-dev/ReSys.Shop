// Types mirror the storefront address DTOs exactly (camelCase JSON).
// Contracts pinned from Module.Profile.Features.Store.Addresses (service/Api):
// - GET/POST api/store/profiles/addresses
// - GET/PUT/DELETE api/store/profiles/addresses/{id}
//
// The location cascade (Task 5.4) stores Country.id / State.id while the address DTO
// carries display text + ISO codes, so the form maps:
//   country.id → countryName (name) + countryCode (isoCode)
//   state.id   → stateProvince (name) + stateCode (abbreviation)
// NOTE: the address DTO has NO companyName / zipPostalCode fields (older smoke tests
// are stale) — the wire uses zipCode, countryName, stateProvince, countryCode, stateCode.

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

// POST/PUT body — AddressRequest (AddressParameters; userId is set server-side).
export interface AddressInput {
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

// DELETE response — { id, label } confirmation.
export interface DeletedAddress {
  id: string
  label: string
}
