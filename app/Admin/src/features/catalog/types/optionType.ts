import type { QueryingParameters } from '@/shared/types/querying'

export interface OptionTypeRequest {
  name: string
  presentation: string
  position: number
  filterable: boolean
}

export interface OptionTypeListItem extends OptionTypeRequest {
  id: string
  optionValuesCount: number
  productsCount: number
}

export interface OptionTypeDetail extends OptionTypeListItem {
  createdAtUtc: string
  modifiedAtUtc: string | null
  createdBy: string | null
  modifiedBy: string | null
}

export interface OptionTypeQuery {
  name?: string
  filterable?: boolean
  search?: string
  sortBy?: 'name' | 'presentation' | 'position' | 'optionValuesCount' | 'productsCount' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const OPTION_TYPE_FILTER_FIELDS = [
  'name',
  'filterable',
  'optionValuesCount',
  'productsCount',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const OPTION_TYPE_SORT_FIELDS = [
  'name',
  'presentation',
  'position',
  'optionValuesCount',
  'productsCount',
  'createdAtUtc',
  'modifiedAtUtc',
]

export function toOptionTypeQueryParams(query: OptionTypeQuery): QueryingParameters {
  const filters: string[] = []

  if (query.name !== undefined && query.name !== '') {
    filters.push(`name*=${query.name}`)
  }
  if (query.filterable !== undefined) {
    filters.push(`filterable=${query.filterable}`)
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
