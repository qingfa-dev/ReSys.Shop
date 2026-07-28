import type { QueryingParameters } from '@/shared/types/querying'

export interface StateRequest {
  name: string
  abbreviation: string
  countryId: string
  isActive: boolean
}

export interface StateListItem extends StateRequest {
  id: string
  countryName: string | null
}

export interface StateDetail extends Omit<StateListItem, 'countryName'> {
  createdAtUtc: string
  modifiedAtUtc: string | null
  createdBy: string | null
  modifiedBy: string | null
}

export interface StateQuery {
  name?: string
  abbreviation?: string
  countryId?: string
  isActive?: boolean
  search?: string
  sortBy?: 'name' | 'abbreviation' | 'countryName' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const STATE_FILTER_FIELDS = [
  'name',
  'abbreviation',
  'countryId',
  'isActive',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const STATE_SORT_FIELDS = [
  'name',
  'abbreviation',
  'countryId',
  'createdAtUtc',
  'modifiedAtUtc',
]

export function toStateQueryParams(query: StateQuery): QueryingParameters {
  const filters: string[] = []

  if (query.name !== undefined && query.name !== '') {
    filters.push(`name*=${query.name}`)
  }
  if (query.abbreviation !== undefined && query.abbreviation !== '') {
    filters.push(`abbreviation=${query.abbreviation}`)
  }
  if (query.countryId !== undefined && query.countryId !== '') {
    filters.push(`countryId=${query.countryId}`)
  }
  if (query.isActive !== undefined) {
    filters.push(`isActive=${query.isActive}`)
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
