import { ref } from 'vue'
import type { Ref } from 'vue'
import type { PagedResult } from '@/shared/types/result'

export interface UseActiveListOptions {
  immediate?: boolean
}

export interface ActiveListState<T> {
  items: Ref<T[]>
  loaded: Ref<boolean>
  loading: Ref<boolean>
  error: Ref<string | null>
  load: () => Promise<void>
  reset: () => void
}

export function useActiveList<T>(
  fetcher: () => Promise<PagedResult<T>>,
  options?: UseActiveListOptions,
): ActiveListState<T> {
  const items = ref<T[]>([]) as Ref<T[]>
  const loaded = ref(false)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function load(): Promise<void> {
    // Cache: Skip refetch once the options list was loaded for this mount
    if (loaded.value) return
    loading.value = true
    error.value = null
    const result = await fetcher()
    loading.value = false
    if (result.isSuccess) {
      items.value = result.items
      loaded.value = true
    } else {
      error.value = result.message ?? result.errors[0]?.message ?? 'Request failed.'
    }
  }

  function reset() {
    items.value = []
    loaded.value = false
    loading.value = false
    error.value = null
  }

  if (options?.immediate) {
    load()
  }

  return { items, loaded, loading, error, load, reset }
}
