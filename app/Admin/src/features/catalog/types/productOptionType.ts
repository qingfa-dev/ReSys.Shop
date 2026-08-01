export interface OptionTypeAssignment {
  optionTypeId: string
  name: string
  presentation: string | null
  position: number
  isAssigned: boolean
}

export interface OptionTypeSyncItem {
  optionTypeId: string
  position: number
}

export interface ProductOptionTypeAssignmentRequest {
  productId: string
  items: OptionTypeSyncItem[]
}
