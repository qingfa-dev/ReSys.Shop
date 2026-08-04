import apiClient from '@/common/api/http/api.client'
import { CATALOG } from '@/common/api/constants'
import type { ServerResult } from '@/common/api/types/result.types'
import type { VariantDetail, VariantSummary } from '../models/variant.response'
import type { CreateVariantRequest, UpdateVariantRequest } from '../models/variant.request'
import type { VariantSummaryModel, VariantDetailModel } from '../models/variant.model'
import { VariantMapper } from './variant.mapper'

export const variantRepository = {
  getById: async (id: string): Promise<ServerResult<VariantDetailModel>> => {
    const result = await apiClient.get(`${CATALOG}/variants/${id}`).then(res => res.data as ServerResult<VariantDetail>)
    if (result.value) result.value = VariantMapper.toDetailModel(result.value as unknown as Record<string, unknown>)
    return result as unknown as ServerResult<VariantDetailModel>
  },

  listByProductId: async (productId: string): Promise<ServerResult<VariantSummaryModel[]>> => {
    const result = await apiClient.get(`${CATALOG}/products/${productId}/variants`).then(res => res.data as ServerResult<VariantSummary[]>)
    if (result.value) result.value = (result.value as unknown as Record<string, unknown>[]).map(VariantMapper.toSummaryModel) as unknown as VariantSummaryModel[]
    return result
  },

  create: async (productId: string, data: CreateVariantRequest): Promise<ServerResult<VariantDetailModel>> => {
    const result = await apiClient.post(`${CATALOG}/products/${productId}/variants`, data).then(res => res.data as ServerResult<VariantDetail>)
    if (result.value) result.value = VariantMapper.toDetailModel(result.value as unknown as Record<string, unknown>)
    return result as unknown as ServerResult<VariantDetailModel>
  },

  update: async (id: string, data: UpdateVariantRequest): Promise<ServerResult<VariantDetailModel>> => {
    const result = await apiClient.put(`${CATALOG}/variants/${id}`, data).then(res => res.data as ServerResult<VariantDetail>)
    if (result.value) result.value = VariantMapper.toDetailModel(result.value as unknown as Record<string, unknown>)
    return result as unknown as ServerResult<VariantDetailModel>
  },

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/variants/${id}`).then(res => res.data as ServerResult<void>),

  syncOptionValues: (variantId: string, optionValueIds: string[]): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/variants/${variantId}/option-values/sync`, { optionValueIds }).then(res => res.data as ServerResult<void>),

  listVariantOptionValues: (variantId: string): Promise<ServerResult<string[]>> =>
    apiClient.get(`${CATALOG}/variants/${variantId}/option-values`).then(res => res.data as ServerResult<string[]>),
}
