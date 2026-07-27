import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { ProductClassificationsResponse, ClassificationItemsRequest } from '../types'

export class ProductClassificationApi {
  static async get(productId: string): Promise<Result<ProductClassificationsResponse>> {
    const res = await apiClient.get<Result<ProductClassificationsResponse>>(`/catalog/products/${productId}/classifications`)
    return res.data
  }

  static async assign(productId: string, data: ClassificationItemsRequest): Promise<Result<void>> {
    const res = await apiClient.post<Result<void>>(`/catalog/products/${productId}/classifications/assign`, data)
    return res.data
  }

  static async revoke(productId: string, data: ClassificationItemsRequest): Promise<Result<void>> {
    const res = await apiClient.post<Result<void>>(`/catalog/products/${productId}/classifications/revoke`, data)
    return res.data
  }

  static async sync(productId: string, data: ClassificationItemsRequest): Promise<Result<void>> {
    const res = await apiClient.put<Result<void>>(`/catalog/products/${productId}/classifications/sync`, data)
    return res.data
  }
}
