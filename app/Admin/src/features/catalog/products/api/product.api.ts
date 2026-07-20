import apiClient from '@/common/api/http/api.client'
import { CATALOG } from '@/common/api/constants'
import type { ServerPagedResult, ServerResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { ProductDetail, ProductSummary } from '../types/product.response'
import type { CreateProductRequest, UpdateProductRequest } from '../types/product.request'
import type { ProductSummaryModel, ProductDetailModel } from '../models/product.model'
import type { ProductImage } from '../types/product-image.response'
import type { OptionTypeDetail } from '../../option-types/types/option-type.response'
import type { ProductClassification } from '../classifications/types/classification.response'
import type { SyncClassificationsRequest } from '../classifications/types/classification.request'
import { productOptionTypeApi } from '../option-types/api/product-option-type.api'
import { productClassificationApi } from '../classifications/api/product-classification.api'
import { mapValue, mapItems } from '@/common/utils/transform'
import { ProductStatusMap } from '@/shared/utils/enums'

export const productRepository = {
  list: async (params?: ServerQueryingParameters): Promise<ServerPagedResult<ProductSummaryModel>> => {
    const res = await apiClient.get(`${CATALOG}/products`, { params })
    const result = res.data as ServerPagedResult<ProductSummary>
    return mapItems(result, d => ({ ...d, statusLabel: ProductStatusMap[d.status] ?? 'Unknown' }))
  },

  getById: async (id: string): Promise<ServerResult<ProductDetailModel>> => {
    const res = await apiClient.get(`${CATALOG}/products/${id}`)
    const result = res.data as ServerResult<ProductDetail>
    return mapValue(result, d => ({ ...d, statusLabel: ProductStatusMap[d.status] ?? 'Unknown' }))
  },

  create: async (data: CreateProductRequest): Promise<ServerResult<ProductDetailModel>> => {
    const res = await apiClient.post(`${CATALOG}/products`, data)
    const result = res.data as ServerResult<ProductDetail>
    return mapValue(result, d => ({ ...d, statusLabel: ProductStatusMap[d.status] ?? 'Unknown' }))
  },

  update: async (id: string, data: UpdateProductRequest): Promise<ServerResult<ProductDetailModel>> => {
    const res = await apiClient.put(`${CATALOG}/products/${id}`, data)
    const result = res.data as ServerResult<ProductDetail>
    return mapValue(result, d => ({ ...d, statusLabel: ProductStatusMap[d.status] ?? 'Unknown' }))
  },

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/products/${id}`).then(res => res.data as ServerResult<void>),

  activate: (id: string): Promise<ServerResult<void>> =>
    apiClient.patch(`${CATALOG}/products/${id}/activate`).then(res => res.data as ServerResult<void>),

  discontinue: (id: string): Promise<ServerResult<void>> =>
    apiClient.patch(`${CATALOG}/products/${id}/discontinue`).then(res => res.data as ServerResult<void>),

  getOptionTypes: (productId: string): Promise<ServerResult<OptionTypeDetail[]>> =>
    productOptionTypeApi.getOptionTypes(productId),

  updateOptionTypes: (productId: string, optionTypeIds: string[]): Promise<ServerResult<void>> =>
    productOptionTypeApi.syncOptionTypes(productId, optionTypeIds),

  getClassifications: (productId: string): Promise<ServerResult<ProductClassification[]>> =>
    productClassificationApi.getClassifications(productId),

  syncClassifications: (
    productId: string,
    data: SyncClassificationsRequest,
  ): Promise<ServerResult<void>> => productClassificationApi.syncClassifications(productId, data),

  async getImages(_productId: string): Promise<ServerPagedResult<ProductImage>> {
    return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, items: [], page: 1, pageSize: 0, totalCount: 0 }
  },
  uploadImage: async (_productId: string, _file: File, _role?: number, _alt?: string): Promise<ServerResult<ProductImage>> => ({
    isSuccess: true,
    statusCode: 200,
    errors: [],
    message: null,
    metadata: null,
    value: {} as ProductImage,
  }),
  deleteImage: async (_imageId: string): Promise<ServerResult<void>> => {
    return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }
  },
  updateImage: async (_imageId: string, _data: { alt?: string; role?: number }): Promise<ServerResult<void>> => {
    return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }
  },
}
