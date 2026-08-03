import { useActiveList } from '@/shared/composables'
import type { CountryListItem } from '../types/country'
import { CountryApi } from '../services/countryApi'

export function useActiveCountries() {
  // Call: Location service — active countries for form Select options
  return useActiveList<CountryListItem>(() => CountryApi.getCountries({ isActive: true }))
}
