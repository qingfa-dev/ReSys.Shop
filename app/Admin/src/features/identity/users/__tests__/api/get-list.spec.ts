import { describe, it, expect, vi } from 'vitest'
import { ref } from 'vue'
import { mount } from '@vue/test-utils'
import { defineComponent, h } from 'vue'
import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query'
import { useUsersList } from '../../api/get-list'

vi.mock('@/shared/api/client', () => ({
  api: {
    getPaged: vi.fn<() => Promise<unknown>>().mockResolvedValue({ items: [], totalCount: 0, page: 1, pageSize: 20 }),
  },
}))

describe('useUsersList', () => {
  it('queries /api/admin/identity/users with params', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const params = ref({ page: 1, pageSize: 20, search: 'a' })
    let captured: ReturnType<typeof useUsersList> | null = null
    const Host = defineComponent({
      setup() {
        captured = useUsersList(params)
        return () => h('div')
      },
    })
    mount(Host, { global: { plugins: [[VueQueryPlugin, { queryClient: client }]] } })
    await (captured as unknown as { suspense: () => Promise<unknown> }).suspense()
    const { api } = await import('@/shared/api/client')
    expect(api.getPaged).toHaveBeenCalledWith(expect.stringContaining('/api/admin/identity/users'))
  })
})
