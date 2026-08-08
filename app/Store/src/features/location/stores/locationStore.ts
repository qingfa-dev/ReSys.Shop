import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { getCountries } from '../services/countryApi'
import { getStates } from '../services/stateApi'
import type { Country, State } from '../types/location'

export const useLocationStore = defineStore('location', () => {
  const countries = ref<Country[]>([])
  const states = ref<State[]>([])
  const selectedCountryId = ref<string | null>(null)
  const selectedStateId = ref<string | null>(null)
  const loading = ref(false)
  const _initialized = ref(false)

  // Compute: Filter states belonging to the selected country (client-side cascade).
  const filteredStates = computed(() =>
    states.value.filter(s => s.countryId === selectedCountryId.value),
  )

  // Compute: Determine if the selected country mandates a state selection at checkout.
  const statesRequired = computed(() =>
    countries.value.find(c => c.id === selectedCountryId.value)?.statesRequired ?? false,
  )

  // Fetch: Load countries and states in parallel on first access only.
  async function loadAll(): Promise<void> {
    if (_initialized.value) return
    _initialized.value = true
    loading.value = true
    const [c, s] = await Promise.all([getCountries(), getStates()])
    if (c.isSuccess) countries.value = c.items
    if (s.isSuccess) states.value = s.items
    loading.value = false
  }

  // Reset: Clear selected state when country changes to avoid orphaned reference.
  function selectCountry(id: string): void {
    selectedCountryId.value = id
    selectedStateId.value = null
  }

  return {
    countries, states, selectedCountryId, selectedStateId, loading,
    filteredStates, statesRequired, loadAll, selectCountry,
  }
})
