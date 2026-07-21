import apiClient from '@/common/api/http/api.client'
import { CATALOG } from '@/common/api/constants'
import type { ServerPagedResult, ServerResult } from '@/common/api/types/result.types'
import type { PriceRecord } from '../models/price.response'
import type { SetVariantPriceRequest } from '../types/price.request'

export const priceApi = {
  listPrices: (variantId: string): Promise<ServerPagedResult<PriceRecord>> =>
    apiClient.get(`${CATALOG}/variants/${variantId}/prices`).then(res => res.data as ServerPagedResult<PriceRecord>),

  setPrice: (variantId: string, data: SetVariantPriceRequest): Promise<ServerResult<PriceRecord>> =>
    apiClient.post(`${CATALOG}/variants/${variantId}/prices`, data).then(res => res.data as ServerResult<PriceRecord>),

  deletePrice: (variantId: string, priceId: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/variants/${variantId}/prices/${priceId}`).then(res => res.data as ServerResult<void>),

  syncPrices: (variantId: string, prices: SetVariantPriceRequest[]): Promise<ServerResult<void>> =>
    apiClient.post(`${CATALOG}/variants/${variantId}/prices/sync`, prices).then(res => res.data as ServerResult<void>),
}
