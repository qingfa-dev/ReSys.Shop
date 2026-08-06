export interface StoreOptionValueListItemResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  optionTypeId: string
  optionTypeName: string | null
}

export interface StoreOptionTypeListItem {
  id: string
  name: string
  presentation: string | null
  position: number
  filterable: boolean
}
