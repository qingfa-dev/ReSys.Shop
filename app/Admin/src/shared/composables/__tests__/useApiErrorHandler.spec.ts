import { describe, it, expect, vi, beforeEach } from 'vitest'
import { HttpError } from '@/shared/api'

const mockToastAdd = vi.fn()

vi.mock('primevue/usetoast', () => ({
  useToast: vi.fn(() => ({ add: mockToastAdd })),
}))

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useApiErrorHandler', () => {
  it('shows toast for HttpError with 4xx severity warn', async () => {
    const { useApiErrorHandler } = await import('../useApiErrorHandler')
    const { handleError } = useApiErrorHandler()

    handleError(new HttpError(400, [{ code: 'BadRequest', message: 'Invalid input', type: 400 }]))

    expect(mockToastAdd).toHaveBeenCalledWith(
      expect.objectContaining({
        severity: 'warn',
        summary: 'Invalid input',
        detail: 'Invalid input',
        life: 5000,
      }),
    )
  })

  it('shows toast for HttpError with 5xx severity error', async () => {
    const { useApiErrorHandler } = await import('../useApiErrorHandler')
    const { handleError } = useApiErrorHandler()

    handleError(new HttpError(500, [{ code: 'ServerError', message: 'Internal error', type: 500 }]))

    expect(mockToastAdd).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'error' }),
    )
  })

  it('shows error toast for generic Error', async () => {
    const { useApiErrorHandler } = await import('../useApiErrorHandler')
    const { handleError } = useApiErrorHandler()

    handleError(new Error('Something broke'))

    expect(mockToastAdd).toHaveBeenCalledWith(
      expect.objectContaining({
        severity: 'error',
        summary: 'Unexpected Error',
        detail: 'Something broke',
      }),
    )
  })

  it('shows error toast for unknown error', async () => {
    const { useApiErrorHandler } = await import('../useApiErrorHandler')
    const { handleError } = useApiErrorHandler()

    handleError('just a string')

    expect(mockToastAdd).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'error', summary: 'Error' }),
    )
  })

  it('handleResult shows toast on failure', async () => {
    const { useApiErrorHandler } = await import('../useApiErrorHandler')
    const { handleResult } = useApiErrorHandler()

    handleResult({
      isSuccess: false,
      statusCode: 422,
      message: 'Validation failed',
      errors: [{ code: 'Required', message: 'Name is required' }],
    })

    expect(mockToastAdd).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'warn' }),
    )
  })

  it('handleResult does nothing on success', async () => {
    const { useApiErrorHandler } = await import('../useApiErrorHandler')
    const { handleResult } = useApiErrorHandler()

    handleResult({
      isSuccess: true,
      errors: [],
    })

    expect(mockToastAdd).not.toHaveBeenCalled()
  })
})
