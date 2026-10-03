import apiClient from '@/shared/api/api.client'
import type { ApiResult } from '@/shared/api/api.types'
import type {
  OptionTypeListItem,
  OptionTypeDetail,
  CreateOptionTypeRequest,
  UpdateOptionTypeRequest,
  OptionTypeQuery,
} from '../types/option-type.types'

const BASE_URL = '/admin/catalog/option-types'

export const optionTypeService = {
  async getList(query?: OptionTypeQuery): Promise<ApiResult<OptionTypeListItem[]>> {
    return (await apiClient.get<OptionTypeListItem[]>(BASE_URL, { params: query })) as any
  },

  async getById(id: string): Promise<ApiResult<OptionTypeDetail>> {
    return (await apiClient.get<OptionTypeDetail>(`${BASE_URL}/${id}`)) as any
  },

  async create(request: CreateOptionTypeRequest): Promise<ApiResult<OptionTypeDetail>> {
    return (await apiClient.post<OptionTypeDetail>(BASE_URL, request)) as any
  },

  async update(id: string, request: UpdateOptionTypeRequest): Promise<ApiResult<OptionTypeDetail>> {
    return (await apiClient.put<OptionTypeDetail>(`${BASE_URL}/${id}`, request)) as any
  },

  async delete(id: string): Promise<ApiResult<void>> {
    return (await apiClient.delete<void>(`${BASE_URL}/${id}`)) as any
  },
}
