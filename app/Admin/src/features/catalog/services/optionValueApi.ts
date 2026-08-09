import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult } from '@/shared/types'
import type {
  OptionValueRequest,
  OptionValueListItem,
  OptionValueDetail,
  OptionValueQuery,
} from '../types/optionValue'
import {
  toOptionValueQueryParams,
  OPTION_VALUE_FILTER_FIELDS,
  OPTION_VALUE_SORT_FIELDS,
} from '../types/optionValue'

export class OptionValueApi {
  private static readonly BASE = 'api/admin/catalog/option-values'

  static getOptionValues(query: OptionValueQuery): Promise<PagedResult<OptionValueListItem>> {
    return getPaged<OptionValueListItem>(OptionValueApi.BASE, toOptionValueQueryParams(query), {
      allowedFilterFields: OPTION_VALUE_FILTER_FIELDS,
      allowedSortFields: OPTION_VALUE_SORT_FIELDS,
    })
  }

  static getOptionValue(id: string): Promise<Result<OptionValueDetail>> {
    return get<Result<OptionValueDetail>>(`${OptionValueApi.BASE}/${id}`)
  }

  static createOptionValue(request: OptionValueRequest): Promise<Result<OptionValueDetail>> {
    return post<Result<OptionValueDetail>>(OptionValueApi.BASE, request)
  }

  static updateOptionValue(id: string, request: OptionValueRequest): Promise<Result<OptionValueDetail>> {
    return put<Result<OptionValueDetail>>(`${OptionValueApi.BASE}/${id}`, request)
  }

  static deleteOptionValue(id: string): Promise<Result<OptionValueListItem>> {
    return del<Result<OptionValueListItem>>(`${OptionValueApi.BASE}/${id}`)
  }
}
