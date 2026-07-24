import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { OptionTypeResponse, CreateOptionTypeRequest, UpdateOptionTypeRequest } from '../types'

export class OptionTypeApi {
  static getMany(query: ListQuery): Promise<PagedResult<OptionTypeResponse>> {
    return getPagedList<OptionTypeResponse>('/catalog/option-types', query)
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
