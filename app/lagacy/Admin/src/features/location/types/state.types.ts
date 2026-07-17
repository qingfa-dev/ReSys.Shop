import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'

export interface State {
  id: string
  name: string
  abbreviation: string
  countryId: string
  countryName?: string
  isActive: boolean
}

export type StateSearchParams = ServerQueryingParameters

export interface StateCreateRequest {
  name: string
  abbreviation: string
  countryId: string
  isActive: boolean
}

export type StateUpdateRequest = StateCreateRequest
