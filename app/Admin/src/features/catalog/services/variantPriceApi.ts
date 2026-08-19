import { post, delWithBody } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type { Price, PriceRequest } from '../types/variantPrice'

export class VariantPriceApi {
  static listPrices(variantId: string, params: QueryingParameters = {}): Promise<PagedResult<Price>> {
    return getPaged<Price>(`/api/admin/catalog/variant-prices?variantId=${variantId}`, params)
  }

  static setPrice(
    request: PriceRequest & { variantId: string },
  ): Promise<Result<{ variantId: string }>> {
    return post<Result<{ variantId: string }>>('/api/admin/catalog/variant-prices', request)
  }

  static removePrice(
    variantId: string,
    priceId: string,
  ): Promise<Result<void>> {
    return delWithBody<Result<void>>(`/api/admin/catalog/variant-prices/${priceId}`, { variantId, priceId })
  }

  static syncPrices(request: {
    variantId: string
    prices: PriceRequest[]
  }): Promise<Result<void>> {
    return post<Result<void>>('/api/admin/catalog/variant-prices/sync', request)
  }
}
