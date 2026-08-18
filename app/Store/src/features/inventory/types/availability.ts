// Types mirror the storefront inventory DTOs exactly (camelCase JSON).
// Contracts pinned from Module.Inventory.Features.Storefront:
// - Reserve:      ReserveCartStock.Request / ReserveCartStock.Response (Result)
// - Release:      ReleaseCartReservation (bare Result — no value payload)
// - Status:       GetCartReservations.Response (PagedResult)
// Guid fields serialize as strings; DateTimeOffset fields as ISO-8601 strings;
// ReservationState serializes as a string (e.g. "Reserved").

// Contract: POST api/storefront/cart/reserve — reserve stock request.
// Matches StockReservationReserveParameters — only VariantId, StockLocationId,
// Quantity, TtlMinutes. OrderId/cartToken are derived server-side from the
// X-Cart-Token header (Storefront.StockReservationReserve.Model.cs).
export interface ReserveStockRequest {
  variantId: string
  stockLocationId: string
  quantity: number
  ttlMinutes?: number
}

// Contract: POST api/storefront/cart/reserve — reservation confirmation response.
// State is always "Reserved" upon successful creation.
export interface CartReservation {
  id: string
  variantId: string
  stockLocationId: string | null
  orderId: string | null
  quantity: number
  state: string
  expiresAtUtc: string
  reason: string | null
  createdAtUtc: string
  modifiedAtUtc: string | null
}

// Contract: GET api/storefront/cart/reserve — active reservation status with TTL countdown.
export interface CartReservationStatus {
  id: string
  variantId: string
  stockLocationId: string | null
  orderId: string | null
  quantity: number
  state: string
  expiresAtUtc: string
  reason: string | null
  createdAtUtc: string
  // ModifiedAtUtc — GetCartReservations.CartReservationStatus (Response.cs:19).
  modifiedAtUtc?: string | null
  remainingSeconds: number
}
