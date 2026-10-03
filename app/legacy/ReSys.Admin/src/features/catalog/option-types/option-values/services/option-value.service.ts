import apiClient from '@/shared/api/api.client'
import type { ApiResult } from '@/shared/api/api.types'
import type {
  OptionValueListItem,
  CreateOptionValueRequest,
  UpdateOptionValueRequest,
  UpdateOptionValuePositionsRequest,
  OptionValueQuery,
} from '../types/option-value.types'

const BASE_URL = '/admin/catalog/option-values'

export const optionValueService = {
  /**
   * List all values (supports filtering by OptionTypeId via query params).
   */
  async list(query?: OptionValueQuery): Promise<ApiResult<OptionValueListItem[]>> {
    return (await apiClient.get<OptionValueListItem[]>(BASE_URL, { params: query })) as any
  },

  /**
   * Create a new Option Value (requires option_type_id in body).
   */
  async create(request: CreateOptionValueRequest): Promise<ApiResult<OptionValueListItem>> {
    return await apiClient.post<OptionValueListItem>(BASE_URL, request) as any;
  },

  /**
   * Update an Option Value by its unique ID.
   */
  async update(
    id: string,
    request: UpdateOptionValueRequest,
  ): Promise<ApiResult<OptionValueListItem>> {
    return (await apiClient.put<OptionValueListItem>(`${BASE_URL}/${id}`, request)) as any
  },

  /**
   * Delete an Option Value by its unique ID.
   */
  async delete(id: string): Promise<ApiResult<void>> {
    return (await apiClient.delete<void>(`${BASE_URL}/${id}`)) as any
  },

  /**
   * Reorder values within an Option Type.
   * Payload must contain the optionTypeId and the new positions.
   */
  async reorder(request: UpdateOptionValuePositionsRequest): Promise<ApiResult<void>> {
    return (await apiClient.put<void>(`${BASE_URL}/positions`, request)) as any
  },
}
