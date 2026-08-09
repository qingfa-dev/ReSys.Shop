import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import Select from 'primevue/select'
import ProductDetailView from '../ProductDetailView.vue'
import ProductCard from '../../components/ProductCard.vue'
import { useProductDetail } from '../../composables/useProductDetail'
import { useCartStore } from '@/features/ordering/stores/cartStore'
import type { StoreProductDetailResponse } from '../../types'

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

// Polyfill: TabList observes the tab strip with ResizeObserver; jsdom has none.
function createResizeObserverStub(): Pick<ResizeObserver, 'observe' | 'unobserve' | 'disconnect'> {
  return {
    observe(): void {},
    unobserve(): void {},
    disconnect(): void {},
  }
}

beforeAll(() => {
  vi.stubGlobal('matchMedia', vi.fn<typeof createMatchMediaStub>(createMatchMediaStub))
  vi.stubGlobal('ResizeObserver', vi.fn<typeof createResizeObserverStub>(createResizeObserverStub))
})

// Fixture: Product with a master variant, one option variant and a taxon trail.
const product: StoreProductDetailResponse = {
  id: 'p-1',
  masterVariantId: 'mv-1',
  name: 'Classic Tee',
  status: 'active',
  description: 'Classic organic cotton tee with a relaxed fit.',
  slug: 'classic-tee',
  styleCode: 'CT-001',
  seasonName: 'Spring 2026',
  materialComposition: '100% Organic Cotton',
  careInstructions: 'Machine wash cold.',
  fitNotes: 'Relaxed fit.',
  department: 'Menswear',
  genderTarget: 'Men',
  variantsCount: 2,
  availableOn: null,
  masterVariant: {
    id: 'mv-1',
    sku: 'CT-001',
    isMaster: true,
    price: 45,
    currency: 'USD',
    optionValues: [],
    images: [{ id: 'img-1', url: '/img/tee.jpg', alt: 'Classic Tee', position: 0 }],
    prices: [{ id: 'pr-1', amount: 45, currency: 'USD', compareAtAmount: 60, countryIso: 'US' }],
    stock: { availableQuantity: 12, backorderable: false },
  },
  variants: [
    {
      id: 'mv-1',
      sku: 'CT-001',
      isMaster: true,
      price: 45,
      currency: 'USD',
      optionValues: [],
      images: [{ id: 'img-1', url: '/img/tee.jpg', alt: 'Classic Tee', position: 0 }],
      prices: [{ id: 'pr-1', amount: 45, currency: 'USD', compareAtAmount: 60, countryIso: 'US' }],
      stock: { availableQuantity: 12, backorderable: false },
    },
    {
      id: 'v-2',
      sku: 'CT-002',
      isMaster: false,
      price: 48,
      currency: 'USD',
      optionValues: [
        {
          id: 'ov-1',
          variantOptionValueId: 'ovv-1',
          name: 'Large',
          presentation: 'Large',
          position: 0,
          optionTypeId: 'ot-1',
          optionTypeName: 'Size',
        },
      ],
      images: [{ id: 'img-2', url: '/img/tee-large.jpg', alt: 'Classic Tee Large', position: 0 }],
      prices: [{ id: 'pr-2', amount: 48, currency: 'USD', compareAtAmount: null, countryIso: 'US' }],
      stock: { availableQuantity: 8, backorderable: false },
    },
  ],
  classifications: [
    {
      id: 'taxon-1',
      name: 'T-Shirts',
      presentation: null,
      description: null,
      position: 0,
      parentId: 'taxon-0',
      taxonomyId: 'tax-1',
      parentName: 'Men',
      taxonomyName: 'Categories',
      depth: 1,
      taxonRuleCount: null,
      productCount: null,
      childrenCount: null,
      permalink: 'categories/men/t-shirts',
      prettyName: 'Men / T-Shirts',
      slug: 't-shirts',
      imageUrl: null,
      breadcrumb: [{ id: 'taxon-0', name: 'Men', permalink: 'categories/men' }],
    },
  ],
}

// Router: Memory-history router with the product route as the active target.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div />' } },
      { path: '/shop', component: { template: '<div />' } },
      { path: '/cart', component: { template: '<div />' } },
      { path: '/products/:slug', component: ProductDetailView },
    ],
  })
}

// Mount: PrimeVue + ToastService so mounted loads work.
async function mountView(slug = 'classic-tee') {
  const router = createTestRouter()
  await router.push(`/products/${slug}`)
  await router.isReady()
  const wrapper = mount(ProductDetailView, {
    global: {
      plugins: [PrimeVue, ToastService, router],
    },
  })
  await flushPromises()
  return wrapper
}

// Seed: Populate the detail composable with the fixture product and master variant.
function seedDetail() {
  const detail = useProductDetail()
  detail.product = product
  detail.selectedVariantId = 'mv-1'
  return detail
}

// Timeout: First mount is slow under full-suite CPU load (Galleria + PrimeVue import)
describe('ProductDetailView', { timeout: 30_000 }, () => {
  beforeEach(() => {
    vi.clearAllMocks()
    // Reset: Clear singleton detail state between tests
    const detail = useProductDetail()
    detail.reset()
  })

  it('renders the breadcrumb trail with home, shop, taxon and product entries', async () => {
    const wrapper = await mountView()
    seedDetail()
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Home')
    expect(wrapper.text()).toContain('Shop')
    expect(wrapper.text()).toContain('Men')
    expect(wrapper.text()).toContain('Classic Tee')
  })

  it('renders the title, current price, compare-at price and sale badge', async () => {
    const wrapper = await mountView()
    seedDetail()
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Classic Tee')
    expect(wrapper.text()).toContain('$45.00')
    expect(wrapper.text()).toContain('$60.00')
    expect(wrapper.text()).toContain('Sale')
  })

  it('adds the selected variant and quantity to the cart from the SplitButton', async () => {
    const wrapper = await mountView()
    const detail = seedDetail()
    detail.quantity = 2
    await wrapper.vm.$nextTick()

    const addButton = wrapper.findAll('button').find(b => b.text().includes('Add to Cart'))
    expect(addButton?.exists()).toBe(true)
    await addButton!.trigger('click')

    const cart = useCartStore()
    expect(cart.addItem).toHaveBeenCalledWith('mv-1', 2)
  })

  it('switches tabs and reveals the details content', async () => {
    const wrapper = await mountView()
    seedDetail()
    await wrapper.vm.$nextTick()

    const activePanel = () => wrapper.find('[data-pc-name="tabpanel"][data-p-active="true"]')
    expect(activePanel().text()).toContain('Classic organic cotton tee')

    const detailsTab = wrapper.findAll('[data-pc-name="tab"]').find(t => t.text().trim() === 'Details')
    expect(detailsTab?.exists()).toBe(true)
    await detailsTab!.trigger('click')

    expect(activePanel().text()).toContain('100% Organic Cotton')
    expect(activePanel().text()).toContain('Spring 2026')
  })

  it('switching the variant select updates the composable selection', async () => {
    const wrapper = await mountView()
    const detail = seedDetail()
    await wrapper.vm.$nextTick()

    const select = wrapper.findComponent(Select)
    expect(select.exists()).toBe(true)
    const options = select.props('options') as { value: string; label: string }[]
    expect(options.some(o => o.value === 'v-2' && o.label === 'Large')).toBe(true)

    select.vm.$emit('change', { value: 'v-2', originalEvent: {} })
    await wrapper.vm.$nextTick()

    expect(detail.selectedVariantId).toBe('v-2')
  })

  it('renders the related products grid from the detail composable', async () => {
    const wrapper = await mountView()
    const detail = seedDetail()
    detail.relatedProducts = [product]
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('You may also like')
    expect(wrapper.findAllComponents(ProductCard)).toHaveLength(1)
  })

  it('reloads the product when the route slug changes', async () => {
    const wrapper = await mountView()
    const detail = seedDetail()
    vi.spyOn(detail, 'load')
    await wrapper.vm.$nextTick()
    vi.clearAllMocks()

    const router = wrapper.vm.$router
    await router.push('/products/linen-shirt')
    await flushPromises()

    expect(detail.load).toHaveBeenCalledWith('linen-shirt')
  })
})
