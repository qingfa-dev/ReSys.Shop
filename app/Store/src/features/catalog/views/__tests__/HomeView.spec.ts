import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import HomeView from '../HomeView.vue'
import { useCatalogStore } from '../../stores/catalogStore'
import { useProductListStore } from '../../stores/productListStore'

vi.mock('../../stores/catalogStore', () => ({
  useCatalogStore: vi.fn(),
}))

vi.mock('../../stores/productListStore', () => ({
  useProductListStore: vi.fn(),
}))

vi.mock('@/shared/composables/usePageTitle', () => ({
  usePageTitle: vi.fn(),
}))

vi.mock('@/shared/composables/usePreferences', () => ({
  usePreferences: vi.fn(() => ({
    formatCurrency: (amount: number) => `$${amount.toFixed(2)}`,
  })),
}))

const stubs = {
  Button: {
    template: '<button><slot /></button>',
    props: ['severity', 'style'],
  },
  Skeleton: {
    template: '<div class="skeleton" />',
    props: ['width', 'height', 'class'],
  },
  InputText: {
    template: '<input />',
    props: ['type', 'placeholder', 'class'],
  },
  RouterLink: {
    template: '<a :href="to"><slot /></a>',
    props: ['to'],
  },
}

function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div />' } },
      { path: '/shop', component: { template: '<div />' } },
    ],
  })
}

describe('HomeView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(useCatalogStore).mockReturnValue({
      taxonomyGroups: [],
      loadTaxonomyGroups: vi.fn(),
    } as never)
    vi.mocked(useProductListStore).mockReturnValue({
      items: [],
      loading: false,
      isInitialLoad: true,
      init: vi.fn(),
    } as never)
  })

  it('renders hero headline text', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()

    const wrapper = mount(HomeView, {
      global: { plugins: [router], stubs },
    })

    expect(wrapper.text()).toContain('Curated fashion, intelligently found')
  })

  it('renders hero subtitle', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()

    const wrapper = mount(HomeView, {
      global: { plugins: [router], stubs },
    })

    expect(wrapper.text()).toContain('Discover pieces matched to your style through AI-powered curation.')
  })

  it('renders Shop New Arrivals CTA button', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()

    const wrapper = mount(HomeView, {
      global: { plugins: [router], stubs },
    })

    expect(wrapper.text()).toContain('Shop New Arrivals')
  })

  it('CTA links to /shop', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()

    const wrapper = mount(HomeView, {
      global: { plugins: [router], stubs },
    })

    const ctaLink = wrapper.find('a[href="/shop"]')
    expect(ctaLink.exists()).toBe(true)
  })

  it('renders Shop by Category section', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()

    const wrapper = mount(HomeView, {
      global: { plugins: [router], stubs },
    })

    expect(wrapper.text()).toContain('Shop by Category')
  })

  it('renders New Arrivals section', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()

    const wrapper = mount(HomeView, {
      global: { plugins: [router], stubs },
    })

    expect(wrapper.text()).toContain('New Arrivals')
  })

  it('renders waitlist CTA section', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()

    const wrapper = mount(HomeView, {
      global: { plugins: [router], stubs },
    })

    expect(wrapper.text()).toContain('Join the waitlist for exclusive drops')
  })
})
