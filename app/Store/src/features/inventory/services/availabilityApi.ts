import { getPaged } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { PagedResult } from '@/shared/types/result'
import type { AvailabilityEntry } from '../types/availability'

// Call: Fetch per-location stock availability — optional cartToken accounts for reserved holds.
export function checkAvailability(
  variantId: string,
  cartToken?: string,
): Promise<PagedResult<AvailabilityEntry>> {
  const baseUrl = cartToken
    ? `${ENDPOINTS.availability(variantId)}?cartToken=${encodeURIComponent(cartToken)}`
    : ENDPOINTS.availability(variantId)
  return getPaged<AvailabilityEntry>(baseUrl, {})
}
