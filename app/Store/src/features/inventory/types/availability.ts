// Types mirror the storefront inventory DTOs exactly (camelCase JSON).
// Contracts pinned from Module.Inventory.Features.Storefront:
// - Availability: GetStockAvailability.Response (PagedResult of per-location availability)
// - Reserve:      ReserveCartStock.Request / ReserveCartStock.Response (Result)
// - Release:      ReleaseCartReservation (bare Result — no value payload)
// - Status:       GetCartReservations.Response (PagedResult)
// Guid fields serialize as strings; DateTimeOffset fields as ISO-8601 strings;
// ReservationState serializes as a string (e.g. "Reserved").

// GetStockAvailability.Response — GET api/storefront/availability/{variantId} (paged).
export interface AvailabilityEntry {
  stockLocationId: string
  locationName: string
  countOnHand: number
  reservedCount: number
  availableCount: number
  backorderable: boolean
  available: boolean
}

// ReserveCartStock.Request — POST api/storefront/cart/reserve.
// CartToken is not sent by the client — the backend derives it from the X-Cart-Token header.
export interface ReserveStockRequest {
  variantId: string
  stockLocationId: string
  quantity: number
  orderId?: string | null
  ttlMinutes?: number
  reason?: string | null
}

// ReserveCartStock.Response — POST api/storefront/cart/reserve (Result).
// StockReservationDetailResponse + State always "Reserved".
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

// GetCartReservations.Response — GET api/storefront/cart/reserve (paged).
// StockReservationListItemResponse + RemainingSeconds.
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
  remainingSeconds: number
}
