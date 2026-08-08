import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import HomeView from '../HomeView.vue'
import ProductCard from '../../components/ProductCard.vue'
import { useCatalogStore } from '../../stores/catalogStore'
import { useProductListStore } from '../../stores/productListStore'
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
// The callback never fires, so the hero stays at opacity 0 — text assertions are unaffected.
// A plain function (not a class) so the vi.fn() mock stays constructable with methods intact.
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
    stock: { availableQuantity: 5, backorderable: false },
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

// Mount: PrimeVue + ToastService + stubbed pinia so mounted loads are no-ops.
function mountView(router = createTestRouter()) {
  return mount(HomeView, {
    global: {
      plugins: [PrimeVue, ToastService, createTestingPinia({ stubActions: true }), router],
    },
  })
}

describe('HomeView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
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

  it('renders one ProductCard per seeded featured product', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mountView(router)
    await flushPromises()
    const list = useProductListStore()
    list.items = [product, { ...product, id: 'p-2', slug: 'linen-shirt' }]
    list.isInitialLoad = false
    await wrapper.vm.$nextTick()

    const cards = wrapper.findAllComponents(ProductCard)
    expect(cards).toHaveLength(2)
    expect(wrapper.text()).toContain('Classic Tee')
  })

  it('shows skeleton placeholders while the featured rail is loading', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mountView(router)
    await flushPromises()
    const list = useProductListStore()
    list.items = []
    list.isInitialLoad = true
    list.loading = true
    await wrapper.vm.$nextTick()

    expect(wrapper.findAllComponents(ProductCard)).toHaveLength(0)
    expect(wrapper.findAll('div[data-pc-name="skeleton"]').length).toBeGreaterThan(0)
  })

  it('renders a category tag link per root taxon pointing at /shop?taxon=<id>', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mountView(router)
    await flushPromises()
    const catalog = useCatalogStore()
    catalog.taxonomyGroups = [taxonomyGroup]
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Shop by Category')
    expect(wrapper.text()).toContain('Men')
    expect(wrapper.text()).toContain('Women')
    expect(wrapper.find('a[href="/shop?taxon=r1"]').exists()).toBe(true)
    expect(wrapper.find('a[href="/shop?taxon=r2"]').exists()).toBe(true)
  })
})
