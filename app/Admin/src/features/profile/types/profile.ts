import type { QueryingParameters } from '@/shared/types/querying'

export interface ProfilePreferences {
  preferredStyle?: string
  preferredFit?: string
  favoriteColors: string[]
  favoriteCategories: string[]
  preferredBrands: string[]
  sizeTop?: string
  sizeBottom?: string
  shoeSize?: string
}

export interface ProfileNotificationPreferences {
  enableSms: boolean
  enableEmail: boolean
  enableNewsfeeds: boolean
}

export interface ProfileRequest {
  userId: string
  firstName: string
  lastName: string
  email: string
  phoneNumber?: string
  dateOfBirth?: string
  preferences?: ProfilePreferences
  notifications?: ProfileNotificationPreferences
}

export interface ProfileListItem extends ProfileRequest {
  id: string
  fullName: string
}

export interface ProfileDetail extends ProfileListItem {
  emailConfirmed: boolean
  phoneNumberConfirmed: boolean
  createdAtUtc: string
  modifiedAtUtc?: string
  createdBy?: string
  modifiedBy?: string
}

export interface ProfileQuery {
  gender?: string
  isActive?: boolean
  search?: string
  sortBy?: 'firstName' | 'lastName' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const CUSTOMER_FILTER_FIELDS = [
  'gender',
  'isActive',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const CUSTOMER_SORT_FIELDS = [
  'firstName',
  'lastName',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const CUSTOMER_SEARCH_FIELDS = ['firstName', 'lastName', 'email', 'bio']

export function toProfileQueryParams(query: ProfileQuery): QueryingParameters {
  const filters: string[] = []

  if (query.gender !== undefined && query.gender !== '') {
    filters.push(`gender=${query.gender}`)
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
    searchFields: CUSTOMER_SEARCH_FIELDS,
    sort,
    pageNumber: query.page ?? null,
    pageSize: query.pageSize ?? null,
  }
}
