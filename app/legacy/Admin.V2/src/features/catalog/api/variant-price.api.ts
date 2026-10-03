import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { VariantPriceResponse, SyncPricesResponse, VariantPriceRequest, SyncPricesRequest } from '../types'

export class VariantPriceApi {
  static async list(variantId: string): Promise<Result<VariantPriceResponse[]>> {
    const res = await apiClient.get<Result<VariantPriceResponse[]>>(`/catalog/variants/${variantId}/prices`)
    return res.data
  }

  static async set(variantId: string, data: VariantPriceRequest): Promise<Result<VariantPriceResponse>> {
    const res = await apiClient.post<Result<VariantPriceResponse>>(`/catalog/variants/${variantId}/prices`, data)
    return res.data
  }

  static async remove(variantId: string, priceId: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/catalog/variants/${variantId}/prices/${priceId}`)
    return res.data
  }

  static async sync(variantId: string, data: SyncPricesRequest): Promise<Result<SyncPricesResponse>> {
    const res = await apiClient.post<Result<SyncPricesResponse>>(`/catalog/variants/${variantId}/prices/sync`, data)
    return res.data
  }
}
