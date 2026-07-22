export interface ProductResponse {
  id: string
  name: string
  slug: string
  description: string | null
  status: ProductStatus
  styleCode: string | null
  seasonName: string | null
  department: string | null
  genderTarget: string | null
  metaTitle: string | null
  metaDescription: string | null
  metaKeywords: string | null
  availableOn: string | null
  discontinueOn: string | null
  materialComposition: string | null
  careInstructions: string | null
  fitNotes: string | null
  createdAt: string
  updatedAt: string
}

export type ProductStatus = 'Draft' | 'Active' | 'Archived'

export interface ProductRequest {
  name: string
  slug: string
  description?: string | null
  status?: ProductStatus
  styleCode?: string | null
  seasonName?: string | null
  department?: string | null
  genderTarget?: string | null
  metaTitle?: string | null
  metaDescription?: string | null
  metaKeywords?: string | null
  availableOn?: string | null
  discontinueOn?: string | null
  materialComposition?: string | null
  careInstructions?: string | null
  fitNotes?: string | null
}

export interface ProductListParams {
  page?: number
  pageSize?: number
  search?: string
  sortBy?: string
  sortDirection?: 'asc' | 'desc'
  status?: ProductStatus
}
