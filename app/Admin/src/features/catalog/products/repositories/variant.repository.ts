import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { VariantDetail, VariantSummary } from '../types/Variant.Response.Type'
import type { CreateVariantRequest, UpdateVariantRequest } from '../types/Variant.Request.Type'

interface PriceRecord {
  id: string
  amount: number
  currency: string
}

interface VariantImage {
  id: string
  variantId: string
  url: string
  alt: string | null
  position: number
  role: number
  fileSize: number | null
  isDefault: boolean
}

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

  listPrices: (variantId: string): Promise<ServerResult<PriceRecord[]>> =>
    apiClient.get(`${CATALOG}/variants/${variantId}/prices`).then(res => res.data as ServerResult<PriceRecord[]>),

  setPrice: (variantId: string, data: { amount: number; currency: string }): Promise<ServerResult<PriceRecord>> =>
    apiClient.post(`${CATALOG}/variants/${variantId}/prices`, data).then(res => res.data as ServerResult<PriceRecord>),

  deletePrice: (variantId: string, priceId: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/variants/${variantId}/prices/${priceId}`).then(res => res.data as ServerResult<void>),

  syncPrices: (variantId: string, prices: Array<{ amount: number; currency: string }>): Promise<ServerResult<void>> =>
    apiClient.post(`${CATALOG}/variants/${variantId}/prices/sync`, prices).then(res => res.data as ServerResult<void>),

  syncOptionValues: (variantId: string, optionValueIds: string[]): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/variants/${variantId}/option-values/sync`, { optionValueIds }).then(res => res.data as ServerResult<void>),

  listImages: (variantId: string): Promise<ServerResult<VariantImage[]>> =>
    apiClient.get(`${CATALOG}/variants/${variantId}/images`).then(res => res.data as ServerResult<VariantImage[]>),

  uploadImage: (variantId: string, file: File, role?: number): Promise<ServerResult<VariantImage>> => {
    const formData = new FormData()
    formData.append('file', file)
    let url = `${CATALOG}/variants/${variantId}/images`
    if (role !== undefined) url += `?role=${role}`
    return apiClient.post(url, formData, { headers: { 'Content-Type': 'multipart/form-data' } }).then(res => res.data as ServerResult<VariantImage>)
  },

  deleteImage: (imageId: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/variants/images/${imageId}`).then(res => res.data as ServerResult<void>),

  updateImage: (imageId: string, data: { alt?: string; role?: number }): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/variants/images/${imageId}`, data).then(res => res.data as ServerResult<void>),
}
