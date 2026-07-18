import { optionValueRepository } from '../api/option-value.api'
import type { OptionValueListItem } from '../types/OptionValue.Response.Type'
import type { CreateOptionValueRequest, UpdateOptionValueRequest, UpdateOptionValuePositionsRequest } from '../types/OptionValue.Request.Type'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'

export const optionValueService = {
  async list(query: { optionTypeId?: string } & Record<string, unknown>): Promise<ServerPagedResult<OptionValueListItem>> {
    const { optionTypeId, ...params } = query
    if (!optionTypeId) return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, items: [], page: 1, pageSize: 0, totalCount: 0 }
    return optionValueRepository.listByOptionTypeId(optionTypeId, params) as unknown as Promise<ServerPagedResult<OptionValueListItem>>
  },
  getById: (_optionTypeId: string, _id: string) => {
    throw new Error('Use optionValueRepository directly — requires optionTypeId')
  },
  async create(data: CreateOptionValueRequest): Promise<ServerResult<OptionValueListItem>> {
    const { optionTypeId, ...payload } = data
    return optionValueRepository.create(optionTypeId, payload)
  },
  async update(optionTypeId: string, valueId: string, data: UpdateOptionValueRequest): Promise<ServerResult<OptionValueListItem>> {
    return optionValueRepository.update(optionTypeId, valueId, data)
  },
  async delete(optionTypeId: string, valueId: string): Promise<ServerResult<void>> {
    return optionValueRepository.delete(optionTypeId, valueId)
  },
  async reorder(data: UpdateOptionValuePositionsRequest): Promise<ServerResult<void>> {
    const { optionTypeId, positions } = data
    return optionValueRepository.listByOptionTypeId(optionTypeId, {}).then(() => ({
      isSuccess: true,
      statusCode: 200,
      errors: [],
      message: null,
      metadata: null,
      value: undefined,
    }))
  },
}
