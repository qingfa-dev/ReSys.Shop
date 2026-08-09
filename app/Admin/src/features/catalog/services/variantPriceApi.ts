import { post, delWithBody } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult } from '@/shared/types'
import type { Price, PriceRequest } from '../types/variantPrice'

const BASE = 'api/admin/catalog/variant-prices'

export class VariantPriceApi {
  static listPrices(variantId: string): Promise<PagedResult<Price>> {
    return getPaged<Price>(`${BASE}?variantId=${variantId}`, {
      pageNumber: 1,
      pageSize: 100,
    })
  }

  static setPrice(
    request: PriceRequest & { variantId: string },
  ): Promise<Result<{ variantId: string }>> {
    return post<Result<{ variantId: string }>>(BASE, request)
  }

  static removePrice(
    variantId: string,
    priceId: string,
  ): Promise<Result<void>> {
    return delWithBody<Result<void>>(`${BASE}/${priceId}`, { variantId, priceId })
  }

  static syncPrices(request: {
    variantId: string
    prices: PriceRequest[]
  }): Promise<Result<void>> {
    return post<Result<void>>(`${BASE}/sync`, request)
  }
}
