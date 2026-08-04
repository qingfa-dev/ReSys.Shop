import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { ProductResponse, CreateProductRequest, UpdateProductRequest } from '../types'

export class ProductApi {
  static getMany(query: ListQuery): Promise<PagedResult<ProductResponse>> {
    return getPagedList<ProductResponse>('/catalog/products', query)
  }

  static async get(id: string): Promise<Result<ProductResponse>> {
    const res = await apiClient.get<Result<ProductResponse>>(`/catalog/products/${id}`)
    return res.data
  }

  static async create(data: CreateProductRequest): Promise<Result<ProductResponse>> {
    const res = await apiClient.post<Result<ProductResponse>>('/catalog/products', data)
    return res.data
  }

  static async update(id: string, data: UpdateProductRequest): Promise<Result<ProductResponse>> {
    const res = await apiClient.put<Result<ProductResponse>>(`/catalog/products/${id}`, data)
    return res.data
  }

  static async delete(id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/catalog/products/${id}`)
    return res.data
  }

  static async activate(id: string): Promise<Result<ProductResponse>> {
    const res = await apiClient.patch<Result<ProductResponse>>(`/catalog/products/${id}/activate`)
    return res.data
  }

  static async discontinue(id: string): Promise<Result<ProductResponse>> {
    const res = await apiClient.patch<Result<ProductResponse>>(`/catalog/products/${id}/discontinue`)
    return res.data
  }
}
