import { getPaged } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { PagedResult } from '@/shared/types/result'
import type { Country } from '../types/location'

// GET api/store/locations/countries — all countries.
// The list endpoint returns a PagedResult envelope. Passing no paging params makes the
// backend return every row (ToPagedOrAllAsync), so this service yields the full catalog.
export function getCountries(): Promise<PagedResult<Country>> {
  return getPaged<Country>(ENDPOINTS.countries, {})
}
