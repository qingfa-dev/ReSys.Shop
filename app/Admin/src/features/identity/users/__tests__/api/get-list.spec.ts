import { describe, it, expect, vi } from 'vitest'
import { ref } from 'vue'
import { useUsersList } from '../../api/get-list'

vi.mock('@/shared/api/client', () => ({
  api: {
    getPaged: vi.fn<() => Promise<unknown>>().mockResolvedValue({ items: [], totalCount: 0, page: 1, pageSize: 20 }),
  },
}))

describe('useUsersList', () => {
  it('queries /api/admin/identity/users with params', async () => {
    const params = ref({ page: 1, pageSize: 20, search: 'a' })
    useUsersList(params)
    await vi.waitFor(async () => {
      const { api } = await import('@/shared/api/client')
      expect(api.getPaged).toHaveBeenCalledWith(expect.stringContaining('/api/admin/identity/users'))
    })
  })
})
