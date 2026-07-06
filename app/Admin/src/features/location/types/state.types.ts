export interface State {
  id: string
  name: string
  abbreviation: string
  countryId: string
  isActive: boolean
}

export interface StateCreateRequest {
  name: string
  abbreviation: string
  countryId: string
  isActive: boolean
}

export type StateUpdateRequest = StateCreateRequest
