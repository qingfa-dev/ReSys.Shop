import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { defineComponent, h } from 'vue'
import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query'
import { useLogin } from '../../api/login'

vi.mock('@/shared/api/client', () => ({
  api: { post: vi.fn<() => Promise<unknown>>().mockResolvedValue({ accessToken: 'a', refreshToken: 'b', expiresAt: '2026-07-06T00:00:00Z' }) },
}))

describe('useLogin', () => {
  it('calls api.post with login endpoint', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    let captured: ReturnType<typeof useLogin> | null = null
    const Host = defineComponent({
      setup() {
        captured = useLogin()
        return () => h('div')
      },
    })
    mount(Host, { global: { plugins: [[VueQueryPlugin, { queryClient: client }]] } })
    await captured!.mutateAsync({ email: 'a@b.co', password: 'secret123' })
    const { api } = await import('@/shared/api/client')
    expect(api.post).toHaveBeenCalledWith('/api/auth/login', { email: 'a@b.co', password: 'secret123' })
  })
})
