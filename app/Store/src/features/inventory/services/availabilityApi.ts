import { getPaged } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { PagedResult } from '@/shared/types/result'
import type { AvailabilityEntry } from '../types/availability'

// GET api/storefront/availability/{variantId} — per-location stock availability (paged).
// The optional cartToken query param accounts for the current cart's reserved holds.
export function checkAvailability(
  variantId: string,
  cartToken?: string,
): Promise<PagedResult<AvailabilityEntry>> {
  const baseUrl = cartToken
    ? `${ENDPOINTS.availability(variantId)}?cartToken=${encodeURIComponent(cartToken)}`
    : ENDPOINTS.availability(variantId)
  return getPaged<AvailabilityEntry>(baseUrl, {})
}
