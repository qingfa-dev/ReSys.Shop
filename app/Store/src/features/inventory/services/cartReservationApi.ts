import { del, getPaged, post } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { PagedResult, Result } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import type { CartReservation, CartReservationStatus, ReserveStockRequest } from '../types/availability'

// POST api/storefront/cart/reserve — reserve inventory for a cart line.
// The cart token is carried in the X-Cart-Token header (never in the body).
export function reserveStock(req: ReserveStockRequest, cartToken: string): Promise<Result<CartReservation>> {
  return post<Result<CartReservation>>(ENDPOINTS.cartReserve, req, {
    headers: { 'X-Cart-Token': cartToken },
  })
}

// DELETE api/storefront/cart/reserve/{reservationId} — release a reservation (bare Result, no value).
export function releaseReservation(reservationId: string): Promise<Result<null>> {
  return del<Result<null>>(ENDPOINTS.cartReserveById(reservationId))
}

// GET api/storefront/cart/reserve — active reservations for the current cart (paged).
// The cart token is carried in the X-Cart-Token header.
export function getCartReservations(
  cartToken: string,
  params: QueryingParameters = {},
): Promise<PagedResult<CartReservationStatus>> {
  return getPaged<CartReservationStatus>(ENDPOINTS.cartReserveStatus, params, {
    headers: { 'X-Cart-Token': cartToken },
  })
}
