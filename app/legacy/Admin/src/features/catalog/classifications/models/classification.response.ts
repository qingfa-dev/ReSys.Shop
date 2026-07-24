export interface ProductClassification {
  id: string
  productId: string
  taxonId: string
  position: number
  isAutomatic: boolean
  isMain: boolean
  taxonName?: string
  taxonomyName?: string
}

export type ClassificationListItem = ProductClassification
