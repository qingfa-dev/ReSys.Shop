import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult } from '@/shared/types'
import type {
  StateRequest,
  StateListItem,
  StateDetail,
  StateQuery,
} from '../types/state'
import {
  toStateQueryParams,
  STATE_FILTER_FIELDS,
  STATE_SORT_FIELDS,
} from '../types/state'

export class StateApi {
  private static readonly BASE = 'api/admin/location/states'

  static getStates(query: StateQuery): Promise<PagedResult<StateListItem>> {
    return getPaged<StateListItem>(StateApi.BASE, toStateQueryParams(query), {
      allowedFilterFields: STATE_FILTER_FIELDS,
      allowedSortFields: STATE_SORT_FIELDS,
    })
  }

  static getState(id: string): Promise<Result<StateDetail>> {
    return get<Result<StateDetail>>(`${StateApi.BASE}/${id}`)
  }

  static getStateByIso(isoCode: string): Promise<Result<StateDetail>> {
    return get<Result<StateDetail>>(`${StateApi.BASE}/by-iso/${isoCode}`)
  }

  static createState(request: StateRequest): Promise<Result<StateDetail>> {
    return post<Result<StateDetail>>(StateApi.BASE, request)
  }

  static updateState(id: string, request: StateRequest): Promise<Result<StateDetail>> {
    return put<Result<StateDetail>>(`${StateApi.BASE}/${id}`, request)
  }

  static deleteState(id: string): Promise<Result<StateListItem>> {
    return del<Result<StateListItem>>(`${StateApi.BASE}/${id}`)
  }
}
