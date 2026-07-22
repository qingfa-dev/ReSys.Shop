import { ref } from 'vue'
import type { Result, Error } from '@/shared/models'

export function useApi<T>() {
  const data = ref<T | null>(null)
  const loading = ref(false)
  const error = ref<Error | null>(null)

  async function execute(apiCall: () => Promise<Result<T>>): Promise<Result<T>> {
    loading.value = true
    error.value = null
    try {
      const result = await apiCall()
      if (result.isSuccess) {
        data.value = result.value
      } else if (result.errors.length > 0) {
        error.value = result.errors[0] ?? null
      }
      return result
    } catch (e) {
      error.value = {
        code: 'UNEXPECTED',
        message: e instanceof Error ? e.message : 'An unexpected error occurred',
        type: 500,
        metadata: null,
      }
      return {
        isSuccess: false,
        statusCode: 500,
        errors: [error.value],
        message: error.value.message,
        metadata: null,
        value: null as T,
      }
    } finally {
      loading.value = false
    }
  }

  return { data, loading, error, execute }
}
