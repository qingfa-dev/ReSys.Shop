import { describe, it, expect, vi } from 'vitest'
import { ref } from 'vue'
import { QueryBuilder } from '@/shared/query'

vi.mock('@/shared/api/client', () => ({
  api: {
    getPaged: vi.fn<() => Promise<unknown>>().mockResolvedValue({
      items: [{ id: '1', name: 'Test' }],
      totalCount: 1,
      page: 1,
      pageSize: 20,
    }),
  },
}))

describe('useList', () => {
  it('calls api.getPaged on mount with correct URL', async () => {
    const { useList } = await import('../useList')
    const builder = ref(new QueryBuilder().page(1, 20))

    const { data, total } = useList('/api/users', builder)

    await vi.waitFor(async () => {
      const { api } = await import('@/shared/api/client')
      expect(api.getPaged).toHaveBeenCalledWith(expect.stringContaining('/api/users'))
    })

    expect(data.value).toEqual([{ id: '1', name: 'Test' }])
    expect(total.value).toBe(1)
  })
})
