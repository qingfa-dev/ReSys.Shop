import { del, getPaged, post } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { PagedResult, Result } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import type { CartReservation, CartReservationStatus, ReserveStockRequest } from '../types/availability'

// Call: Reserve inventory for a cart line — cart token carried in X-Cart-Token header.
export function reserveStock(req: ReserveStockRequest, cartToken: string): Promise<Result<CartReservation>> {
  return post<Result<CartReservation>>(ENDPOINTS.cartReserve, req, {
    headers: { 'X-Cart-Token': cartToken },
  })
}

// Call: Release a reservation — returns bare Result with no value payload.
export function releaseReservation(reservationId: string): Promise<Result<null>> {
  return del<Result<null>>(ENDPOINTS.cartReserveById(reservationId))
}

// Call: Fetch active reservations for the current cart (paged) — uses X-Cart-Token header.
export function getCartReservations(
  cartToken: string,
  params: QueryingParameters = {},
): Promise<PagedResult<CartReservationStatus>> {
  return getPaged<CartReservationStatus>(ENDPOINTS.cartReserveStatus, params, {
    headers: { 'X-Cart-Token': cartToken },
  })
}
