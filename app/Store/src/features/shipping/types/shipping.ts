// Types mirror the storefront shipping DTOs exactly (camelCase JSON).
// Contracts pinned from Module.Shipping.Features.Storefront.Shipping:
// - Methods:   GetShippingMethods.Response (PagedResult)
// - Calculate: CalculateShipping.Request / CalculateShipping.Response (Result)
// - Rates:     ListShippingRates.Response (PagedResult)
// Decimal fields (cost, finalPrice, weights, thresholds) serialize as JSON numbers.

// GetShippingMethods.Response — GET api/storefront/shipping/methods (paged).
export interface ShippingMethod {
  id: string
  name: string
  adminName: string | null
  code: string | null
  calculatorType: string
  position: number
}

// CalculateShipping.Request — POST api/storefront/shipping/calculate.
// The backend computes the cost from the order's line-item weights; both ids are required.
export interface CalculateShippingRequest {
  orderId: string
  shippingMethodId: string
}

// CalculateShipping.Response — POST api/storefront/shipping/calculate (Result).
export interface ShippingCalculation {
  shippingMethodId: string
  methodName: string
  cost: number
  currency: string
  isFreeShipping: boolean
}

// ListShippingRates.Response — GET api/storefront/shipping/rates (paged).
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
