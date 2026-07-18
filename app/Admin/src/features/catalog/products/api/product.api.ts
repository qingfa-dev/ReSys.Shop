import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ServerPagedResult, ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { ProductDetail, ProductSummary } from '../types/Product.Response.Type'
import type { CreateProductRequest, UpdateProductRequest } from '../types/Product.Request.Type'

export const productRepository = {
  list: (params?: ServerQueryingParameters): Promise<ServerPagedResult<ProductSummary>> =>
    apiClient.get(`${CATALOG}/products`, { params }).then(res => res.data as ServerPagedResult<ProductSummary>),

  getById: (id: string): Promise<ServerResult<ProductDetail>> =>
    apiClient.get(`${CATALOG}/products/${id}`).then(res => res.data as ServerResult<ProductDetail>),

  create: (data: CreateProductRequest): Promise<ServerResult<ProductDetail>> =>
    apiClient.post(`${CATALOG}/products`, data).then(res => res.data as ServerResult<ProductDetail>),

  update: (id: string, data: UpdateProductRequest): Promise<ServerResult<ProductDetail>> =>
    apiClient.put(`${CATALOG}/products/${id}`, data).then(res => res.data as ServerResult<ProductDetail>),

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/products/${id}`).then(res => res.data as ServerResult<void>),

  activate: (id: string): Promise<ServerResult<void>> =>
    apiClient.patch(`${CATALOG}/products/${id}/activate`).then(res => res.data as ServerResult<void>),

  discontinue: (id: string): Promise<ServerResult<void>> =>
    apiClient.patch(`${CATALOG}/products/${id}/discontinue`).then(res => res.data as ServerResult<void>),
}
