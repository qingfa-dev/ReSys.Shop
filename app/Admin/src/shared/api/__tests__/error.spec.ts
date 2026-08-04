import { describe, it, expect } from 'vitest'
import axios, { type AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from 'axios'
import { errorInterceptor } from '../interceptors/error'

function axiosError(overrides: Record<string, unknown> = {}) {
  const err = new Error((overrides.message as string) ?? 'Request failed') as AxiosError
  err.isAxiosError = true
  err.config = (overrides.config ?? { headers: {}, url: '/api/products' }) as InternalAxiosRequestConfig
  err.response = overrides.response as AxiosResponse | undefined
  return err
}

describe('errorInterceptor', () => {
  it('passes through canceled errors', async () => {
    await expect(errorInterceptor(new axios.CanceledError('Canceled'))).rejects.toThrow('Canceled')
  })

  it('wraps non-axios errors as HttpError(0)', async () => {
    const err = new Error('Boom')
    await expect(errorInterceptor(err)).rejects.toMatchObject({
      statusCode: 0,
      errors: [{ code: 'Unexpected' }],
    })
  })

  it('extracts errors from response body', async () => {
    const err = axiosError({
      response: {
        status: 422,
        data: { errors: [{ code: 'Validation', message: 'Invalid' }] },
      },
    })

    await expect(errorInterceptor(err)).rejects.toMatchObject({
      statusCode: 422,
      errors: [{ code: 'Validation', message: 'Invalid' }],
    })
  })

  it('extracts title and code from problem details', async () => {
    const err = axiosError({
      response: {
        status: 400,
        data: { title: 'Bad Request', code: 'BadRequest' },
      },
    })

    await expect(errorInterceptor(err)).rejects.toMatchObject({
      statusCode: 400,
      errors: [{ code: 'BadRequest', message: 'Bad Request' }],
    })
  })

  it('handles 401 as a normal error', async () => {
    const err = axiosError({
      config: { headers: {}, url: '/api/products' },
      response: { status: 401, data: null },
    })

    await expect(errorInterceptor(err)).rejects.toMatchObject({
      statusCode: 401,
      errors: [{ code: 'HttpError', message: 'HTTP 401' }],
    })
  })

  it('handles network error without response', async () => {
    const err = axiosError({
      response: null,
      message: 'Network Error',
    })

    await expect(errorInterceptor(err)).rejects.toMatchObject({
      statusCode: 0,
      errors: [{ code: 'HttpError' }],
    })
  })
})
