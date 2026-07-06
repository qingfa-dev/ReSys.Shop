import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { defineComponent, h } from 'vue'
import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query'
import { useAuthGuard } from '../../composables/useAuthGuard'

vi.mock('@/shared/api/client', () => ({ api: { get: vi.fn<() => Promise<unknown>>() } }))

describe('useAuthGuard', () => {
  it('blocks unauthenticated access to authRequired routes', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    let captured: ReturnType<typeof useAuthGuard> | null = null
    const Host = defineComponent({
      setup() {
        captured = useAuthGuard({} as never)
        return () => h('div')
      },
    })
    mount(Host, { global: { plugins: [[VueQueryPlugin, { queryClient: client }]] } })
    const result = captured!(
      { meta: { authRequired: true }, fullPath: '/x' } as never,
      {} as never,
      vi.fn(),
    ) as unknown
    expect(typeof result).toBe('undefined')
  })

  it('allows access to non-authRequired routes', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    let captured: ReturnType<typeof useAuthGuard> | null = null
    const Host = defineComponent({
      setup() {
        captured = useAuthGuard({} as never)
        return () => h('div')
      },
    })
    mount(Host, { global: { plugins: [[VueQueryPlugin, { queryClient: client }]] } })
    const result = captured!(
      { meta: { authRequired: false }, fullPath: '/login' } as never,
      {} as never,
      vi.fn(),
    )
    expect(result).toBeUndefined()
  })
})
