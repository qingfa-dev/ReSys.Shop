export interface ProductOptionTypeItem {
  optionTypeId: string
  position: number
  name: string
  presentation?: string | null
  isAssigned: boolean
}

export interface ProductOptionTypesResponse {
  items: ProductOptionTypeItem[]
}
