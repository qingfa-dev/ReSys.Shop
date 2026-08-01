import type { QueryingParameters } from '@/shared/types/querying'

export interface UserRequest {
  email: string
  userName: string
  firstName: string
  lastName: string
  phoneNumber?: string | null
  emailConfirmed: boolean
  phoneNumberConfirmed: boolean
}

export interface UserListItem extends UserRequest {
  id: string
  fullName: string
  isActive: boolean
}

export interface UserDetail {
  id: string
  isActive: boolean
  createdAtUtc: string
  modifiedAtUtc?: string | null
  email: string
  userName: string
  firstName: string
  lastName: string
  phoneNumber?: string | null
  emailConfirmed: boolean
  phoneNumberConfirmed: boolean
}

export interface UserQuery {
  search?: string
  sortBy?: 'userName' | 'email' | 'createdAtUtc' | 'modifiedAtUtc' | 'lastLoginAtUtc'
  sortDirection?: 'asc' | 'desc'
  isActive?: boolean
  emailConfirmed?: boolean
  phoneNumberConfirmed?: boolean
  page?: number
  pageSize?: number
}

export const USER_FILTER_FIELDS = [
  'isActive',
  'emailConfirmed',
  'phoneNumberConfirmed',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const USER_SORT_FIELDS = [
  'userName',
  'email',
  'createdAtUtc',
  'modifiedAtUtc',
  'lastLoginAtUtc',
]

export const USER_SEARCH_FIELDS = ['userName', 'email', 'firstName', 'lastName']

export function toUserQueryParams(query: UserQuery): QueryingParameters {
  const filters: string[] = []

  if (query.isActive !== undefined) {
    filters.push(`isActive=${query.isActive}`)
  }
  if (query.emailConfirmed !== undefined) {
    filters.push(`emailConfirmed=${query.emailConfirmed}`)
  }
  if (query.phoneNumberConfirmed !== undefined) {
    filters.push(`phoneNumberConfirmed=${query.phoneNumberConfirmed}`)
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
