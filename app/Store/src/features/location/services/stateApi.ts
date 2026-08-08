import { getPaged } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { PagedResult } from '@/shared/types/result'
import type { State } from '../types/location'

// Call: Fetch all states — empty params triggers ToPagedOrAllAsync full catalog.
// Each State carries a countryId so callers can cascade-filter states by country.
export function getStates(): Promise<PagedResult<State>> {
  return getPaged<State>(ENDPOINTS.states, {})
}
