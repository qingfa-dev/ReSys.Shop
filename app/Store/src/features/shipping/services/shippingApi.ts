import { getPaged } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { PagedResult } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import type { ShippingMethod, ShippingRate } from '../types/shipping'

// GET api/storefront/shipping/methods — paged result of methods available to storefront users.
export function getShippingMethods(params: QueryingParameters = {}): Promise<PagedResult<ShippingMethod>> {
  return getPaged<ShippingMethod>(ENDPOINTS.shippingMethods, params)
}

// GET api/storefront/shipping/rates — paged result of shipping rates with cost/delivery details.
export function getShippingRates(params: QueryingParameters = {}): Promise<PagedResult<ShippingRate>> {
  return getPaged<ShippingRate>(ENDPOINTS.shippingRates, params)
}
