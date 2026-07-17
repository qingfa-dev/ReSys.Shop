import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { ProductClassification, ProductDetail, ProductSummary } from '../products/types/product.domain.types'
import type { CreateProductRequest, UpdateProductRequest } from '../products/types/product.request.types'
import type { OptionTypeDetail } from '../option-types/types/option-type.domain.types'

export const productRepository = {
  list: (params?: ServerQueryingParameters): Promise<ServerResult<ProductSummary[]>> =>
    apiClient.get(`${CATALOG}/products`, { params }).then(res => res.data as ServerResult<ProductSummary[]>),

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

  getOptionTypes: (productId: string): Promise<ServerResult<OptionTypeDetail[]>> =>
    apiClient.get(`${CATALOG}/products/${productId}/option-types`).then(res => res.data as ServerResult<OptionTypeDetail[]>),

  syncOptionTypes: (productId: string, optionTypeIds: string[]): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/products/${productId}/option-types/sync`, { optionTypeIds }).then(res => res.data as ServerResult<void>),

  getClassifications: (productId: string): Promise<ServerResult<ProductClassification[]>> =>
    apiClient.get(`${CATALOG}/products/${productId}/classifications`).then(res => res.data as ServerResult<ProductClassification[]>),

  syncClassifications: (productId: string, data: { taxonIds: string[]; mainTaxonId?: string }): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/products/${productId}/classifications/sync`, data).then(res => res.data as ServerResult<void>),
}
