import { defineStore } from 'pinia'
import { ref } from 'vue'
import { checkAvailability } from '../services/availabilityApi'
import type { AvailabilityEntry } from '../types/availability'

const CACHE_TTL_MS = 60_000

export const useAvailabilityStore = defineStore('availability', () => {
  const cache = ref<Record<string, { entry: AvailabilityEntry; fetchedAt: number }>>({})
  const loading = ref(false)
  const pendingIds = ref<Set<string>>(new Set())

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

  async function checkBatch(variantIds: string[]): Promise<void> {
    const uncached = variantIds.filter(
      id => !cache.value[id] || Date.now() - cache.value[id].fetchedAt > CACHE_TTL_MS,
    )
    await Promise.all(uncached.slice(0, 10).map(id => check(id)))
  }

  function invalidate(variantId: string): void {
    delete cache.value[variantId]
  }

  return { cache, loading, pendingIds, check, checkBatch, invalidate }
})
