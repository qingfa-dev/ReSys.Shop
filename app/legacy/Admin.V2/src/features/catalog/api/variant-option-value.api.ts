import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { VariantOptionValuesResponse, OptionValueIdsRequest } from '../types'

export class VariantOptionValueApi {
  static async get(variantId: string): Promise<Result<VariantOptionValuesResponse>> {
    const res = await apiClient.get<Result<VariantOptionValuesResponse>>(`/catalog/variants/${variantId}/option-values`)
    return res.data
  }

  static async assign(variantId: string, data: OptionValueIdsRequest): Promise<Result<void>> {
    const res = await apiClient.post<Result<void>>(`/catalog/variants/${variantId}/option-values/assign`, data)
    return res.data
  }

  static async revoke(variantId: string, data: OptionValueIdsRequest): Promise<Result<void>> {
    const res = await apiClient.post<Result<void>>(`/catalog/variants/${variantId}/option-values/revoke`, data)
    return res.data
  }

  static async sync(variantId: string, data: OptionValueIdsRequest): Promise<Result<void>> {
    const res = await apiClient.put<Result<void>>(`/catalog/variants/${variantId}/option-values/sync`, data)
    return res.data
  }
}
