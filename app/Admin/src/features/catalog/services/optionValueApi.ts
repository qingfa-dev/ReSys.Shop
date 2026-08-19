import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  OptionValueRequest,
  OptionValueListItem,
  OptionValueDetail,
} from '../types/optionValue'
import {
  OPTION_VALUE_FILTER_FIELDS,
  OPTION_VALUE_SORT_FIELDS,
} from '../types/optionValue'

export class OptionValueApi {
  static getOptionValues(params: QueryingParameters): Promise<PagedResult<OptionValueListItem>> {
    return getPaged<OptionValueListItem>('/api/admin/catalog/option-values', params, {
      allowedFilterFields: OPTION_VALUE_FILTER_FIELDS,
      allowedSortFields: OPTION_VALUE_SORT_FIELDS,
      allowedSearchFields: ['name', 'presentation'],
    })
  }

  static getOptionValue(id: string): Promise<Result<OptionValueDetail>> {
    return get<Result<OptionValueDetail>>(`/api/admin/catalog/option-values/${id}`)
  }

  static createOptionValue(request: OptionValueRequest): Promise<Result<OptionValueDetail>> {
    return post<Result<OptionValueDetail>>('/api/admin/catalog/option-values', request)
  }

  static updateOptionValue(id: string, request: OptionValueRequest): Promise<Result<OptionValueDetail>> {
    return put<Result<OptionValueDetail>>(`/api/admin/catalog/option-values/${id}`, request)
  }

  static deleteOptionValue(id: string): Promise<Result<OptionValueListItem>> {
    return del<Result<OptionValueListItem>>(`/api/admin/catalog/option-values/${id}`)
  }
}
