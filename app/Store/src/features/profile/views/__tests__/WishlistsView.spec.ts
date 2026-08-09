import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import WishlistsView from '../WishlistsView.vue'
import { useWishlists } from '../../composables/useWishlists'
import type { WishlistListItem, WishlistDetail } from '../../types'

// Polyfill: Dialog calls matchMedia on mount; jsdom does not provide it.
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

// Polyfill: TabList binds a ResizeObserver on mount; jsdom does not provide it.
class ResizeObserverStub {
  observe(): void {}
  unobserve(): void {}
  disconnect(): void {}
}

beforeAll(() => {
  vi.stubGlobal('matchMedia', vi.fn<typeof createMatchMediaStub>(createMatchMediaStub))
  vi.stubGlobal('ResizeObserver', ResizeObserverStub)
})

// Router: Memory-history router with the account route.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [{ path: '/account/wishlists', component: WishlistsView }],
  })
}

// Mount: PrimeVue + ToastService + stubbed pinia so mounted fetches are no-ops.
async function mountView() {
  const router = createTestRouter()
  await router.push('/account/wishlists')
  await router.isReady()
  const wrapper = mount(WishlistsView, {
    global: {
      plugins: [PrimeVue, ToastService, createTestingPinia({ stubActions: true }), router],
      stubs: { teleport: true },
    },
  })
  await flushPromises()
  return wrapper
}

// Fixtures: Two list summaries plus the detail cache for the first list.
const listOne: WishlistListItem = { id: 'wl-1', name: 'Summer Faves', isPrivate: false, itemCount: 2 }
const listTwo: WishlistListItem = { id: 'wl-2', name: 'Gifts', isPrivate: true, itemCount: 0 }

const detailOne: WishlistDetail = {
  id: 'wl-1',
  name: 'Summer Faves',
  isPrivate: false,
  itemCount: 2,
  token: 'tok-1',
  isDefault: true,
  wishedItems: [
    { id: 'item-1', variantId: 'v-100', quantity: 1, addedAtUtc: '2026-07-01T10:00:00Z' },
    { id: 'item-2', variantId: 'v-200', quantity: 2, addedAtUtc: '2026-07-05T10:00:00Z' },
  ],
}

// Seed: Populate lists and the detail cache; the list watcher selects the first tab.
async function seedLists() {
  const store = useWishlists()
  store.lists = [listOne, listTwo]
  store.details = { 'wl-1': detailOne }
  await flushPromises()
  return store
}

describe('WishlistsView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders one tab per wishlist with item-count tags', async () => {
    const wrapper = await mountView()
    await seedLists()
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Summer Faves')
    expect(wrapper.text()).toContain('Gifts')
    expect(wrapper.text()).toContain('2')
  })

  it('shows the empty state when no wishlists exist', async () => {
    const wrapper = await mountView()

    expect(wrapper.text()).toContain('You have no wishlists yet.')
  })

  it('renders the active list items and removes one through the store', async () => {
    const wrapper = await mountView()
    const store = await seedLists()
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('v-100')
    expect(wrapper.text()).toContain('v-200')
    await wrapper.findAll('[aria-label="Remove item"]')[0]!.trigger('click')
    await flushPromises()

    expect(store.removeItem).toHaveBeenCalledWith('wl-1', 'item-1')
  })

  it('creates a new list through the dialog and the store', async () => {
    const wrapper = await mountView()
    const store = await seedLists()
    vi.mocked(store.createWishlist).mockResolvedValue(true)

    await wrapper.findAll('button').find(b => b.text() === 'New list')!.trigger('click')
    await wrapper.vm.$nextTick()
    await wrapper.find('#wishlist-name').setValue('Birthday Ideas')
    await wrapper.findAll('button').find(b => b.text() === 'Create')!.trigger('click')
    await flushPromises()

    expect(store.createWishlist).toHaveBeenCalledWith({ name: 'Birthday Ideas', isPrivate: false })
  })
})
