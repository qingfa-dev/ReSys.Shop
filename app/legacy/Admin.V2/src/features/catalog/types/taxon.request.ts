export interface TaxonRequest {
  name: string
  presentation?: string | null
  description?: string | null
  slug?: string
  position?: number
  hideFromNav?: boolean
  automatic?: boolean
  parentId?: string | null
}
