import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import MobileNav from '../MobileNav.vue'
import { useAuthStore } from '@/features/identity/stores/authStore'

// Polyfill: Drawer and PanelMenu call matchMedia on mount; jsdom does not provide it.
function createMatchMediaStub(query: string) {
  return {
    matches: false,
    media: query,
    onchange: null,
    addEventListener: vi.fn<() => void>(),
    removeEventListener: vi.fn<() => void>(),
    addListener: vi.fn<() => void>(),
    removeListener: vi.fn<() => void>(),
    dispatchEvent: vi.fn<() => void>(),
  }
}

beforeAll(() => {
  vi.stubGlobal('matchMedia', vi.fn<typeof createMatchMediaStub>(createMatchMediaStub))
})

// Router: Memory-history router with empty views for the drawer's link targets.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div />' } },
      { path: '/shop', component: { template: '<div />' } },
      { path: '/login', component: { template: '<div />' } },
    ],
  })
}

// Mount: Provide PrimeVue + stubbed pinia so the drawer renders without network calls.
// Teleport: Keep the drawer DOM inside the wrapper so assertions stay scoped.
async function mountNav(router = createTestRouter(), visible = true) {
  const wrapper = mount(MobileNav, {
    props: { visible },
    global: {
      plugins: [PrimeVue, createTestingPinia({ stubActions: true }), router],
      stubs: { teleport: true },
    },
  })
  // Flush: The drawer renders its body through an appear transition.
  await flushPromises()
  await wrapper.vm.$nextTick()
  return wrapper
}

describe('MobileNav', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders the primary storefront routes', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = await mountNav(router)

    expect(wrapper.text()).toContain('Home')
    expect(wrapper.text()).toContain('Shop')
    expect(wrapper.text()).toContain('Collections')
    expect(wrapper.text()).toContain('Visual Search')
  })

  it('shows the Sign In fallback for guests', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = await mountNav(router)

    expect(wrapper.text()).toContain('Sign In')
    expect(wrapper.text()).not.toContain('Profile')
    expect(wrapper.text()).not.toContain('Orders')
  })

  it('shows account links when authenticated', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = await mountNav(router)
    const auth = useAuthStore()
    auth.$patch({
      status: 'authenticated',
      user: { userId: 'u1', userName: 'Ada', email: 'ada@test.dev', roles: [], permissions: [], isAuthenticated: true },
    })
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Profile')
    expect(wrapper.text()).toContain('Orders')
    expect(wrapper.text()).not.toContain('Sign In')
  })

  it('closes the drawer on route change', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = await mountNav(router)

    await router.push('/shop')
    await wrapper.vm.$nextTick()

    expect(wrapper.emitted('update:visible')?.at(-1)).toEqual([false])
  })

  it('adds no native interactive elements of its own', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = await mountNav(router)

    // Drawer renders its own PrimeVue close button; ours must not add more.
    expect(wrapper.findAll('button').length).toBe(1)
    expect(wrapper.find('input').exists()).toBe(false)
  })
})
