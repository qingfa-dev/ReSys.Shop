import apiClient from '@/common/api/http/api.client'
import { CATALOG } from '@/common/api/constants'
import type { ServerPagedResult, ServerResult } from '@/common/api/types/result.types'
import type { PriceRecord } from '../models/price.response'
import type { SetVariantPriceRequest } from '../types/price.request'
import { PriceMapper } from './price.mapper'

export const priceApi = {
  listPrices: async (variantId: string): Promise<ServerPagedResult<PriceRecord>> => {
    const res = await apiClient.get(`${CATALOG}/variants/${variantId}/prices`);
    const result = res.data as ServerPagedResult<PriceRecord>;
    return { ...result, items: result.items?.map(PriceMapper.toPriceRecord) ?? [] };
  },

  setPrice: async (variantId: string, data: SetVariantPriceRequest): Promise<ServerResult<PriceRecord>> => {
    const res = await apiClient.post(`${CATALOG}/variants/${variantId}/prices`, data);
    const result = res.data as ServerResult<PriceRecord>;
    if (result.value) result.value = PriceMapper.toPriceRecord(result.value)
    return result;
  },

  deletePrice: (variantId: string, priceId: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/variants/${variantId}/prices/${priceId}`).then(res => res.data as ServerResult<void>),

  syncPrices: (variantId: string, prices: SetVariantPriceRequest[]): Promise<ServerResult<void>> =>
    apiClient.post(`${CATALOG}/variants/${variantId}/prices/sync`, prices).then(res => res.data as ServerResult<void>),
}
