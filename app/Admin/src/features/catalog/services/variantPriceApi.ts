import { post, get, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type { Price } from '../types/variant'

const BASE = `${CATALOG}/variants`

export interface PriceRequest {
  amount?: number
  currency: string
  compareAtAmount?: number
  countryIso?: string
}

export class VariantPriceApi {
  static listPrices(variantId: string): Promise<PagedResult<Price>> {
    return getPaged<Price>(`${BASE}/${variantId}/prices`, {
      pageNumber: 1,
      pageSize: 100,
    })
  }

  static setPrice(
    variantId: string,
    request: PriceRequest,
  ): Promise<Result<{ variantId: string }>> {
    return post<Result<{ variantId: string }>>(
      `${BASE}/${variantId}/prices`,
      request,
    )
  }

  static removePrice(
    variantId: string,
    priceId: string,
  ): Promise<Result<void>> {
    return del<Result<void>>(`${BASE}/${variantId}/prices/${priceId}`)
  }
}
