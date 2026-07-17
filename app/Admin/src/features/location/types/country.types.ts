import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'

export interface Country {
  id: string
  name: string
  isoCode: string
  callingCode: string
  isActive: boolean
  statesRequired?: boolean
  zipcodeRequired?: boolean
  createdAtUtc?: string
  modifiedAtUtc?: string
}

export type CountrySearchParams = ServerQueryingParameters

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
