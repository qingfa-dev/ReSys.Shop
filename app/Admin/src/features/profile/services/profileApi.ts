import { getPaged } from '@/shared/api'
import { post, put, del } from '@/shared/api/client'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  ProfileRequest,
  ProfileListItem,
  ProfileDetail,
} from '../types/profile'
import {
  CUSTOMER_FILTER_FIELDS,
  CUSTOMER_SORT_FIELDS,
  CUSTOMER_SEARCH_FIELDS,
} from '../types/profile'

export class ProfileApi {
  static getProfiles(params: QueryingParameters): Promise<PagedResult<ProfileListItem>> {
    return getPaged<ProfileListItem>('/api/admin/customer/all', params, {
      allowedFilterFields: CUSTOMER_FILTER_FIELDS,
      allowedSortFields: CUSTOMER_SORT_FIELDS,
      allowedSearchFields: CUSTOMER_SEARCH_FIELDS,
    })
  }

  static createProfile(request: ProfileRequest): Promise<Result<ProfileDetail>> {
    return post<Result<ProfileDetail>>('/api/admin/customer', request)
  }

  static updateProfile(request: ProfileRequest): Promise<Result<ProfileDetail>> {
    return put<Result<ProfileDetail>>('/api/admin/customer', request)
  }

  static deleteProfile(userId: string): Promise<Result<void>> {
    return del<Result<void>>(`/api/admin/customer?userId=${userId}`)
  }
}