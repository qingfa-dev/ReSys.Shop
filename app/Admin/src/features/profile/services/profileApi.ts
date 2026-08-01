import { getPaged } from '@/shared/api'
import { post, put, del } from '@/shared/api/client'
import { PROFILE } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  ProfileRequest,
  ProfileListItem,
  ProfileDetail,
  ProfileQuery,
} from '../types/profile'
import {
  toProfileQueryParams,
  PROFILE_FILTER_FIELDS,
  PROFILE_SORT_FIELDS,
  PROFILE_SEARCH_FIELDS,
} from '../types/profile'

export class ProfileApi {
  private static readonly BASE = `${PROFILE}/profiles`

  static getProfiles(query: ProfileQuery): Promise<PagedResult<ProfileListItem>> {
    return getPaged<ProfileListItem>(`${ProfileApi.BASE}/all`, toProfileQueryParams(query), {
      allowedFilterFields: PROFILE_FILTER_FIELDS,
      allowedSortFields: PROFILE_SORT_FIELDS,
      allowedSearchFields: PROFILE_SEARCH_FIELDS,
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
