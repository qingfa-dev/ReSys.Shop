export interface OptionTypeResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  filterable: boolean
  createdAt: string
  updatedAt: string
}

export interface OptionTypeRequest {
  name: string
  presentation?: string | null
  position?: number
  filterable?: boolean
}

export interface OptionTypeListParams {
  page?: number
  pageSize?: number
  search?: string
}

export interface OptionValueResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  optionTypeId: string
}

export interface OptionValueRequest {
  name: string
  presentation?: string | null
  position?: number
}
