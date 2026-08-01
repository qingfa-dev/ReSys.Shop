import type { QueryingParameters } from '@/shared/types/querying'

export interface RoleRequest {
  name: string
  description?: string | null
  presentation?: string | null
}

export interface RoleListItem extends RoleRequest {
  id: string
  isSystem: boolean
}

export interface RoleDetail {
  id: string
  name: string
  description?: string | null
  presentation?: string | null
  isSystem: boolean
  createdAtUtc: string
  modifiedAtUtc?: string | null
  createdBy?: string | null
  modifiedBy?: string | null
}

export interface RoleQuery {
  name?: string
  search?: string
  sortBy?: 'name' | 'isSystem' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const ROLE_FILTER_FIELDS = [
  'isSystem',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const ROLE_SORT_FIELDS = [
  'name',
  'isSystem',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const ROLE_SEARCH_FIELDS = ['name', 'description']

export function toRoleQueryParams(query: RoleQuery): QueryingParameters {
  let sort: string[] | null = null
  if (query.sortBy) {
    const dir = query.sortDirection === 'desc' ? '-' : ''
    sort = [`${dir}${query.sortBy}`]
  }

  const nameSearch = query.name !== undefined && query.name !== '' ? query.name : null

  return {
    filter: null,
    search: nameSearch ?? query.search ?? null,
    searchFields: nameSearch ? ['name'] : undefined,
    sort,
    pageNumber: query.page ?? null,
    pageSize: query.pageSize ?? null,
  }
}
