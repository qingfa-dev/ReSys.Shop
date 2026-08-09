import { getPaged } from '@/shared/api'
import type { PagedResult } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import type { ShippingMethod, ShippingRate } from '../types/shipping'

// Call: Fetch available shipping methods for storefront display.
export function getShippingMethods(params: QueryingParameters = {}): Promise<PagedResult<ShippingMethod>> {
  return getPaged<ShippingMethod>('/api/storefront/shipping/methods', params)
}

// Call: Fetch shipping rates with cost and delivery-range details.
export function getShippingRates(params: QueryingParameters = {}): Promise<PagedResult<ShippingRate>> {
  return getPaged<ShippingRate>('/api/storefront/shipping/rates', params)
}
