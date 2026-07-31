import { post, get, del } from '@/shared/api/client'
import { CATALOG } from '@/shared/constants/api'
import type { Result } from '@/shared/types'
import type { Price } from '../types/variant'

const BASE = `${CATALOG}/variants`

export interface PriceRequest {
  amount?: number
  currency: string
  compareAtAmount?: number
  countryIso?: string
}

export class VariantPriceApi {
  static listPrices(
    variantId: string,
  ): Promise<Result<{ items: Price[] }>> {
    return get<Result<{ items: Price[] }>>(
      `${BASE}/${variantId}/prices`,
    )
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
