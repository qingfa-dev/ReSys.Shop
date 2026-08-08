import { describe, it, expect } from 'vitest'
import { ok, created, noContent, failure, badRequest, notFound, unauthorized, pagedOk, pagedFailure, isSuccess, isFailure } from '@/shared/types/result'
import { StatusCode } from '@/shared/types/error'
import type { ApiError } from '@/shared/types/error'

describe('Result factories', () => {
  it('ok() returns success Result with value', () => {
    const r = ok({ id: '1', name: 'Test' })
    expect(r.isSuccess).toBe(true)
    expect(r.statusCode).toBe(StatusCode.Ok)
    expect(r.value).toEqual({ id: '1', name: 'Test' })
    expect(r.errors).toEqual([])
    expect(isSuccess(r)).toBe(true)
    expect(isFailure(r)).toBe(false)
  })

  it('created() returns 201 with value', () => {
    const r = created({ id: '2' })
    expect(r.isSuccess).toBe(true)
    expect(r.statusCode).toBe(StatusCode.Created)
  })

  it('noContent() returns 204 with null value', () => {
    const r = noContent()
    expect(r.isSuccess).toBe(true)
    expect(r.statusCode).toBe(StatusCode.NoContent)
    expect(r.value).toBeNull()
  })

  it('failure() returns error Result', () => {
    const apiError: ApiError = { code: 'Test.Error', message: 'Something went wrong', type: 500 }
    const r = failure(apiError)
    expect(r.isSuccess).toBe(false)
    expect(r.errors[0]?.code).toBe('Test.Error')
    expect(r.value).toBeNull()
    expect(isFailure(r)).toBe(true)
  })

  it('badRequest() returns 400 with message', () => {
    const r = badRequest('Missing field')
    expect(r.isSuccess).toBe(false)
    expect(r.statusCode).toBe(400)
    expect(r.message).toBe('Missing field')
  })

  it('notFound() returns 404', () => {
    const r = notFound('Product not found')
    expect(r.isSuccess).toBe(false)
    expect(r.statusCode).toBe(404)
  })

  it('unauthorized() returns 401', () => {
    const r = unauthorized()
    expect(r.statusCode).toBe(401)
  })
})

describe('PagedResult factories', () => {
  it('pagedOk() returns paged result with items', () => {
    const items = [{ id: '1' }, { id: '2' }]
    const r = pagedOk(items, 1, 20, 42)
    expect(r.isSuccess).toBe(true)
    expect(r.items).toEqual(items)
    expect(r.page).toBe(1)
    expect(r.pageSize).toBe(20)
    expect(r.totalCount).toBe(42)
    expect(r.totalPages).toBe(3)
  })

  it('pagedOk() with zero pageSize returns 0 totalPages', () => {
    const r = pagedOk([], 1, 0, 10)
    expect(r.totalPages).toBe(0)
  })

  it('pagedFailure() returns error with empty items', () => {
    const r = pagedFailure([{ code: 'E1', message: 'fail', type: 500 }])
    expect(r.isSuccess).toBe(false)
    expect(r.items).toEqual([])
  })
})
