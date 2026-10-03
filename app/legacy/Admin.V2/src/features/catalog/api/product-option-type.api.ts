import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { ProductOptionTypesResponse, OptionTypeItemsRequest } from '../types'

export class ProductOptionTypeApi {
  static async get(productId: string): Promise<Result<ProductOptionTypesResponse>> {
    const res = await apiClient.get<Result<ProductOptionTypesResponse>>(`/catalog/products/${productId}/option-types`)
    return res.data
  }

  static async assign(productId: string, data: OptionTypeItemsRequest): Promise<Result<void>> {
    const res = await apiClient.post<Result<void>>(`/catalog/products/${productId}/option-types/assign`, data)
    return res.data
  }

  static async revoke(productId: string, data: OptionTypeItemsRequest): Promise<Result<void>> {
    const res = await apiClient.post<Result<void>>(`/catalog/products/${productId}/option-types/revoke`, data)
    return res.data
  }

  static async sync(productId: string, data: OptionTypeItemsRequest): Promise<Result<void>> {
    const res = await apiClient.put<Result<void>>(`/catalog/products/${productId}/option-types/sync`, data)
    return res.data
  }
}
