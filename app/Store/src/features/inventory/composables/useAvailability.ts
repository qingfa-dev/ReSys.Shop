import { ref, reactive } from 'vue'
import { checkAvailability } from '../services/availabilityApi'
import type { AvailabilityEntry } from '../types/availability'

// Cache: 60 s TTL prevents stale stock reads while avoiding excessive API calls.
const CACHE_TTL_MS = 60_000

// Module-level singleton state
const cache = ref<Record<string, { entry: AvailabilityEntry; fetchedAt: number }>>({})
const loading = ref(false)
const pendingIds = ref<Set<string>>(new Set())

// Fetch: Check availability for a single variant, returning cached data when fresh.
async function check(variantId: string): Promise<AvailabilityEntry | null> {
  const cached = cache.value[variantId]
  if (cached && Date.now() - cached.fetchedAt < CACHE_TTL_MS) return cached.entry
  if (pendingIds.value.has(variantId)) return null
  pendingIds.value.add(variantId)
  loading.value = true
  const result = await checkAvailability(variantId)
  pendingIds.value.delete(variantId)
  loading.value = false
  if (result.isSuccess && result.items.length > 0) {
    const entry = result.items[0]
    if (entry) {
      cache.value[variantId] = { entry, fetchedAt: Date.now() }
      return entry
    }
  }
  return null
}

// Batch: Pre-warm cache for up to 10 uncached variant IDs in parallel.
async function checkBatch(variantIds: string[]): Promise<void> {
  const uncached = variantIds.filter(
    id => !cache.value[id] || Date.now() - cache.value[id].fetchedAt > CACHE_TTL_MS,
  )
  await Promise.all(uncached.slice(0, 10).map(id => check(id)))
}

// Purge: Remove a single variant from cache (e.g. after stock mutation).
function invalidate(variantId: string): void {
  delete cache.value[variantId]
}

export function useAvailability() {
  return reactive({
    cache,
    loading,
    pendingIds,
    check,
    checkBatch,
    invalidate,
  })
}
