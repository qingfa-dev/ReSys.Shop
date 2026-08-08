import { ref, watch } from 'vue'
import { getCountries } from '../services/countryApi'
import { getStates } from '../services/stateApi'
import type { Country, State } from '../types/location'

// Cascading country → state selector state used by address forms.
// Both lists are fetched once (no paging params → backend returns ALL rows). The states
// offered for the selected country are derived client-side via each State.countryId, so
// country changes re-filter the cached catalog instead of issuing another request.
export function useLocationCascade() {
  const countries = ref<Country[]>([])
  const states = ref<State[]>([])
  const selectedCountryId = ref<string | null>(null)
  const selectedStateId = ref<string | null>(null)
  const loading = ref(false)

  // Cache: Full state catalog so country changes only re-filter, never re-fetch.
  const allStates = ref<State[]>([])

  // Filter: Derive states for selected country; clear selection if orphaned.
  function applyCountryFilter(countryId: string | null): void {
    if (!countryId) {
      states.value = []
      selectedStateId.value = null
      return
    }
    states.value = allStates.value.filter((state) => state.countryId === countryId)
    // Drop the selected state when it no longer belongs to the selected country.
    if (selectedStateId.value && !states.value.some((state) => state.id === selectedStateId.value)) {
      selectedStateId.value = null
    }
  }

  // Fetch: Load countries and states in parallel, then apply initial filter.
  async function loadCountries(): Promise<void> {
    loading.value = true
    try {
      const [countryResult, stateResult] = await Promise.all([getCountries(), getStates()])
      if (countryResult.isSuccess) countries.value = countryResult.items
      if (stateResult.isSuccess) {
        allStates.value = stateResult.items
        // Re-apply in case a country was selected before the lists finished loading.
        applyCountryFilter(selectedCountryId.value)
      }
    } finally {
      loading.value = false
    }
  }

  // Subscribe: Re-filter states whenever the selected country changes.
  watch(selectedCountryId, (countryId) => {
    applyCountryFilter(countryId)
  })

  return { countries, states, selectedCountryId, selectedStateId, loading, loadCountries }
}
