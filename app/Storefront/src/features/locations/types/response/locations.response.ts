import type { Result } from '@/core/models/result'
import type { AddressSchemaType } from '../schemas'

export interface AddressResponse extends AddressSchemaType {}

export interface CreateAddressResponse {
  address: AddressResponse
  createdAt: string
}

export interface UpdateAddressResponse {
  address: AddressResponse
  updatedAt: string
}

export interface GetAddressesResponse {
  addresses: AddressResponse[]
  totalCount: number
}

export interface GetDefaultAddressResponse {
  address: AddressResponse | null
}

export interface SetDefaultAddressResponse {
  success: boolean
  previousDefaultId: string | null
}

export type AddressSingleResponse = Result<AddressResponse>
export type AddressListResponse = Result<AddressResponse[]>
