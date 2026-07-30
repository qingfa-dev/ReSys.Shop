import type { QueryingParameters } from '@/shared/types/querying'

export interface OptionValueRequest {
  optionTypeId: string
  name: string
  presentation: string
  position: number
}

export interface OptionValueListItem extends OptionValueRequest {
  id: string
}

export interface OptionValueDetail extends OptionValueListItem {
  createdAtUtc: string
  modifiedAtUtc: string | null
}

export interface OptionValueQuery {
  optionTypeId?: string
  name?: string
  search?: string
  sortBy?: 'name' | 'presentation' | 'position' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const OPTION_VALUE_FILTER_FIELDS = [
  'optionTypeId',
  'name',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const OPTION_VALUE_SORT_FIELDS = [
  'name',
  'presentation',
  'position',
  'createdAtUtc',
  'modifiedAtUtc',
]

export function toOptionValueQueryParams(query: OptionValueQuery): QueryingParameters {
  const filters: string[] = []

  if (query.optionTypeId !== undefined && query.optionTypeId !== '') {
    filters.push(`optionTypeId=${query.optionTypeId}`)
  }
  if (query.name !== undefined && query.name !== '') {
    filters.push(`name*=${query.name}`)
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
