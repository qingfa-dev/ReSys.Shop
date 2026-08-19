import { useToast } from 'primevue/usetoast'
import { HttpError } from '@/shared/api'
import type { ApiError } from '@/shared/types/error'

type ResultLike = {
  isSuccess: boolean
  statusCode?: number
  message?: string | null
  errors: Array<ApiError>
}

type FieldErrorSetter = (field: never, message: string) => void

export function useApiErrorHandler() {
  const toast = useToast()

  function handleError(error: unknown): void {
    if (error instanceof HttpError) {
      // Guard: 5xx already toasted by the interceptor — avoid a duplicate toast
      if (error.statusCode >= 500) {
        return
      }
      // Map: HTTP 4xx = warn severity for user-facing toast
      const severity = 'warn'
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

    // Fallback: Non-Error thrown values — display generic message
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'An unexpected error occurred.',
      life: 5000,
    })
  }

  function applyFieldErrors(errors: ApiError[], setFieldError: FieldErrorSetter): ApiError[] {
    // Map: Push each field-scoped error into the matching form field
    const fieldErrors = errors.filter((e): e is ApiError & { field: string } => typeof e.field === 'string' && e.field.length > 0)
    for (const error of fieldErrors) {
      setFieldError(error.field as never, error.message)
    }
    // Return: Only the remaining (field-less) errors for a general error surface
    return errors.filter((e) => typeof e.field !== 'string' || e.field.length === 0)
  }

  function handleResult(result: ResultLike): void {
    // Guard: Only show toast for failed results with error details
    if (!result.isSuccess && result.errors.length > 0) {
      // Map: Status code 5xx = error, 4xx = warn — matches HTTP severity convention
      const severity = (result.statusCode ?? 500) >= 500 ? 'error' : 'warn'
      toast.add({
        severity,
        summary: result.message ?? 'Request failed',
        detail: result.errors.map(e => e.message).join(', '),
        life: 5000,
      })
    }
  }

  return { handleError, handleResult, applyFieldErrors }
}
