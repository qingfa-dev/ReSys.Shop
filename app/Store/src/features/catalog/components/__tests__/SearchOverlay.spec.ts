import { describe, it, expect, vi, beforeAll, beforeEach, afterEach } from 'vitest'
import { mount, flushPromises, enableAutoUnmount } from '@vue/test-utils'
import { nextTick } from 'vue'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import SearchOverlay from '../SearchOverlay.vue'
import { useSearch } from '../../composables/useSearch'
import type { StoreProductListItemResponse } from '../../types'

// Polyfill: Dialog and Menu call matchMedia on mount; jsdom does not provide it.
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

// Fixture: Minimal list-item shape for search results.
function product(id: string, name: string, slug: string): StoreProductListItemResponse {
  return {
    id,
    masterVariantId: `mv-${id}`,
    name,
    status: 'active',
    description: null,
    slug,
    styleCode: null,
    seasonName: null,
    materialComposition: null,
    careInstructions: null,
    fitNotes: null,
    department: null,
    genderTarget: null,
    variantsCount: 1,
    availableOn: null,
    masterVariant: null,
    classifications: [],
  }
}

// Router: Memory-history router for the shop view-all destination.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div />' } },
      { path: '/shop', component: { template: '<div />' } },
    ],
  })
}

// Mount: PrimeVue + stubbed pinia; teleport stays in-tree so dialog assertions stay scoped.
// Flush: Dialog content mounts through microtask chains, so settle twice before asserting.
async function mountPalette(router = createTestRouter()) {
  const wrapper = mount(SearchOverlay, {
    global: {
      plugins: [PrimeVue, createTestingPinia({ stubActions: true }), router],
      stubs: { teleport: true },
    },
  })
  await flushPromises()
  await nextTick()
  return wrapper
}

// Unmount: Leftover palettes re-open on the shared singleton and leak into later tests.
enableAutoUnmount(afterEach)

describe('SearchOverlay', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    // Reset: The useSearch singleton survives across tests in this file.
    const search = useSearch()
    search.close()
    search.loading.value = false
    search.results.value = []
    // Timers: Any in-flight debounce from a prior test must never fire mid-test.
    vi.useFakeTimers()
  })

  it('stays closed until useSearch().open() is called', async () => {
    const wrapper = await mountPalette()

    expect(wrapper.find('.p-dialog').exists()).toBe(false)

    useSearch().open()
    await nextTick()

    expect(wrapper.find('.p-dialog').exists()).toBe(true)
  })

  it('updates the shared query and triggers search() as the user types', async () => {
    const search = useSearch()
    search.open()
    const wrapper = await mountPalette()
    const spy = vi.spyOn(search, 'search')

    await wrapper.find('input[role="combobox"]').setValue('jeans')

    expect(search.query.value).toBe('jeans')
    expect(spy).toHaveBeenCalledOnce()
    // Debounce: Advance the 300ms window so no real API call leaks into later tests.
    await vi.advanceTimersByTimeAsync(300)
  })

  it('maps results to commands and navigates on selection', async () => {
    const search = useSearch()
    search.results.value = [product('p1', 'Slim Jeans', 'slim-jeans'), product('p2', 'Loose Jeans', 'loose-jeans')]
    search.open()
    const wrapper = await mountPalette()
    const spy = vi.spyOn(search, 'navigateToResult')

    const links = wrapper.findAll('.p-menu-item-link')
    expect(links.map(link => link.text())).toContain('Slim Jeans')

    const target = links.find(link => link.text().includes('Slim Jeans'))
    expect(target).toBeDefined()
    await target!.trigger('click')

    expect(spy).toHaveBeenCalledExactlyOnceWith(0)
  })

  it('pushes /shop with the query via the view-all command and closes', async () => {
    const search = useSearch()
    search.query.value = 'denim'
    search.open()
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = await mountPalette(router)

    const buttons = wrapper.findAll('button')
    const target = buttons.find(button => button.text().includes('View all results'))
    expect(target).toBeDefined()
    await target!.trigger('click')
    await flushPromises()

    expect(router.currentRoute.value.path).toBe('/shop')
    expect(router.currentRoute.value.query.q).toBe('denim')
    expect(search.isOpen.value).toBe(false)
  })

  it('shows the empty message when a query has no results', async () => {
    const search = useSearch()
    search.query.value = 'denim'
    search.open()
    const wrapper = await mountPalette()
    await nextTick()

    expect(wrapper.find('.p-commandmenu-empty-message').text()).toContain('No products found')
  })

  it('shows skeleton rows while a search is loading', async () => {
    const search = useSearch()
    search.query.value = 'denim'
    search.loading.value = true
    search.open()
    const wrapper = await mountPalette()
    await nextTick()

    expect(wrapper.findAll('.p-skeleton')).toHaveLength(3)
    expect(wrapper.find('.p-message').exists()).toBe(false)
  })

  it('closes the search when the dialog is dismissed', async () => {
    const search = useSearch()
    search.open()
    const wrapper = await mountPalette()

    wrapper.getComponent({ name: 'Dialog' }).vm.$emit('update:visible', false)
    await nextTick()

    expect(search.isOpen.value).toBe(false)
    expect(search.query.value).toBe('')
  })

  it('adds no native interactive elements of its own', async () => {
    const search = useSearch()
    search.open()
    const wrapper = await mountPalette()

    // Palette input is PrimeVue's; only the view-all footer button is ours, gated on a query.
    expect(wrapper.findAll('input')).toHaveLength(1)
    expect(wrapper.findAll('button')).toHaveLength(0)
  })

  it('adds the view-all button only while a query is set', async () => {
    const search = useSearch()
    search.query.value = 'denim'
    search.open()
    const wrapper = await mountPalette()

    const buttons = wrapper.findAll('button')
    expect(buttons).toHaveLength(1)
    expect(buttons[0]!.text()).toContain('View all results')
  })
})
