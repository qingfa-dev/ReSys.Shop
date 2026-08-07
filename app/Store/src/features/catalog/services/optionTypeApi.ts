import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import { OptionTypeSchema, OptionValueSchema } from '../validations/optionType'
import { PagedResultSchema } from '@/shared/validations/result'
import type { PagedResult } from '@/shared/types'
import type { StoreOptionTypeListItem, StoreOptionValueListItemResponse } from '../types'
import type { QueryingParameters } from '@/shared/types/querying'

const optionTypeList = PagedResultSchema(OptionTypeSchema)
const optionValueList = PagedResultSchema(OptionValueSchema)

export class OptionTypeApi {
  static async getOptionTypes(q: QueryingParameters): Promise<PagedResult<StoreOptionTypeListItem>> {
    const result = await getPaged<unknown>(`${CATALOG}/option-types`, q)
    if (!result.isSuccess) return result as PagedResult<StoreOptionTypeListItem>
    const parsed = optionTypeList.parse({ ...result, items: result.items })
    return parsed as PagedResult<StoreOptionTypeListItem>
  }

  static async getOptionValues(q: QueryingParameters): Promise<PagedResult<StoreOptionValueListItemResponse>> {
    const result = await getPaged<unknown>(`${CATALOG}/option-values`, q)
    if (!result.isSuccess) return result as PagedResult<StoreOptionValueListItemResponse>
    const parsed = optionValueList.parse({ ...result, items: result.items })
    return parsed as PagedResult<StoreOptionValueListItemResponse>
  }
}
