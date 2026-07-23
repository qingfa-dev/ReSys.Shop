import apiClient from '@/shared/api/client'
import type { Result, PagedResult } from '@/shared/models'
import type { OptionTypeResponse, CreateOptionTypeRequest, UpdateOptionTypeRequest, OptionTypeListParams, OptionValueResponse, OptionValueRequest } from '../types'

export class OptionTypeApi {
  static async getMany(params: OptionTypeListParams = {}): Promise<PagedResult<OptionTypeResponse>> {
    const res = await apiClient.get<PagedResult<OptionTypeResponse>>('/catalog/option-types', { params })
    return res.data
  }

  static async get(id: string): Promise<Result<OptionTypeResponse>> {
    const res = await apiClient.get<Result<OptionTypeResponse>>(`/catalog/option-types/${id}`)
    return res.data
  }

  static async create(data: CreateOptionTypeRequest): Promise<Result<OptionTypeResponse>> {
    const res = await apiClient.post<Result<OptionTypeResponse>>('/catalog/option-types', data)
    return res.data
  }

  static async update(id: string, data: UpdateOptionTypeRequest): Promise<Result<OptionTypeResponse>> {
    const res = await apiClient.put<Result<OptionTypeResponse>>(`/catalog/option-types/${id}`, data)
    return res.data
  }

  static async delete(id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/catalog/option-types/${id}`)
    return res.data
  }

  static async getValues(optionTypeId: string): Promise<Result<OptionValueResponse[]>> {
    const res = await apiClient.get<Result<OptionValueResponse[]>>(`/catalog/option-types/${optionTypeId}/values`)
    return res.data
  }

  static async createValue(optionTypeId: string, data: OptionValueRequest): Promise<Result<OptionValueResponse>> {
    const res = await apiClient.post<Result<OptionValueResponse>>(`/catalog/option-types/${optionTypeId}/values`, data)
    return res.data
  }

  static async updateValue(optionTypeId: string, id: string, data: OptionValueRequest): Promise<Result<OptionValueResponse>> {
    const res = await apiClient.put<Result<OptionValueResponse>>(`/catalog/option-types/${optionTypeId}/values/${id}`, data)
    return res.data
  }

  static async deleteValue(optionTypeId: string, id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/catalog/option-types/${optionTypeId}/values/${id}`)
    return res.data
  }
}
