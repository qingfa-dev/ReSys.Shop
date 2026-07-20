import apiClient from '@/common/api/http/api.client'
import { CATALOG } from '@/common/api/constants'
import type { ServerResult } from '@/common/api/types/result.types'
import type { VariantDetail, VariantSummary } from '../types/variant.response.type'
import type { CreateVariantRequest, UpdateVariantRequest } from '../types/variant.request.type'
import { mapValue } from '@/common/utils/transform'
import { decimalToDisplay } from '@/shared/utils/currency'
import type { VariantSummaryModel, VariantDetailModel } from '../types/variant.model.type'

export const variantRepository = {
  getById: async (id: string): Promise<ServerResult<VariantDetailModel>> => {
    const result = await apiClient.get(`${CATALOG}/variants/${id}`).then(res => res.data as ServerResult<VariantDetail>)
    return mapValue(result, d => ({ ...d, priceDisplay: decimalToDisplay(d.price) }))
  },

  listByProductId: async (productId: string): Promise<ServerResult<VariantSummaryModel[]>> => {
    const result = await apiClient.get(`${CATALOG}/products/${productId}/variants`).then(res => res.data as ServerResult<VariantSummary[]>)
    return mapValue(result, items => items.map(d => ({ ...d, priceDisplay: decimalToDisplay(d.price) })))
  },

  create: async (productId: string, data: CreateVariantRequest): Promise<ServerResult<VariantDetailModel>> => {
    const result = await apiClient.post(`${CATALOG}/products/${productId}/variants`, data).then(res => res.data as ServerResult<VariantDetail>)
    return mapValue(result, d => ({ ...d, priceDisplay: decimalToDisplay(d.price) }))
  },

  update: async (id: string, data: UpdateVariantRequest): Promise<ServerResult<VariantDetailModel>> => {
    const result = await apiClient.put(`${CATALOG}/variants/${id}`, data).then(res => res.data as ServerResult<VariantDetail>)
    return mapValue(result, d => ({ ...d, priceDisplay: decimalToDisplay(d.price) }))
  },

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/variants/${id}`).then(res => res.data as ServerResult<void>),

  syncOptionValues: (variantId: string, optionValueIds: string[]): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/variants/${variantId}/option-values/sync`, { optionValueIds }).then(res => res.data as ServerResult<void>),

  listVariantOptionValues: (variantId: string): Promise<ServerResult<string[]>> =>
    apiClient.get(`${CATALOG}/variants/${variantId}/option-values`).then(res => res.data as ServerResult<string[]>),
}
