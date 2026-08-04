import type { Result } from '@/core/models/result'
import type { AddressSchemaType, StoreLocationSchemaType, GeoLocationSchemaType } from '../schemas'

export interface AddressResponse extends AddressSchemaType {}
export interface StoreLocationResponse extends StoreLocationSchemaType {}
export interface GeoLocationResponse extends GeoLocationSchemaType {}

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

export interface GetStoreLocationsResponse {
  stores: StoreLocationResponse[]
  totalCount: number
}

export interface FindNearestStoreResponse {
  store: StoreLocationResponse
  distance: number
}

export interface GetGeoLocationResponse {
  location: GeoLocationResponse
  formattedAddress: string
}

export type AddressSingleResponse = Result<AddressResponse>
export type AddressListResponse = Result<AddressResponse[]>
export type StoreLocationSingleResponse = Result<StoreLocationResponse>
export type StoreLocationListResponse = Result<StoreLocationResponse[]>