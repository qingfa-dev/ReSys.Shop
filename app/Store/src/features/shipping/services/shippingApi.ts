import { get } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import type { PagedResult, Result } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import type { ShippingCost, ShippingMethod, ShippingRate } from '../types/shipping'

// Call: Fetch available shipping methods for storefront display.
export function getShippingMethods(params: QueryingParameters = {}): Promise<PagedResult<ShippingMethod>> {
  return getPaged<ShippingMethod>('/api/storefront/shipping/methods', params)
}

// Call: Fetch shipping rates with cost and delivery-range details.
export function getShippingRates(params: QueryingParameters = {}): Promise<PagedResult<ShippingRate>> {
  return getPaged<ShippingRate>('/api/storefront/shipping/rates', params)
}

// Call: Calculate shipping cost for an order via GET with query params.
export function calculateShipping(shippingMethodId: string, orderId: string): Promise<Result<ShippingCost>> {
  return get<Result<ShippingCost>>(`/api/storefront/shipping/calculate?shippingMethodId=${shippingMethodId}&orderId=${orderId}`)
}
