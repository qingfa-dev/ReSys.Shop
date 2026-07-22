import apiClient from '@/shared/api/client'
import { resultToMapped, pagedResultToMapped } from '@/shared/api/utils/result.mapper'
import type { MappedResult } from '@/shared/api/utils/result.mapper'
import type { Result, PagedResult, PaginationMeta } from '@/shared/models'
import type { ProductResponse, ProductRequest, ProductListParams } from '../models/Product'

export async function getProducts(
  params: ProductListParams = {},
): Promise<MappedResult<ProductResponse[]> & { meta?: PaginationMeta }> {
  const res = await apiClient.get<PagedResult<ProductResponse>>('/catalog/products', { params })
  return pagedResultToMapped(res.data)
}

export async function getProduct(id: string): Promise<MappedResult<ProductResponse>> {
  const res = await apiClient.get<Result<ProductResponse>>(`/catalog/products/${id}`)
  return resultToMapped(res.data)
}

export async function createProduct(data: ProductRequest): Promise<MappedResult<ProductResponse>> {
  const res = await apiClient.post<Result<ProductResponse>>('/catalog/products', data)
  return resultToMapped(res.data)
}

export async function updateProduct(
  id: string,
  data: ProductRequest,
): Promise<MappedResult<ProductResponse>> {
  const res = await apiClient.put<Result<ProductResponse>>(`/catalog/products/${id}`, data)
  return resultToMapped(res.data)
}

export async function deleteProduct(id: string): Promise<MappedResult<void>> {
  const res = await apiClient.delete<Result<void>>(`/catalog/products/${id}`)
  return resultToMapped(res.data)
}
