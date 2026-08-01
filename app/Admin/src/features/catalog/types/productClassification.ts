export interface ClassificationAssignment {
  taxonId: string
  name: string
  prettyName: string | null
  position: number
  isAssigned: boolean
}

export interface ClassificationSyncItem {
  taxonId: string
  position: number
}

export interface ProductClassificationAssignmentRequest {
  productId: string
  items: ClassificationSyncItem[]
}
