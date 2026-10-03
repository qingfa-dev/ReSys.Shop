import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { OptionValueResponse, OptionValueRequest } from '../types'

export class OptionValueApi {
  static async getMany(optionTypeId: string): Promise<Result<OptionValueResponse[]>> {
    const res = await apiClient.get<Result<OptionValueResponse[]>>(`/catalog/option-types/${optionTypeId}/values`)
    return res.data
  }

  static async create(optionTypeId: string, data: OptionValueRequest): Promise<Result<OptionValueResponse>> {
    const res = await apiClient.post<Result<OptionValueResponse>>(`/catalog/option-types/${optionTypeId}/values`, data)
    return res.data
  }

  static async update(optionTypeId: string, id: string, data: OptionValueRequest): Promise<Result<OptionValueResponse>> {
    const res = await apiClient.put<Result<OptionValueResponse>>(`/catalog/option-types/${optionTypeId}/values/${id}`, data)
    return res.data
  }

  static async delete(optionTypeId: string, id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/catalog/option-types/${optionTypeId}/values/${id}`)
    return res.data
  }
}
