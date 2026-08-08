import { getPaged } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { PagedResult } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import type { ShippingMethod, ShippingRate } from '../types/shipping'

// Call: Fetch available shipping methods for storefront display.
export function getShippingMethods(params: QueryingParameters = {}): Promise<PagedResult<ShippingMethod>> {
  return getPaged<ShippingMethod>(ENDPOINTS.shippingMethods, params)
}

// Call: Fetch shipping rates with cost and delivery-range details.
export function getShippingRates(params: QueryingParameters = {}): Promise<PagedResult<ShippingRate>> {
  return getPaged<ShippingRate>(ENDPOINTS.shippingRates, params)
}
