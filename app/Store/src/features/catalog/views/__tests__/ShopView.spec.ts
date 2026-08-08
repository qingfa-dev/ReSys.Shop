import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import ShopView from '../ShopView.vue'
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
  Breadcrumb: {
    template: '<nav><ul><li v-for="item in model" :key="item.label">{{ item.label }}</li></ul></nav>',
    props: ['model'],
  },
  InputText: {
    template: '<input :type="type" :placeholder="placeholder" />',
    props: ['type', 'placeholder', 'modelValue', 'class'],
  },
  Select: {
    template: '<select />',
    props: ['modelValue', 'options', 'optionLabel', 'optionValue', 'class'],
  },
  Paginator: {
    template: '<div class="paginator" />',
    props: ['first', 'rows', 'totalRecords', 'pageLinkLimit'],
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

describe('ShopView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(useCatalogStore).mockReturnValue({
      taxonomyGroups: [],
      optionTypes: [],
      selectedTaxonIds: [],
      selectedOptionValueIds: [],
      minPrice: null,
      maxPrice: null,
      sortField: '-createdAtUtc',
      activeFilterCount: 0,
      loadTaxonomyGroups: vi.fn(),
      loadOptionTypes: vi.fn(),
      toggleTaxon: vi.fn(),
      toggleOptionValue: vi.fn(),
      setPriceRange: vi.fn(),
      setSort: vi.fn(),
      clearFilters: vi.fn(),
    } as never)
    vi.mocked(useProductListStore).mockReturnValue({
      items: [],
      loading: false,
      error: null,
      page: 1,
      pageSize: 20,
      totalCount: 0,
      isInitialLoad: true,
      init: vi.fn(),
      refresh: vi.fn(),
      goToPage: vi.fn(),
    } as never)
  })

  it('renders breadcrumb with Home and Shop labels', async () => {
    const router = createTestRouter()
    await router.push('/shop')
    await router.isReady()

    const wrapper = mount(ShopView, {
      global: { plugins: [router], stubs },
    })

    expect(wrapper.text()).toContain('Home')
    expect(wrapper.text()).toContain('Shop')
  })

  it('renders Price filter section', async () => {
    const router = createTestRouter()
    await router.push('/shop')
    await router.isReady()

    const wrapper = mount(ShopView, {
      global: { plugins: [router], stubs },
    })

    expect(wrapper.text()).toContain('Price')
  })

  it('renders Min price input', async () => {
    const router = createTestRouter()
    await router.push('/shop')
    await router.isReady()

    const wrapper = mount(ShopView, {
      global: { plugins: [router], stubs },
    })

    const minInput = wrapper.find('input[placeholder="Min"]')
    expect(minInput.exists()).toBe(true)
  })

  it('renders Max price input', async () => {
    const router = createTestRouter()
    await router.push('/shop')
    await router.isReady()

    const wrapper = mount(ShopView, {
      global: { plugins: [router], stubs },
    })

    const maxInput = wrapper.find('input[placeholder="Max"]')
    expect(maxInput.exists()).toBe(true)
  })

  it('renders sort dropdown', async () => {
    const router = createTestRouter()
    await router.push('/shop')
    await router.isReady()

    const wrapper = mount(ShopView, {
      global: { plugins: [router], stubs },
    })

    expect(wrapper.findComponent({ name: 'Select' }).exists() || wrapper.find('select').exists()).toBe(true)
  })

  it('renders Filters button for mobile', async () => {
    const router = createTestRouter()
    await router.push('/shop')
    await router.isReady()

    const wrapper = mount(ShopView, {
      global: { plugins: [router], stubs },
    })

    expect(wrapper.text()).toContain('Filters')
  })

  it('calls store.init on mount', async () => {
    const init = vi.fn()
    vi.mocked(useProductListStore).mockReturnValue({
      items: [],
      loading: false,
      error: null,
      page: 1,
      pageSize: 20,
      totalCount: 0,
      isInitialLoad: true,
      init,
      refresh: vi.fn(),
      goToPage: vi.fn(),
    } as never)

    const router = createTestRouter()
    await router.push('/shop')
    await router.isReady()

    mount(ShopView, {
      global: { plugins: [router], stubs },
    })

    expect(init).toHaveBeenCalled()
  })
})
