import { catalogApi } from '../../../services/catalog.api'
import type { OptionValueListItem, CreateOptionValueRequest, UpdateOptionValueRequest, UpdateOptionValuePositionsRequest } from '../types/option-value.types'
import type { ApiResult } from '@/shared/api/types/api.types'

export const optionValueService = {
  async list(query: { optionTypeId?: string } & Record<string, unknown>): Promise<ApiResult<OptionValueListItem[]>> {
    const { optionTypeId, ...params } = query
    if (!optionTypeId) return { success: true, data: [] }
    return catalogApi.optionTypes.listValues(optionTypeId, params)
  },
  getById: (_optionTypeId: string, _id: string) => {
    throw new Error('Use catalogApi.optionTypes directly — requires optionTypeId')
  },
  async create(data: CreateOptionValueRequest): Promise<ApiResult<OptionValueListItem>> {
    const { optionTypeId, ...payload } = data
    return catalogApi.optionTypes.createValue(optionTypeId, payload)
  },
  async update(optionTypeId: string, valueId: string, data: UpdateOptionValueRequest): Promise<ApiResult<OptionValueListItem>> {
    return catalogApi.optionTypes.updateValue(optionTypeId, valueId, data)
  },
  async delete(optionTypeId: string, valueId: string): Promise<ApiResult<void>> {
    return catalogApi.optionTypes.deleteValue(optionTypeId, valueId)
  },
  async reorder(data: UpdateOptionValuePositionsRequest): Promise<ApiResult<void>> {
    const { optionTypeId, positions } = data
    return catalogApi.optionTypes.listValues(optionTypeId, {}).then(() => ({
      success: true as const,
      data: undefined,
    }))
  },
}
