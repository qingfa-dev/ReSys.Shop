import { describe, it, expect, vi, beforeEach } from 'vitest'

const { isCancelMock, isAxiosErrorMock } = vi.hoisted(() => ({
  isCancelMock: vi.fn<(...args: unknown[]) => unknown>(),
  isAxiosErrorMock: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('axios', () => ({
  default: {
    isCancel: isCancelMock,
    isAxiosError: isAxiosErrorMock,
  },
}))

vi.mock('@/shared/api/notify', () => ({
  notifyError: vi.fn<(...args: unknown[]) => unknown>(),
}))

import { errorInterceptor } from '../error'
import { HttpError } from '../../errors'
import { notifyError } from '@/shared/api/notify'

const mockedNotify = vi.mocked(notifyError)

function axiosLikeError(status: number, data?: Record<string, unknown>) {
  return { response: { status, data } }
}

async function capture<T>(promise: Promise<T>): Promise<T | unknown> {
  try {
    return await promise
  } catch (e) {
    return e
  }
}

describe('errorInterceptor', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    isCancelMock.mockReturnValue(false)
    isAxiosErrorMock.mockReturnValue(true)
  })

  it('notifies a toast on 5xx responses', async () => {
    const caught = await capture(errorInterceptor(axiosLikeError(500, { title: 'Server Error' })))

    expect(caught).toBeInstanceOf(HttpError)
    expect((caught as HttpError).statusCode).toBe(500)
    expect(mockedNotify).toHaveBeenCalledWith('Server Error')
  })

  it('extracts errors from the errors array', async () => {
    const caught = await capture(
      errorInterceptor(
        axiosLikeError(500, { errors: [{ code: 'Server.Boom', message: 'Boom', type: 500 }] }),
      ),
    )

    expect(caught).toBeInstanceOf(HttpError)
    expect((caught as HttpError).statusCode).toBe(500)
    expect((caught as HttpError).errors[0]?.code).toBe('Server.Boom')
    expect((caught as HttpError).errors[0]?.message).toBe('Boom')
    expect(mockedNotify).toHaveBeenCalledWith('Boom')
  })

  it('does not notify on 4xx responses', async () => {
    const caught = await capture(errorInterceptor(axiosLikeError(400, { title: 'Bad Request' })))

    expect(caught).toBeInstanceOf(HttpError)
    expect((caught as HttpError).statusCode).toBe(400)
    expect(mockedNotify).not.toHaveBeenCalled()
  })

  it('rejects canceled requests without notifying', async () => {
    isCancelMock.mockReturnValue(true)
    const err = new Error('canceled')

    const caught = await capture(errorInterceptor(err))

    expect(caught).toBe(err)
    expect(mockedNotify).not.toHaveBeenCalled()
  })

  it('wraps non-axios errors as HttpError(0)', async () => {
    isAxiosErrorMock.mockReturnValue(false)

    const caught = await capture(errorInterceptor(new Error('boom')))

    expect(caught).toBeInstanceOf(HttpError)
    expect((caught as HttpError).statusCode).toBe(0)
    expect(mockedNotify).not.toHaveBeenCalled()
  })

  it('passes through result-shaped responses without notifying', async () => {
    const response = { status: 200, data: { isSuccess: true } }

    const result = await capture(errorInterceptor({ response }))

    expect(result).toBe(response)
    expect(mockedNotify).not.toHaveBeenCalled()
  })
})
