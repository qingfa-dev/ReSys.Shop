import { getPaged } from '@/shared/api'
import type { PagedResult } from '@/shared/types/result'
import type { AvailabilityEntry } from '../types/availability'

// Call: Fetch per-location stock availability — optional cartToken accounts for reserved holds.
export function checkAvailability(
  variantId: string,
  cartToken?: string,
): Promise<PagedResult<AvailabilityEntry>> {
  const baseUrl = cartToken
    ? `/api/storefront/inventory/stock-items/${variantId}?cartToken=${encodeURIComponent(cartToken)}`
    : `/api/storefront/inventory/stock-items/${variantId}`
  return getPaged<AvailabilityEntry>(baseUrl, {})
}
