import apiClient from '@/shared/api/client'
import type { Result, PagedResult } from '@/shared/models'
import type { OptionTypeResponse, CreateOptionTypeRequest, UpdateOptionTypeRequest, OptionTypeListParams } from '../types'

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
}
