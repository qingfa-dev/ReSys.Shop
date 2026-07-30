import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  OptionTypeRequest,
  OptionTypeListItem,
  OptionTypeDetail,
  OptionTypeQuery,
} from '../types/optionType'
import {
  toOptionTypeQueryParams,
  OPTION_TYPE_FILTER_FIELDS,
  OPTION_TYPE_SORT_FIELDS,
} from '../types/optionType'

export class OptionTypeApi {
  private static readonly BASE = `${CATALOG}/option-types`

  static getOptionTypes(query: OptionTypeQuery): Promise<PagedResult<OptionTypeListItem>> {
    return getPaged<OptionTypeListItem>(OptionTypeApi.BASE, toOptionTypeQueryParams(query), {
      allowedFilterFields: OPTION_TYPE_FILTER_FIELDS,
      allowedSortFields: OPTION_TYPE_SORT_FIELDS,
    })
  }

  static getOptionType(id: string): Promise<Result<OptionTypeDetail>> {
    return get<Result<OptionTypeDetail>>(`${OptionTypeApi.BASE}/${id}`)
  }

  static createOptionType(request: OptionTypeRequest): Promise<Result<OptionTypeDetail>> {
    return post<Result<OptionTypeDetail>>(OptionTypeApi.BASE, request)
  }

  static updateOptionType(id: string, request: OptionTypeRequest): Promise<Result<OptionTypeDetail>> {
    return put<Result<OptionTypeDetail>>(`${OptionTypeApi.BASE}/${id}`, request)
  }

  static deleteOptionType(id: string): Promise<Result<OptionTypeListItem>> {
    return del<Result<OptionTypeListItem>>(`${OptionTypeApi.BASE}/${id}`)
  }
}
