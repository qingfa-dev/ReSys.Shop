export interface ClassificationItem {
  taxonId: string
  position: number
  name: string
  prettyName?: string | null
  isAssigned: boolean
}

export interface ProductClassificationsResponse {
  items: ClassificationItem[]
}
