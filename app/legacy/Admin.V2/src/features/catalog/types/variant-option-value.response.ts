export interface VariantOptionValueItem {
  optionValueId: string
  optionTypeId: string
  optionTypeName: string
  name: string
  presentation?: string | null
  isAssigned: boolean
}

export interface VariantOptionValuesResponse {
  items: VariantOptionValueItem[]
}
