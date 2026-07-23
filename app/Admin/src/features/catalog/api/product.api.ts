import apiClient from '@/shared/api/client'
import type { Result, PagedResult } from '@/shared/models'
import type { ProductResponse, CreateProductRequest, UpdateProductRequest, ProductListParams } from '../types'

export class ProductApi {
  static async getMany(params: ProductListParams = {}): Promise<PagedResult<ProductResponse>> {
    const res = await apiClient.get<PagedResult<ProductResponse>>('/catalog/products', { params })
    return res.data
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
}
