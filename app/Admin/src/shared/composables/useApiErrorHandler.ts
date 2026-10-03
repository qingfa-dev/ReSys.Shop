import { useToast } from 'primevue/usetoast'
import { HttpError } from '@/shared/api'

type ResultLike = {
  isSuccess: boolean
  statusCode?: number
  message?: string | null
  errors: Array<{ code: string; message: string }>
}

export function useApiErrorHandler() {
  const toast = useToast()

  function handleError(error: unknown): void {
    if (error instanceof HttpError) {
      const severity = error.statusCode >= 500 ? 'error' : 'warn'
      toast.add({
        severity,
        summary: error.message,
        detail: error.errors.map(e => e.message).join(', '),
        life: 5000,
      })
      return
    }

    if (error instanceof Error) {
      toast.add({
        severity: 'error',
        summary: 'Unexpected Error',
        detail: error.message,
        life: 5000,
      })
      return
    }

    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'An unexpected error occurred.',
      life: 5000,
    })
  }

  function handleResult(result: ResultLike): void {
    if (!result.isSuccess && result.errors.length > 0) {
      const severity = (result.statusCode ?? 500) >= 500 ? 'error' : 'warn'
      toast.add({
        severity,
        summary: result.message ?? 'Request failed',
        detail: result.errors.map(e => e.message).join(', '),
        life: 5000,
      })
    }
  }

  return { handleError, handleResult }
}
