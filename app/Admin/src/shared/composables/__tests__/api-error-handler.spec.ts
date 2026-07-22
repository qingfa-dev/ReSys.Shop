import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useApiErrorHandler } from '../useApiErrorHandler'
import type { Result } from '@/shared/models'

const mockShowToast = vi.fn<() => void>()
vi.mock('../useToast', () => ({
  useToast: () => ({
    showToast: mockShowToast,
  }),
}))

describe('useApiErrorHandler - Edge Cases', () => {
  beforeEach(() => {
    mockShowToast.mockClear()
  })

  it('should handle undefined error gracefully', () => {
    const { handleFormErrors } = useApiErrorHandler()
    const setErrors = vi.fn<() => void>()
    handleFormErrors(undefined, setErrors, [])
    expect(setErrors).not.toHaveBeenCalled()
  })

  it('should include errorCode in the toast title', () => {
    const { handleFormErrors } = useApiErrorHandler()
    const apiError = {
      statusCode: 409,
      title: 'Conflict',
      detail: 'Already exists',
      errorCode: 'DuplicateName',
    }

    handleFormErrors(apiError, undefined, [])
    expect(mockShowToast).toHaveBeenCalledWith('warn', 'Conflict (DuplicateName)', 'Already exists')
  })

  it('should prioritize apiError.detail over genericError locale', () => {
    const { handleFormErrors } = useApiErrorHandler()
    const apiError = {
      statusCode: 500,
      title: 'Error',
      detail: 'Specific server error message',
    }

    handleFormErrors(apiError, undefined, [], { genericError: 'Generic Locale Error' })
    expect(mockShowToast).toHaveBeenCalledWith('error', 'Error', 'Specific server error message')
  })

  it('should prioritize apiError.title over errorTitle locale', () => {
    const { handleFormErrors } = useApiErrorHandler()
    const apiError = {
      statusCode: 409,
      title: 'Conflict',
      detail: 'Detail',
    }

    handleFormErrors(apiError, undefined, [], { errorTitle: 'Generic Error' })
    expect(mockShowToast).toHaveBeenCalledWith('warn', 'Conflict', 'Detail')
  })

  it('should toast for unmapped validation errors', () => {
    const { handleFormErrors } = useApiErrorHandler()
    const setErrors = vi.fn<() => void>()

    const apiError = {
      statusCode: 400,
      title: 'Validation Error',
      errors: { 'request.secret_field': ['Internal error'] },
    }

    handleFormErrors(apiError, setErrors, ['name', 'email'])

    expect(setErrors).toHaveBeenCalledWith({})
    expect(mockShowToast).toHaveBeenCalledWith('warn', 'Validation Error', 'Internal error')
  })

  it('should prioritize apiError over custom locales', () => {
    const { handleApiResult } = useApiErrorHandler()
    const result: Result<unknown> = {
      isSuccess: false,
      statusCode: 500,
      errors: [{ code: 'ServerError', message: 'Server Error', type: 4, metadata: null }],
      message: 'Server Error',
      metadata: null,
      value: null,
    }

    handleApiResult(result, {
      errorTitle: 'Custom Title',
      genericError: 'Custom Detail',
    })

    expect(mockShowToast).toHaveBeenCalledWith('warn', 'Server Error', 'Server Error')
  })

  it('should use custom locales as fallback when apiError fields are missing', () => {
    const { handleApiResult } = useApiErrorHandler()
    const result: Result<unknown> = {
      isSuccess: false,
      statusCode: 500,
      errors: [],
      message: null,
      metadata: null,
      value: null,
    }

    handleApiResult(result, {
      errorTitle: 'Fallback Title',
      genericError: 'Fallback Detail',
    })

    expect(mockShowToast).toHaveBeenCalledWith('error', 'Fallback Title', 'Fallback Detail')
  })

  it('should handle multiple messages for the same field by taking the first one', () => {
    const { handleFormErrors } = useApiErrorHandler()
    const setErrors = vi.fn<() => void>()

    const apiError = {
      statusCode: 400,
      errors: { name: ['Too short', 'No numbers'] },
    }

    handleFormErrors(apiError, setErrors, ['name'])
    expect(setErrors).toHaveBeenCalledWith({ name: 'Too short' })
  })

  it('should handle handleApiResult without options', () => {
    const { handleApiResult } = useApiErrorHandler()
    const result: Result<unknown> = { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: {} }

    expect(handleApiResult(result)).toBe(true)
    expect(mockShowToast).not.toHaveBeenCalled()
  })

  it('should normalize nested field names correctly', () => {
    const { handleFormErrors } = useApiErrorHandler()
    const setErrors = vi.fn<() => void>()

    const apiError = {
      statusCode: 400,
      errors: { 'order.customer.first_name': ['Required'] },
    }

    handleFormErrors(apiError, setErrors, ['first_name'])
    expect(setErrors).toHaveBeenCalledWith({ first_name: 'Required' })
  })
})
