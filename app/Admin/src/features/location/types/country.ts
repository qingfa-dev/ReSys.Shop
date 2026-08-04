import type { QueryingParameters } from '@/shared/types/querying'

export interface CountryRequest {
  name: string
  isoCode: string
  callingCode: string | null
  statesRequired: boolean
  isActive: boolean
}

export interface CountryListItem extends CountryRequest {
  id: string
}

export interface CountryDetail extends CountryListItem {
  createdAtUtc: string
  modifiedAtUtc: string | null
  createdBy: string | null
  modifiedBy: string | null
}

export interface CountryQuery {
  name?: string
  isoCode?: string
  callingCode?: string
  isActive?: boolean
  statesRequired?: boolean
  search?: string
  sortBy?: 'name' | 'isoCode' | 'callingCode' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const COUNTRY_FILTER_FIELDS = [
  'name',
  'isoCode',
  'callingCode',
  'isActive',
  'statesRequired',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const COUNTRY_SORT_FIELDS = [
  'name',
  'isoCode',
  'callingCode',
  'createdAtUtc',
  'modifiedAtUtc',
]

export function toCountryQueryParams(query: CountryQuery): QueryingParameters {
  const filters: string[] = []

  if (query.name !== undefined && query.name !== '') {
    filters.push(`name*=${query.name}`)
  }
  if (query.isoCode !== undefined && query.isoCode !== '') {
    filters.push(`isoCode=${query.isoCode}`)
  }
  if (query.callingCode !== undefined && query.callingCode !== '') {
    filters.push(`callingCode*=${query.callingCode}`)
  }
  if (query.isActive !== undefined) {
    filters.push(`isActive=${query.isActive}`)
  }
  if (query.statesRequired !== undefined) {
    filters.push(`statesRequired=${query.statesRequired}`)
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
