import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import AppHeader from '../AppHeader.vue'
import { useCartStore } from '@/features/ordering/stores/cartStore'

// Polyfill: Menubar and MegaMenu call matchMedia on mount; jsdom does not provide it.
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

// Router: Memory-history router with empty views for command navigation targets.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div />' } },
      { path: '/login', component: { template: '<div />' } },
      { path: '/shop', component: { template: '<div />' } },
    ],
  })
}

// Mount: Provide PrimeVue + stubbed pinia so real components render without network calls.
function mountHeader(router = createTestRouter()) {
  return mount(AppHeader, {
    global: {
      plugins: [PrimeVue, createTestingPinia({ stubActions: true }), router],
    },
  })
}

describe('AppHeader', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders the brand and the Sign In fallback when logged out', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mountHeader(router)

    expect(wrapper.find('[aria-label="ReSys.Shop home"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('ReSys.Shop')
    expect(wrapper.text()).toContain('Sign In')
  })

  it('emits open-mobile-nav when the hamburger is clicked', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mountHeader(router)

    await wrapper.find('[aria-label="Open navigation menu"]').trigger('click')

    expect(wrapper.emitted('open-mobile-nav')).toHaveLength(1)
  })

  it('emits open-cart when the cart button is clicked', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mountHeader(router)
    const cart = useCartStore()
    expect(cart.itemCount).toBe(0)

    await wrapper.find('[aria-label="Open cart"]').trigger('click')

    expect(wrapper.emitted('open-cart')).toHaveLength(1)
  })

  it('renders the Catalog button linking to /shop', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mountHeader(router)

    const catalogLink = wrapper.find('a[href="/shop"]')
    expect(catalogLink.exists()).toBe(true)
    expect(catalogLink.text()).toContain('Catalog')
  })
})
