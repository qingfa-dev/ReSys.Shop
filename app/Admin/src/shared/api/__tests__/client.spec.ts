import { describe, it, expect, vi, beforeEach } from 'vitest'
import { api, ApiError } from '../client'

describe('api client', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
  })

  it('returns parsed JSON for 2xx responses', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        new Response(JSON.stringify({ id: 1 }), { status: 200, headers: { 'Content-Type': 'application/json' } }),
      ),
    )
    const result = await api.get<{ id: number }>('/x')
    expect(result).toEqual({ id: 1 })
  })

  it('throws ApiError on non-2xx', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('nope', { status: 404 })))
    await expect(api.get('/x')).rejects.toBeInstanceOf(ApiError)
  })

  it('returns undefined for 204 responses', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(null, { status: 204 })))
    const result = await api.delete('/x')
    expect(result).toBeUndefined()
  })
})
