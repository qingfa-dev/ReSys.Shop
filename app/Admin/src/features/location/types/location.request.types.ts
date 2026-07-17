import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export type CountrySearchParams = ServerQueryingParameters
export type StateSearchParams = ServerQueryingParameters

export interface CountryCreateRequest {
  name: string
  isoCode: string
  callingCode: string
  isActive: boolean
}

export interface CountryUpdateRequest {
  name: string
  isoCode: string
  callingCode: string
  isActive: boolean
}

export interface StateCreateRequest {
  name: string
  abbreviation: string
  countryId: string
  isActive: boolean
}

export type StateUpdateRequest = StateCreateRequest
