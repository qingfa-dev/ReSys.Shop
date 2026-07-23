export interface TaxonResponse {
  id: string
  name: string
  presentation: string | null
  description: string | null
  slug: string
  position: number
  depth: number
  lft: number
  rgt: number
  childrenCount: number
  hideFromNav: boolean
  automatic: boolean
  taxonomyId: string
  parentId: string | null
  createdAt: string
  updatedAt: string
}
