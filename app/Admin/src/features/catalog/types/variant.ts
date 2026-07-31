import type { QueryingParameters } from '@/shared/types/querying'

export interface VariantParameters {
  sku: string
  position: number
  trackInventory: boolean
  weight?: number
  weightUnit?: string
  height?: number
  width?: number
  depth?: number
  dimensionsUnit?: string
  price?: number
  costPrice?: number
  costCurrency?: string
}

export interface VariantRequest extends VariantParameters {
  productId: string
  isMaster: boolean
  optionValueIds?: string[]
}

export interface VariantListItem extends VariantParameters {
  id: string
  productId: string
  isMaster: boolean
  discontinuedOn?: string | null
  pricesCount: number
}

export type VariantDetail = VariantListItem

export interface VariantQuery {
  search?: string
  isMaster?: boolean
  sortBy?: 'sku' | 'position' | 'price' | 'weight' | 'height' | 'width' | 'depth'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const VARIANT_FILTER_FIELDS = [
  'isMaster',
  'trackInventory',
  'discontinuedOn',
  'dimensionsUnit',
  'weightUnit',
]

export const VARIANT_SORT_FIELDS = [
  'sku',
  'position',
  'price',
  'weight',
  'height',
  'width',
  'depth',
]

export const VARIANT_SEARCH_FIELDS = ['sku', 'barcode', 'hsCode']

export function toVariantQueryParams(query: VariantQuery): QueryingParameters {
  const filters: string[] = []

  if (query.isMaster === true) {
    filters.push('isMaster=true')
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

export interface VariantImage {
  id: string
  variantId: string
  url: string
  contentType: string
  fileName: string
  fileSize: number
  width?: number
  height?: number
  alt?: string
  position: number
  type: string
  createdAtUtc: string
}

export interface Price {
  id: string
  variantId: string
  amount?: number
  currency: string
  compareAtAmount?: number
  countryIso?: string
}

export interface OptionValueAssignment {
  optionValueId: string
  optionTypeId: string
  optionTypeName: string
  name: string
  presentation: string
  isAssigned: boolean
}
