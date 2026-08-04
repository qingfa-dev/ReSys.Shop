import { post, put, del, getPaged } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types/result'
import type { Address, AddressInput, DeletedAddress } from '../types/address'

// GET api/store/profiles/addresses — PagedResult envelope; no paging params → all rows.
export function getAddresses(): Promise<PagedResult<Address>> {
  return getPaged<Address>(ENDPOINTS.addresses, {})
}

// POST api/store/profiles/addresses — 201 with the created address.
export function createAddress(req: AddressInput): Promise<Result<Address>> {
  return post<Result<Address>>(ENDPOINTS.addresses, req)
}

// PUT api/store/profiles/addresses/{id} — full replacement of address fields.
// There is no dedicated `{id}/default` route; Set-default sends isDefault: true here.
export function updateAddress(id: string, req: AddressInput): Promise<Result<Address>> {
  return put<Result<Address>>(ENDPOINTS.addressById(id), req)
}

// DELETE api/store/profiles/addresses/{id} — returns { id, label } confirmation.
export function deleteAddress(id: string): Promise<Result<DeletedAddress>> {
  return del<Result<DeletedAddress>>(ENDPOINTS.addressById(id))
}
