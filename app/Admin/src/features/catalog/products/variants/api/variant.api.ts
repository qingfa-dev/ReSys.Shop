import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { VariantDetail, VariantSummary } from '../types/Variant.Response.Type'
import type { CreateVariantRequest, UpdateVariantRequest } from '../types/Variant.Request.Type'

export const variantRepository = {
  getById: (id: string): Promise<ServerResult<VariantDetail>> =>
    apiClient.get(`${CATALOG}/variants/${id}`).then(res => res.data as ServerResult<VariantDetail>),

  listByProductId: (productId: string): Promise<ServerResult<VariantSummary[]>> =>
    apiClient.get(`${CATALOG}/products/${productId}/variants`).then(res => res.data as ServerResult<VariantSummary[]>),

  create: (productId: string, data: CreateVariantRequest): Promise<ServerResult<VariantDetail>> =>
    apiClient.post(`${CATALOG}/products/${productId}/variants`, data).then(res => res.data as ServerResult<VariantDetail>),

  update: (id: string, data: UpdateVariantRequest): Promise<ServerResult<VariantDetail>> =>
    apiClient.put(`${CATALOG}/variants/${id}`, data).then(res => res.data as ServerResult<VariantDetail>),

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/variants/${id}`).then(res => res.data as ServerResult<void>),

  syncOptionValues: (variantId: string, optionValueIds: string[]): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/variants/${variantId}/option-values/sync`, { optionValueIds }).then(res => res.data as ServerResult<void>),
}
