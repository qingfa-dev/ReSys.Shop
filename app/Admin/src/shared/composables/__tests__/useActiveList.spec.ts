import { describe, it, expect, vi } from 'vitest'
import { useActiveList } from '../useActiveList'
import type { PagedResult } from '@/shared/types/result'

function okResult(overrides: Partial<PagedResult<{ id: string; name: string }>> = {}): PagedResult<{ id: string; name: string }> {
  return {
    isSuccess: true,
    statusCode: 200,
    items: [{ id: '1', name: 'Test' }],
    page: 1,
    pageSize: 20,
    totalCount: 1,
    totalPages: 1,
    errors: [],
    message: null,
    metadata: null,
    ...overrides,
  }
}

describe('useActiveList', () => {
  it('loads items on demand and dedupes subsequent load calls', async () => {
    const fetcher = vi.fn<() => Promise<PagedResult<{ id: string; name: string }>>>().mockResolvedValue(okResult())
    const { items, load } = useActiveList<{ id: string; name: string }>(fetcher)

    await load()
    await load()

    expect(fetcher).toHaveBeenCalledTimes(1)
    expect(items.value).toHaveLength(1)
    expect(items.value[0]!.name).toBe('Test')
  })

  it('exposes the failure message and allows retry after reset', async () => {
    const fetcher = vi.fn<() => Promise<PagedResult<{ id: string; name: string }>>>()
      .mockResolvedValueOnce({ ...okResult(), isSuccess: false, message: 'boom' })
      .mockResolvedValueOnce(okResult())
    const { error, items, load, reset } = useActiveList<{ id: string; name: string }>(fetcher)

    await load()
    expect(error.value).toBe('boom')

    reset()
    await load()
    expect(items.value).toHaveLength(1)
  })
})
