import type { QueryingParameters } from '@/shared/types/querying'

export interface ProductRequest {
  name: string
  slug: string
  description: string | null
  metaTitle: string | null
  metaDescription: string | null
  metaKeywords: string | null
  availableOn: string | null
  discontinueOn: string | null
  trackInventory: boolean
  styleCode: string | null
  seasonName: string | null
  materialComposition: string | null
  careInstructions: string | null
  fitNotes: string | null
  department: string | null
  genderTarget: string | null
}

export interface ProductListItem extends ProductRequest {
  id: string
  status: 'Draft' | 'Active' | 'Archived'
  masterVariantId: string
  variantsCount: number
  createdAtUtc: string
  modifiedAtUtc: string | null
}

export type ProductDetail = ProductListItem

export interface ProductQuery {
  status?: 'Draft' | 'Active' | 'Archived'
  season?: string
  taxonId?: string
  search?: string
  sortBy?: 'name' | 'createdAtUtc' | 'modifiedAtUtc' | 'availableOn' | 'variantsCount'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const PRODUCT_FILTER_FIELDS = [
  'status',
  'seasonName',
  'department',
  'createdAtUtc',
  'availableOn',
]

export const PRODUCT_SORT_FIELDS = [
  'name',
  'createdAtUtc',
  'modifiedAtUtc',
  'availableOn',
  'variantsCount',
]

export function toProductQueryParams(query: ProductQuery): QueryingParameters {
  const filters: string[] = []

  if (query.status !== undefined && query.status !== '') {
    filters.push(`status=${query.status}`)
  }
  if (query.season !== undefined && query.season !== '') {
    filters.push(`seasonName*=${query.season}`)
  }
  if (query.taxonId !== undefined && query.taxonId !== '') {
    filters.push(`taxonId=${query.taxonId}`)
  }

  let sort: string[] | null = null
  if (query.sortBy) {
    const dir = query.sortDirection === 'desc' ? '-' : ''
    sort = [`${dir}${query.sortBy}`]
  }

  return {
    filter: filters.length > 0 ? filters.join(',') : null,
    search: query.search ?? null,
    sort,
    pageNumber: query.page ?? null,
    pageSize: query.pageSize ?? null,
  }
}
