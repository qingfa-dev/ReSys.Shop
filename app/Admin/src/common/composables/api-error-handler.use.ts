import type { ServerResult } from '@/common/api/types/result.types'
import { mapToErrors } from '@/common/api/types/api.types'
import { parseApiError } from '@/common/api/utils/api.utils'
import { useToast } from './toast.use'

export function useApiErrorHandler() {
  const { showToast } = useToast()

  const handleFormErrors = (
    error: unknown,
    setErrors: ((errors: Record<string, string | undefined>) => void) | undefined,
    fieldNames: string[],
    locales?: { errorTitle?: string; genericError?: string },
  ) => {
    if (!error) return
    const apiError = parseApiError(error)
    console.log('[API Trace] Handler received parsed error:', apiError)

    if (apiError.errors && Object.keys(apiError.errors).length > 0) {
      console.log('[API Trace] Validation error dictionary detected.')
      const formErrors: Record<string, string> = {}
      const unmappedMessages: string[] = []

      Object.entries(apiError.errors).forEach(([key, messages]) => {
        const normalizedKey = key.toLowerCase()
        const messagesArray = messages as string[]

        const field = fieldNames.find((f) => {
          const lowerF = f.toLowerCase()
          return normalizedKey === lowerF || normalizedKey.endsWith(`.${lowerF}`)
        })

        if (field && setErrors) {
          formErrors[field] = messagesArray[0] || 'Invalid value'
        } else {
          unmappedMessages.push(...messagesArray)
        }
      })

      if (setErrors) {
        console.log('[API Trace] Mapping errors to fields:', formErrors)
        setErrors(formErrors)
      }

      const isGenericDetail = apiError.detail?.toLowerCase()?.includes('one or more validation errors') ?? false
      const toastDetail =
        (isGenericDetail && unmappedMessages.length > 0)
          ? unmappedMessages.join('. ')
          : (apiError.detail || (unmappedMessages.length > 0 ? unmappedMessages.join('. ') : (locales?.genericError || 'Validation Error')))

      const baseTitle = apiError.title || locales?.errorTitle || 'Error'
      const toastTitle = apiError.errorCode ? `${baseTitle} (${apiError.errorCode})` : baseTitle

      showToast('warn', toastTitle, toastDetail)
    } else {
      const severity = apiError.statusCode && apiError.statusCode < 500 ? 'warn' : 'error'
      const baseTitle = apiError.title || locales?.errorTitle || 'Error'
      const toastTitle = apiError.errorCode ? `${baseTitle} (${apiError.errorCode})` : baseTitle
      const toastDetail = apiError.detail || locales?.genericError || 'An unexpected error occurred.'

      console.log(
        `[API Trace] Showing global toast. Severity: ${severity}, Title: ${toastTitle}, Detail: ${toastDetail}`,
      )

      showToast(severity, toastTitle, toastDetail)
    }
  }

  const handleApiResult = <T>(
    result: ServerResult<T>,
    options?: {
      setErrors?: (errors: Record<string, string | undefined>) => void
      fieldNames?: string[]
      successMessage?: string
      successTitle?: string
      errorTitle?: string
      genericError?: string
    },
  ) => {
    if (result.isSuccess) {
      if (options?.successMessage) {
        showToast('success', options.successTitle || 'Success', options.successMessage)
      }
      return true
    }

    handleFormErrors(
      {
        statusCode: result.statusCode,
        title: result.message,
        message: result.message,
        detail: result.message,
        isSuccess: result.isSuccess,
        errors: mapToErrors(result.errors),
        errorCode: undefined,
      },
      options?.setErrors,
      options?.fieldNames || [],
      { errorTitle: options?.errorTitle, genericError: options?.genericError },
    )
    return false
  }

  return {
    handleFormErrors,
    handleApiResult,
  }
}
