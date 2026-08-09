import { getPaged } from '@/shared/api'
import { post, put, del } from '@/shared/api/client'
import type { Result, PagedResult } from '@/shared/types'
import type {
  ProfileRequest,
  ProfileListItem,
  ProfileDetail,
  ProfileQuery,
} from '../types/profile'
import {
  toProfileQueryParams,
  CUSTOMER_FILTER_FIELDS,
  CUSTOMER_SORT_FIELDS,
  CUSTOMER_SEARCH_FIELDS,
} from '../types/profile'

export class ProfileApi {
  private static readonly BASE = 'api/admin/customer'

  static getProfiles(query: ProfileQuery): Promise<PagedResult<ProfileListItem>> {
    return getPaged<ProfileListItem>(`${ProfileApi.BASE}/all`, toProfileQueryParams(query), {
      allowedFilterFields: CUSTOMER_FILTER_FIELDS,
      allowedSortFields: CUSTOMER_SORT_FIELDS,
      allowedSearchFields: CUSTOMER_SEARCH_FIELDS,
    })
  }

  static createProfile(request: ProfileRequest): Promise<Result<ProfileDetail>> {
    return post<Result<ProfileDetail>>(ProfileApi.BASE, request)
  }

  static updateProfile(request: ProfileRequest): Promise<Result<ProfileDetail>> {
    return put<Result<ProfileDetail>>(ProfileApi.BASE, request)
  }

  static deleteProfile(userId: string): Promise<Result<void>> {
    return del<Result<void>>(`${ProfileApi.BASE}?userId=${userId}`)
  }
}
