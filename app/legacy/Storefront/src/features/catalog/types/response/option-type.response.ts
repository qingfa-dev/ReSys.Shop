import type { PagedResult } from '@/core/models/result'

export interface StoreOptionValueResponse {
  id: string
  name: string
  presentation?: string
  position: number
  optionTypeId: string
}

export interface StoreOptionTypeResponse {
  id: string
  name: string
  presentation?: string
  position: number
  filterable: boolean
  values: StoreOptionValueResponse[]
}

export type OptionTypeListResponse = PagedResult<StoreOptionTypeResponse>
