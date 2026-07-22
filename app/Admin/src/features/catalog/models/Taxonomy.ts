export interface TaxonomyResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  createdAt: string
  updatedAt: string
}

export interface TaxonomyRequest {
  name: string
  presentation?: string | null
  position?: number
}

export interface TaxonomyListParams {
  page?: number
  pageSize?: number
  search?: string
}

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
