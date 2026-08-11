import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import ProductGridCard from '../ProductGridCard.vue'
import { useWishlists } from '@/features/profile/composables/useWishlists'
import { useCart } from '@/features/ordering/composables/useCart'
import type { StoreProductListItemResponse } from '@/features/catalog/types'

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

beforeAll(() => {
  vi.stubGlobal('matchMedia', vi.fn<typeof createMatchMediaStub>(createMatchMediaStub))
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
    prices: [{ id: 'pr-1', amount: 45, currency: 'USD', compareAtAmount: 60, countryIso: 'US' }],
    stock: { totalOnHand: 5, totalReserved: 0, totalAvailable: 5, backorderable: false, locations: [] },
  },
  classifications: [],
}

// Router: Memory-history router with the product detail route as link target.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div />' } },
      { path: '/products/:id', component: { template: '<div />' } },
    ],
  })
}

// Mount: PrimeVue + ToastService + stubbed pinia; teleport stays in-wrapper for popover asserts.
async function mountCard(router = createTestRouter()) {
  const wrapper = mount(ProductGridCard, {
    props: { product, ratingAverage: 4.5, ratingCount: 12 },
    global: {
      plugins: [PrimeVue, ToastService, createTestingPinia({ stubActions: true }), router],
      stubs: { teleport: true },
    },
  })
  await router.isReady()
  return wrapper
}

describe('ProductGridCard', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders the product name and formatted price line with compare-at', async () => {
    const wrapper = await mountCard()

    expect(wrapper.text()).toContain('Classic Tee')
    expect(wrapper.text()).toContain('$45.00')
    expect(wrapper.text()).toContain('$60.00')
  })

  it('calls wishlistStore.addItem on the default wishlist when toggled', async () => {
    const router = createTestRouter()
    const wrapper = await mountCard(router)
    const wishlists = useWishlists()
    wishlists.details = {
      'wl-1': {
        id: 'wl-1',
        name: 'Default',
        isPrivate: false,
        itemCount: 0,
        token: 'tok-1',
        isDefault: true,
        wishedItems: [],
      },
    }

    await wrapper.find('[aria-label="Add to wishlist"]').trigger('click')

    expect(wishlists.addItem).toHaveBeenCalledWith('wl-1', { variantId: 'mv-1', quantity: 1 })
    expect(wrapper.emitted('toggle-wishlist')).toHaveLength(1)
    expect(wrapper.emitted('toggle-wishlist')?.[0]).toEqual([product])
  })

  it('navigates to the product detail page when the card body is clicked', async () => {
    const router = createTestRouter()
    const wrapper = await mountCard(router)

    await wrapper.find('a').trigger('click')
    await flushPromises()

    expect(router.currentRoute.value.path).toBe('/products/p-1')
  })

  it('does not navigate when the wishlist or quick-add buttons are clicked', async () => {
    const router = createTestRouter()
    const wrapper = await mountCard(router)
    const cart = useCart()
    vi.mocked(cart.addItem).mockResolvedValue(true)

    await wrapper.find('[aria-label="Add to wishlist"]').trigger('click')
    await wrapper.find('[aria-label="Add to cart"]').trigger('click')

    expect(router.currentRoute.value.path).toBe('/')
    expect(cart.addItem).toHaveBeenCalledWith('mv-1', 1)
  })

  it('opens the quick-view popover with image, name, price and detail link', async () => {
    const wrapper = await mountCard()

    await wrapper.find('[aria-label="Quick view"]').trigger('click')
    await flushPromises()

    expect(wrapper.text()).toContain('View details')
    expect(wrapper.find('img[alt="Classic Tee"]').exists()).toBe(true)
  })

  it('renders quick actions in the right-click context menu', async () => {
    const wrapper = await mountCard()

    await wrapper.find('a').trigger('contextmenu')
    await flushPromises()

    expect(wrapper.text()).toContain('Quick view')
    expect(wrapper.text()).toContain('Add to cart')
    expect(wrapper.text()).toContain('Wishlist')
  })
})
