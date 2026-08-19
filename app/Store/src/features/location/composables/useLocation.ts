import { ref, reactive } from 'vue'
import { getCountries } from '../services/countryApi'
import { getStates } from '../services/stateApi'
import type { Country, State } from '../types/location'

// Module-level singleton state
const countries = ref<Country[]>([])
const states = ref<State[]>([])
const selectedCountryId = ref<string | null>(null)
const selectedStateId = ref<string | null>(null)
const loading = ref(false)
const _initialized = ref(false)

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

export function useLocation() {
  return reactive({
    countries,
    states,
    selectedCountryId,
    selectedStateId,
    loading,
    loadAll,
    selectCountry,
  })
}
