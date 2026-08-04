export interface ProductClassificationAssignmentItem {
  taxonId: string
  position: number
}

export interface ClassificationItemsRequest {
  items: ProductClassificationAssignmentItem[]
}
