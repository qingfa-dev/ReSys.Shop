import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  StateRequest,
  StateListItem,
  StateDetail,
} from '../types/state'
import {
  STATE_FILTER_FIELDS,
  STATE_SORT_FIELDS,
} from '../types/state'

export class StateApi {
  static getStates(params: QueryingParameters): Promise<PagedResult<StateListItem>> {
    return getPaged<StateListItem>('/api/admin/location/states', params, {
      allowedFilterFields: STATE_FILTER_FIELDS,
      allowedSortFields: STATE_SORT_FIELDS,
      allowedSearchFields: STATE_FILTER_FIELDS,
    })
  }

  static getState(id: string): Promise<Result<StateDetail>> {
    return get<Result<StateDetail>>(`/api/admin/location/states/${id}`)
  }

  static getStateByIso(isoCode: string): Promise<Result<StateDetail>> {
    return get<Result<StateDetail>>(`/api/admin/location/states/by-iso/${isoCode}`)
  }

  static createState(request: StateRequest): Promise<Result<StateDetail>> {
    return post<Result<StateDetail>>('/api/admin/location/states', request)
  }

  static updateState(id: string, request: StateRequest): Promise<Result<StateDetail>> {
    return put<Result<StateDetail>>(`/api/admin/location/states/${id}`, request)
  }

  static deleteState(id: string): Promise<Result<StateListItem>> {
    return del<Result<StateListItem>>(`/api/admin/location/states/${id}`)
  }
}
