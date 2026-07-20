import { describe, it, expect } from 'vitest'
import {
  ServerResultConstants,
  createServerResult,
  createServerErrorResult,
  createServerPagedResult,
} from '../result.type'

describe('createServerResult', () => {
  it('returns success result for statusCode 200', () => {
    const result = createServerResult(200, 'data')

    expect(result.isSuccess).toBe(true)
    expect(result.statusCode).toBe(200)
    expect(result.value).toBe('data')
    expect(result.errors).toEqual([])
    expect(result.message).toBeNull()
    expect(result.metadata).toBeNull()
  })

  it('returns success result with object value for statusCode 201', () => {
    const result = createServerResult(201, { id: 1 })

    expect(result.isSuccess).toBe(true)
    expect(result.statusCode).toBe(201)
    expect(result.value).toEqual({ id: 1 })
  })

  it('returns failure result for statusCode 400', () => {
    const result = createServerResult(400, null)

    expect(result.isSuccess).toBe(false)
    expect(result.statusCode).toBe(400)
    expect(result.value).toBeNull()
  })

  it('returns failure result for statusCode 500', () => {
    const result = createServerResult(500, 'error')

    expect(result.isSuccess).toBe(false)
    expect(result.statusCode).toBe(500)
  })

  it('sets message when provided', () => {
    const result = createServerResult(200, 'data', 'OK')

    expect(result.message).toBe('OK')
  })

  it('sets metadata when provided without message', () => {
    const result = createServerResult(200, 'data', undefined, { key: 'val' })

    expect(result.metadata).toEqual({ key: 'val' })
  })

  it('returns success when statusCode is 399 (boundary)', () => {
    const result = createServerResult(399, 'data')

    expect(result.isSuccess).toBe(true)
  })
})

describe('createServerErrorResult', () => {
  it('returns error result with validation errors', () => {
    const result = createServerErrorResult(422, [
      { code: 'V', message: 'bad', type: 0, metadata: null },
    ])

    expect(result.isSuccess).toBe(false)
    expect(result.statusCode).toBe(422)
    expect(result.errors).toHaveLength(1)
    expect(result.errors[0]).toEqual({
      code: 'V',
      message: 'bad',
      type: 0,
      metadata: null,
    })
  })

  it('sets message on error result', () => {
    const result = createServerErrorResult(409, [], 'Conflict')

    expect(result.message).toBe('Conflict')
    expect(result.isSuccess).toBe(false)
  })

  it('sets metadata on error result without message', () => {
    const result = createServerErrorResult(500, [], undefined, { trace: 'x' })

    expect(result.metadata).toEqual({ trace: 'x' })
  })
})

describe('createServerPagedResult', () => {
  it('returns paged result with items and pagination', () => {
    const result = createServerPagedResult(200, ['a', 'b'], 1, 2, 100)

    expect(result.isSuccess).toBe(true)
    expect(result.items).toEqual(['a', 'b'])
    expect(result.page).toBe(1)
    expect(result.pageSize).toBe(2)
    expect(result.totalCount).toBe(100)
  })

  it('returns failure paged result for statusCode 500', () => {
    const result = createServerPagedResult(500, [], 1, 10, 0)

    expect(result.isSuccess).toBe(false)
    expect(result.statusCode).toBe(500)
  })

  it('sets message on paged result', () => {
    const result = createServerPagedResult(200, ['x'], 1, 10, 1, 'OK')

    expect(result.message).toBe('OK')
  })

  it('sets metadata on paged result without message', () => {
    const result = createServerPagedResult(
      200,
      [],
      1,
      10,
      0,
      undefined,
      { total: 50 },
    )

    expect(result.metadata).toEqual({ total: 50 })
  })
})

describe('ServerResultConstants', () => {
  it('exposes standard HTTP status codes', () => {
    expect(ServerResultConstants).toEqual({
      Ok: 200,
      Created: 201,
      Accepted: 202,
      NoContent: 204,
    })
  })
})
