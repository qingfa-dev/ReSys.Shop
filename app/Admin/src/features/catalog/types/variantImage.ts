import type { QueryingParameters } from '@/shared/types/querying'

export interface VariantImage {
  id: string
  variantId?: string | null
  url: string
  contentType: string
  fileName: string
  fileSize: number
  width?: number
  height?: number
  dimensionsUnit?: string
  alt?: string
  position: number
  type: string
  createdAtUtc: string
}

export interface VariantImageUploadRequest {
  variantId: string
  file: File
  alt?: string
  position?: number
  type?: string
}

export interface VariantImageUpdateRequest {
  alt?: string
  position?: number
  type?: string
}

export interface VariantImageQuery {
  filter?: string
  search?: string
  searchFields?: string[]
  searchMode?: string
  sortBy?: 'position' | 'createdAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const VARIANT_IMAGE_FILTER_FIELDS = ['Type', 'ContentType', 'DimensionsUnit']

export const VARIANT_IMAGE_SORT_FIELDS = ['Position', 'CreatedAtUtc']

export const VARIANT_IMAGE_SEARCH_FIELDS = ['FileName', 'Alt']

export function toVariantImageQueryParams(query: VariantImageQuery): QueryingParameters {
  const filters: string[] = []

  if (query.filter !== undefined && query.filter !== '') {
    filters.push(query.filter)
  }

  let sort: string[] | null = null
  if (query.sortBy) {
    const dir = query.sortDirection === 'desc' ? '-' : ''
    sort = [`${dir}${query.sortBy}`]
  }

  return {
    filter: filters.length > 0 ? filters.join(',') : null,
    search: query.search ?? null,
    searchFields: query.searchFields && query.searchFields.length > 0 ? query.searchFields : null,
    searchMode: query.searchMode ?? null,
    sort,
    pageNumber: query.page ?? null,
    pageSize: query.pageSize ?? null,
  }
}
