import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import PrimeVue from 'primevue/config'
import { createTestingPinia } from '@pinia/testing'
import ToastService from 'primevue/toastservice'
import HomeView from '../HomeView.vue'
import ProductGridCard from '../../components/ProductGridCard.vue'
import { useTaxonomy } from '../../composables/useTaxonomy'
import { useProducts } from '../../composables/useProducts'
import type { StoreProductListItemResponse, TaxonomyGroup } from '../../types'

// Polyfill: Overlay components call matchMedia on mount; jsdom does not provide it.
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

// Polyfill: AnimateOnScroll constructs an IntersectionObserver on mount; jsdom has none.
function createIntersectionObserverStub(): Pick<IntersectionObserver, 'observe' | 'unobserve' | 'disconnect' | 'takeRecords'> {
  return {
    observe(): void {},
    unobserve(): void {},
    disconnect(): void {},
    takeRecords(): IntersectionObserverEntry[] {
      return []
    },
  }
}

beforeAll(() => {
  vi.stubGlobal('matchMedia', vi.fn<typeof createMatchMediaStub>(createMatchMediaStub))
  vi.stubGlobal('IntersectionObserver', vi.fn<typeof createIntersectionObserverStub>(createIntersectionObserverStub))
})

// Fixture: Minimal product with a priced master variant and one image.
const product: StoreProductListItemResponse = {
  id: 'p-1',
  masterVariantId: 'mv-1',
  name: 'Classic Tee',
  status: 'active',
  description: null,
  slug: 'classic-tee',
  styleCode: null,
  seasonName: null,
  materialComposition: null,
  careInstructions: null,
  fitNotes: null,
  department: 'Menswear',
  genderTarget: null,
  variantsCount: 1,
  availableOn: null,
  masterVariant: {
    id: 'mv-1',
    sku: 'CT-001',
    isMaster: true,
    price: 45,
    currency: 'USD',
    optionValues: [],
    images: [{ id: 'img-1', url: '/img/tee.jpg', alt: 'Classic Tee', position: 0 }],
    prices: [{ id: 'pr-1', amount: 45, currency: 'USD', compareAtAmount: null, countryIso: 'US' }],
    stock: { totalOnHand: 5, totalReserved: 0, totalAvailable: 5, backorderable: false, locations: [] },
  },
  classifications: [],
}

// Fixture: Taxonomy group with two root taxons for the category row.
const taxonomyGroup: TaxonomyGroup = {
  taxonomy: { id: 't1', name: 'Categories', presentation: 'Categories' },
  tree: [
    {
      id: 'r1',
      name: 'Men',
      presentation: null,
      permalink: 'men',
      depth: 0,
      hasChildren: false,
      children: [],
    },
    {
      id: 'r2',
      name: 'Women',
      presentation: null,
      permalink: 'women',
      depth: 0,
      hasChildren: false,
      children: [],
    },
  ],
}

// Router: Memory-history router with home and shop routes as link targets.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div />' } },
      { path: '/shop', component: { template: '<div />' } },
    ],
  })
}

// Mount: PrimeVue + ToastService so mounted loads work.
function mountView(router = createTestRouter()) {
  return mount(HomeView, {
    global: {
      plugins: [PrimeVue, createTestingPinia({ stubActions: true }), ToastService, router],
    },
  })
}

describe('HomeView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    // Reset: Clear singleton state between tests
    const taxonomy = useTaxonomy()
    taxonomy.taxonomyGroups.splice(0)
    taxonomy.optionTypes.splice(0)
    taxonomy.collections.splice(0)
    const list = useProducts()
    list.items.splice(0)
    list.totalCount = 0
    list.page = 1
    list.isInitialLoad = true
    list.loading = false
    list.error = null
  })

  it('renders the hero headline and a CTA linking to /shop', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mountView(router)
    await flushPromises()

    expect(wrapper.text()).toContain('Curated fashion, intelligently found')
    expect(wrapper.find('a[href="/shop"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('Shop New Arrivals')
  })

  it('renders one ProductGridCard per seeded featured product', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mountView(router)
    await flushPromises()
    const list = useProducts()
    list.items.push(product)
    list.items.push({ ...product, id: 'p-2', slug: 'linen-shirt' })
    list.isInitialLoad = false
    await wrapper.vm.$nextTick()

    const cards = wrapper.findAllComponents(ProductGridCard)
    expect(cards).toHaveLength(2)
    expect(wrapper.text()).toContain('Classic Tee')
  })

  it('shows skeleton placeholders while the featured rail is loading', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mountView(router)
    await flushPromises()
    const list = useProducts()
    list.items.splice(0)
    list.isInitialLoad = true
    list.loading = true
    await wrapper.vm.$nextTick()

    expect(wrapper.findAllComponents(ProductGridCard)).toHaveLength(0)
    expect(wrapper.findAll('div[data-pc-name="skeleton"]').length).toBeGreaterThan(0)
  })

  it('renders a category tag link per root taxon pointing at /shop?taxon=<id>', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mountView(router)
    await flushPromises()
    const taxonomy = useTaxonomy()
    taxonomy.taxonomyGroups.push(taxonomyGroup)
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Shop by Category')
    expect(wrapper.text()).toContain('Men')
    expect(wrapper.text()).toContain('Women')
    expect(wrapper.find('a[href="/shop?taxon=r1"]').exists()).toBe(true)
    expect(wrapper.find('a[href="/shop?taxon=r2"]').exists()).toBe(true)
  })
})
