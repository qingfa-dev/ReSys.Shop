import { ref, watch, type Ref } from 'vue'
import { api } from '@/shared/api/client'
import type { PagedResult } from '@/shared/api/paged-result'
import { QueryBuilder } from '@/shared/query'

export function useList<TItem>(
  baseUrl: string,
  builder: Ref<QueryBuilder>,
) {
  const data = ref<TItem[]>([])
  const total = ref(0)
  const isLoading = ref(false)
  const error = ref<Error | null>(null)

  async function load(): Promise<void> {
    isLoading.value = true
    error.value = null
    try {
      const { toUrl } = builder.value.build()
      const result = await api.getPaged<TItem>(toUrl(baseUrl))
      data.value = result.items
      total.value = result.totalCount
    } catch (e) {
      error.value = e instanceof Error ? e : new Error('Unknown error')
      data.value = []
      total.value = 0
    } finally {
      isLoading.value = false
    }
  }

  watch(builder, load, { immediate: true })

  return { data, total, isLoading, error, refetch: load }
}
