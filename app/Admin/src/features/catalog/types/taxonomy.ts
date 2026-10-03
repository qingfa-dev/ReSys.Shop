import type { QueryingParameters } from '@/shared/types/querying'

export interface TaxonomyRequest {
  name: string
  presentation: string
  position: number
}

export interface TaxonomyListItem extends TaxonomyRequest {
  id: string
  taxonsCount: number
  createdAtUtc: string
  modifiedAtUtc: string | null
}

export interface TaxonomyDetail extends TaxonomyListItem {
  createdBy: string | null
  modifiedBy: string | null
}

export interface TaxonomyQuery {
  name?: string
  search?: string
  sortBy?: 'name' | 'presentation' | 'position' | 'taxonsCount' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const TAXONOMY_FILTER_FIELDS = [
  'name',
  'taxonsCount',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const TAXONOMY_SORT_FIELDS = [
  'name',
  'presentation',
  'position',
  'taxonsCount',
  'createdAtUtc',
  'modifiedAtUtc',
]

export function toTaxonomyQueryParams(query: TaxonomyQuery): QueryingParameters {
  const filters: string[] = []

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
