import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { VariantDetailResponse, VariantListItemResponse, VariantRequest } from '../types'

export class VariantApi {
  static getMany(productId: string, query: ListQuery): Promise<PagedResult<VariantListItemResponse>> {
    return getPagedList<VariantListItemResponse>(`/catalog/products/${productId}/variants`, query)
  }
  static async get(id: string): Promise<Result<VariantDetailResponse>> {
    const res = await apiClient.get<Result<VariantDetailResponse>>(`/catalog/products/variants/${id}`)
    return res.data
  }
  static async create(productId: string, data: VariantRequest): Promise<Result<VariantDetailResponse>> {
    const res = await apiClient.post<Result<VariantDetailResponse>>(`/catalog/products/${productId}/variants`, data)
    return res.data
  }
  static async update(id: string, data: VariantRequest): Promise<Result<VariantDetailResponse>> {
    const res = await apiClient.put<Result<VariantDetailResponse>>(`/catalog/products/variants/${id}`, data)
    return res.data
  }
  static async delete(id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/catalog/products/variants/${id}`)
    return res.data
  }
}
