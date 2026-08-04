import { ref, type Ref } from 'vue'
import { ImageEmbeddingApi } from '../services/imageEmbeddingApi'
import type { EmbeddingDetailResponse } from '../types/imageEmbedding'

const ACTIVE_POLLS = new Map<string, ReturnType<typeof setTimeout>>()

export function useEmbeddingStatus(variantImageId: Ref<string | null>) {
  const embedding = ref<EmbeddingDetailResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function refresh(): Promise<void> {
    if (!variantImageId.value) return
    loading.value = true
    error.value = null
    try {
      const result = await ImageEmbeddingApi.get(variantImageId.value)
      if (result.isSuccess) {
        embedding.value = result.value
        loading.value = false
      } else {
        embedding.value = null
        loading.value = false
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to load embedding'
      embedding.value = null
      loading.value = false
    }
  }

  async function poll(maxAttempts = 20, intervalMs = 1500): Promise<void> {
    const key = variantImageId.value
    if (!key) return

    if (ACTIVE_POLLS.has(key)) {
      clearTimeout(ACTIVE_POLLS.get(key)!)
      ACTIVE_POLLS.delete(key)
    }

    for (let attempt = 0; attempt < maxAttempts; attempt++) {
      await refresh()

      if (embedding.value) {
        const status = embedding.value.status
        if (status === 'Completed' || status === 'Failed') break
      } else {
        break
      }

      if (attempt < maxAttempts - 1) {
        await new Promise<void>((resolve) => {
          const timer = setTimeout(() => {
            ACTIVE_POLLS.delete(key)
            resolve()
          }, intervalMs)
          ACTIVE_POLLS.set(key, timer)
        })
      }
    }

    if (embedding.value && embedding.value.status === 'Pending') {
      error.value = 'Embedding timed out after 30 seconds'
    }
  }

  return { embedding, loading, error, poll, refresh }
}
