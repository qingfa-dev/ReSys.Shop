export interface StoreOptionValueResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  optionTypeId: string
}

export interface StoreOptionTypeResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  filterable: boolean
  values: StoreOptionValueResponse[]
}
