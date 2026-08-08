import { getPaged } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { PagedResult } from '@/shared/types/result'
import type { Country } from '../types/location'

// Call: Fetch all countries — empty params triggers ToPagedOrAllAsync full catalog.
export function getCountries(): Promise<PagedResult<Country>> {
  return getPaged<Country>(ENDPOINTS.countries, {})
}
