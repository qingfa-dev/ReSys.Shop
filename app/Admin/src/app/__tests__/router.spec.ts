import { describe, it, expect, vi } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'
import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query'
import PrimeVue from 'primevue/config'
import { defineComponent, h, type Ref, ref } from 'vue'
import App from '../App.vue'
import { routes } from '../router/routes'
import { useAuthGuard } from '@/features/auth/composables/useAuthGuard'

vi.mock('@/shared/api/client', () => ({
  api: {
    get: vi.fn<() => Promise<unknown>>().mockResolvedValue({}),
    getPaged: vi.fn<() => Promise<unknown>>().mockResolvedValue({ items: [], totalCount: 0, page: 1, pageSize: 20 }),
    post: vi.fn<() => Promise<unknown>>().mockResolvedValue({}),
    put: vi.fn<() => Promise<unknown>>().mockResolvedValue({}),
    delete: vi.fn<() => Promise<unknown>>().mockResolvedValue(undefined),
  },
}))

describe('app router integration', () => {
  it('redirects unauthenticated users from / to /login', async () => {
    setActivePinia(createPinia())
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const router = createRouter({ history: createMemoryHistory(), routes })
    const guardRef: Ref<ReturnType<typeof useAuthGuard> | null> = ref(null)
    const GuardHost = defineComponent({
      setup() {
        guardRef.value = useAuthGuard()
        return () => h('div')
      },
    })
    mount(GuardHost, { global: { plugins: [[VueQueryPlugin, { queryClient: client }], PrimeVue] } })
    router.beforeEach(guardRef.value!)
    await router.push('/')
    await router.isReady()
    const wrapper = mount(App, {
      global: { plugins: [router, [VueQueryPlugin, { queryClient: client }], PrimeVue] },
    })
    await flushPromises()
    expect(wrapper.html()).toContain('Sign in')
  })
})
