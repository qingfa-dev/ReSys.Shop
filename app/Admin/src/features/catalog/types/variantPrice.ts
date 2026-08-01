import type { QueryingParameters } from '@/shared/types/querying'

export interface Price {
  id: string
  variantId: string
  amount?: number
  currency: string
  compareAtAmount?: number
  countryIso?: string
}

export interface PriceRequest {
  amount?: number
  currency: string
  compareAtAmount?: number
  countryIso?: string
}

export interface PriceQuery {
  variantId?: string
  currency?: string
  filter?: string
  search?: string
  searchFields?: string[]
  searchMode?: string
  sortBy?: 'amount' | 'currency' | 'compareAtAmount'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const VARIANT_PRICE_FILTER_FIELDS = ['Currency', 'CountryIso', 'IsDefault', 'PriceListId', 'CompareAtAmount']

export const VARIANT_PRICE_SORT_FIELDS = ['Amount', 'Currency', 'CompareAtAmount']

export const VARIANT_PRICE_SEARCH_FIELDS = ['Currency', 'CountryIso']

export function toVariantPriceQueryParams(query: PriceQuery): QueryingParameters {
  const filters: string[] = []

  if (query.filter !== undefined && query.filter !== '') {
    filters.push(query.filter)
  }
  if (query.currency !== undefined && query.currency !== '') {
    filters.push(`currency=${query.currency}`)
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
