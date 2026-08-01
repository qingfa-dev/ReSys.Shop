import type { QueryingParameters } from '@/shared/types/querying'

export interface StockLocationRequest {
  name: string
  presentation?: string
  code?: string
  address1?: string
  address2?: string
  city?: string
  postalCode?: string
  phone?: string
  active: boolean
  default: boolean
  backorderableDefault: boolean
  propagateAllVariants: boolean
  position: number
}

export interface StockLocationListItem extends StockLocationRequest {
  id: string
  createdAtUtc: string
  modifiedAtUtc?: string
  createdBy?: string
  modifiedBy?: string
}

export type StockLocationDetail = StockLocationListItem

export interface StockLocationQuery {
  active?: boolean
  default?: boolean
  isDeleted?: boolean
  search?: string
  sortBy?: 'name' | 'code' | 'position' | 'createdAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const STOCK_LOCATION_FILTER_FIELDS = [
  'Active',
  'Default',
  'BackorderableDefault',
  'IsDeleted',
  'CountryId',
  'StateId',
]

export const STOCK_LOCATION_SORT_FIELDS = ['Name', 'Code', 'Position', 'CreatedAtUtc']

export const STOCK_LOCATION_SEARCH_FIELDS = ['Name', 'Code', 'City', 'AdminName']

export function toStockLocationQueryParams(query: StockLocationQuery): QueryingParameters {
  const filters: string[] = []

  if (query.active === true) {
    filters.push('Active=true')
  }
  if (query.default === true) {
    filters.push('Default=true')
  }
  if (query.isDeleted === true) {
    filters.push('IsDeleted=true')
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
