import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  OptionTypeRequest,
  OptionTypeListItem,
  OptionTypeDetail,
} from '../types/optionType'
import {
  OPTION_TYPE_FILTER_FIELDS,
  OPTION_TYPE_SORT_FIELDS,
} from '../types/optionType'

export class OptionTypeApi {
  static getOptionTypes(params: QueryingParameters): Promise<PagedResult<OptionTypeListItem>> {
    return getPaged<OptionTypeListItem>('/api/admin/catalog/option-types', params, {
      allowedFilterFields: OPTION_TYPE_FILTER_FIELDS,
      allowedSortFields: OPTION_TYPE_SORT_FIELDS,
      allowedSearchFields: ['name', 'presentation'],
    })
  }

  static getOptionType(id: string): Promise<Result<OptionTypeDetail>> {
    return get<Result<OptionTypeDetail>>(`/api/admin/catalog/option-types/${id}`)
  }

  static createOptionType(request: OptionTypeRequest): Promise<Result<OptionTypeDetail>> {
    return post<Result<OptionTypeDetail>>('/api/admin/catalog/option-types', request)
  }

  static updateOptionType(id: string, request: OptionTypeRequest): Promise<Result<OptionTypeDetail>> {
    return put<Result<OptionTypeDetail>>(`/api/admin/catalog/option-types/${id}`, request)
  }

  static deleteOptionType(id: string): Promise<Result<OptionTypeListItem>> {
    return del<Result<OptionTypeListItem>>(`/api/admin/catalog/option-types/${id}`)
  }
}
