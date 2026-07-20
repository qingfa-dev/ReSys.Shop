import apiClient from '@/common/api/http/api.client'
import { CATALOG } from '@/common/api/constants'
import type { ServerPagedResult, ServerResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { ProductDetail, ProductSummary } from '../types/product.response.type'
import type { CreateProductRequest, UpdateProductRequest } from '../types/product.request.type'
import type { ProductSummaryModel, ProductDetailModel } from '../types/product.model.type'
import { mapValue, mapItems } from '@/common/utils/transform'
import { ProductStatusMap } from '@/shared/utils/enums'

export const productRepository = {
  list: async (params?: ServerQueryingParameters): Promise<ServerPagedResult<ProductSummaryModel>> => {
    const result = await apiClient.get(`${CATALOG}/products`, { params }).then(res => res.data as ServerPagedResult<ProductSummary>)
    return mapItems(result, d => ({ ...d, statusLabel: ProductStatusMap[d.status] ?? 'Unknown' }))
  },

  getById: async (id: string): Promise<ServerResult<ProductDetailModel>> => {
    const result = await apiClient.get(`${CATALOG}/products/${id}`).then(res => res.data as ServerResult<ProductDetail>)
    return mapValue(result, d => ({ ...d, statusLabel: ProductStatusMap[d.status] ?? 'Unknown' }))
  },

  create: async (data: CreateProductRequest): Promise<ServerResult<ProductDetailModel>> => {
    const result = await apiClient.post(`${CATALOG}/products`, data).then(res => res.data as ServerResult<ProductDetail>)
    return mapValue(result, d => ({ ...d, statusLabel: ProductStatusMap[d.status] ?? 'Unknown' }))
  },

  update: async (id: string, data: UpdateProductRequest): Promise<ServerResult<ProductDetailModel>> => {
    const result = await apiClient.put(`${CATALOG}/products/${id}`, data).then(res => res.data as ServerResult<ProductDetail>)
    return mapValue(result, d => ({ ...d, statusLabel: ProductStatusMap[d.status] ?? 'Unknown' }))
  },

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/products/${id}`).then(res => res.data as ServerResult<void>),

  activate: (id: string): Promise<ServerResult<void>> =>
    apiClient.patch(`${CATALOG}/products/${id}/activate`).then(res => res.data as ServerResult<void>),

  discontinue: (id: string): Promise<ServerResult<void>> =>
    apiClient.patch(`${CATALOG}/products/${id}/discontinue`).then(res => res.data as ServerResult<void>),
}
