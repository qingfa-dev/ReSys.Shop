// Types mirror the storefront shipping DTOs exactly (camelCase JSON).
// Contracts pinned from Module.Shipping.Features.Storefront.Shipping:
// - Methods:   GetShippingMethods.Response (PagedResult)
// - Calculate: CalculateShipping.Request / CalculateShipping.Response (Result)
// - Rates:     ListShippingRates.Response (PagedResult)
// Decimal fields (cost, finalPrice, weights, thresholds) serialize as JSON numbers.

// Contract: GET api/storefront/shipping/methods — available shipping method.
export interface ShippingMethod {
  id: string
  name: string
  adminName: string | null
  code: string | null
  calculatorType: string
  position: number
}

// Contract: GET api/storefront/shipping/calculate — cost computation request.
// Backend computes cost from order line-item weights; both IDs are query params.
export interface CalculateShippingRequest {
  orderId: string
  shippingMethodId: string
}

// Contract: GET api/storefront/shipping/calculate — cost computation response.
export interface ShippingCost {
  shippingMethodId: string
  methodName: string
  cost: number
  currency: string
  isFreeShipping: boolean
}

export interface ShippingCalculation {
  shippingMethodId: string
  methodName: string
  cost: number
  currency: string
  isFreeShipping: boolean
}

// Contract: GET api/storefront/shipping/rates — rate with weight thresholds.
export interface ShippingRate {
  id: string
  shippingMethodId: string
  name: string
  cost: number
  finalPrice: number
  deliveryRange: string | null
  minWeight: number | null
  maxWeight: number | null
  freeShippingThreshold: number | null
}
