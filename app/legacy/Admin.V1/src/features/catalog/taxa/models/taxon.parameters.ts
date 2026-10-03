export interface TaxonParameters {
  taxonomyId: string
  name: string
  presentation: string
  description?: string | null
  slug: string
  position: number
  hideFromNav: boolean
  parentId?: string | null
  automatic: boolean
  rulesMatchPolicy: string
  sortOrder: string
  metaTitle?: string | null
  metaDescription?: string | null
  metaKeywords?: string | null
}
