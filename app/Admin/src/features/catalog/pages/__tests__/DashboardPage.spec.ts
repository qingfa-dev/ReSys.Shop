import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createWebHistory } from 'vue-router'
import { createPinia, setActivePinia } from 'pinia'
import PrimeVue from 'primevue/config'
import ConfirmationService from 'primevue/confirmationservice'
import ToastService from 'primevue/toastservice'
import DashboardPage from '../DashboardPage.vue'
import type { CatalogDashboardResponse } from '../../types'

vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: (key: string) => {
      const map: Record<string, string> = {
        'catalog.dashboard.title': 'Catalog',
        'catalog.dashboard.description': 'Overview of your product catalog',
        'catalog.dashboard.total_products': 'Total Products',
        'catalog.dashboard.active': 'active',
        'catalog.dashboard.inactive': 'inactive',
        'catalog.dashboard.this_month': 'this month',
        'catalog.dashboard.catalog_coverage': 'Catalog Coverage',
        'catalog.dashboard.needs_attention': 'Needs Attention',
        'catalog.dashboard.quick_actions': 'Quick Actions',
        'catalog.dashboard.add_product': 'Add Product',
        'catalog.dashboard.import_csv': 'Import CSV',
        'catalog.dashboard.manage_categories': 'Manage Categories',
        'catalog.dashboard.recently_updated': 'Recently Updated',
        'catalog.dashboard.recent_empty': 'No products added yet.',
        'catalog.dashboard.attention_empty': 'Everything looks good',
        'catalog.taxonomies.titles.list': 'Taxonomies',
        'catalog.option_types.titles.list': 'Option Types',
        'catalog.dashboard.messages.load_failed': 'Failed to load dashboard.',
      }
      return map[key] ?? key
    },
  }),
}))

const mockDashboardApi = vi.fn()

vi.mock('../../api', () => ({
  CatalogDashboardApi: {
    get: () => mockDashboardApi(),
  },
}))

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: { template: '<div />' } },
    { name: 'catalog.products.create', path: '/catalog/products/new', component: { template: '<div />' } },
    { name: 'catalog.taxonomies.list', path: '/catalog/taxonomies', component: { template: '<div />' } },
  ],
})

function createSuccessResponse(overrides: Partial<CatalogDashboardResponse> = {}) {
  return {
    isSuccess: true,
    statusCode: 200,
    errors: [],
    message: null,
    metadata: null,
    value: {
      totalProducts: 1247,
      activeProducts: 987,
      draftProducts: 260,
      totalVariants: 15,
      totalTaxonomies: 8,
      totalTaxons: 42,
      recentProducts: [
        { id: '1', name: 'Vintage Denim Jacket', slug: 'vintage-denim-jacket', createdAtUtc: new Date(Date.now() - 7200000).toISOString() },
        { id: '2', name: 'Merino Wool Sweater', slug: 'merino-wool-sweater', createdAtUtc: new Date(Date.now() - 86400000).toISOString() },
      ],
      ...overrides,
    },
  }
}

describe('Catalog DashboardPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    setActivePinia(createPinia())
  })

  it('renders page header with correct title', async () => {
    mockDashboardApi.mockResolvedValue(createSuccessResponse())
    const wrapper = mount(DashboardPage, {
      global: { plugins: [PrimeVue, ConfirmationService, ToastService, router] },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Catalog')
  })

  it('renders hero section with total product count', async () => {
    mockDashboardApi.mockResolvedValue(createSuccessResponse({ totalProducts: 1247 }))
    const wrapper = mount(DashboardPage, {
      global: { plugins: [PrimeVue, ConfirmationService, ToastService, router] },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('1,247')
    expect(wrapper.text()).toContain('total products')
  })

  it('renders 4 stat cards with metrics', async () => {
    mockDashboardApi.mockResolvedValue(createSuccessResponse())
    const wrapper = mount(DashboardPage, {
      global: { plugins: [PrimeVue, ConfirmationService, ToastService, router] },
    })
    await flushPromises()
    const statCards = wrapper.findAllComponents({ name: 'StatCard' })
    expect(statCards).toHaveLength(4)
    expect(wrapper.text()).toContain('Taxonomies')
  })

  it('renders quick action buttons', async () => {
    mockDashboardApi.mockResolvedValue(createSuccessResponse())
    const wrapper = mount(DashboardPage, {
      global: { plugins: [PrimeVue, ConfirmationService, ToastService, router] },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Add Product')
    expect(wrapper.text()).toContain('Import CSV')
    expect(wrapper.text()).toContain('Manage Categories')
  })

  it('renders recently updated product list', async () => {
    mockDashboardApi.mockResolvedValue(createSuccessResponse())
    const wrapper = mount(DashboardPage, {
      global: { plugins: [PrimeVue, ConfirmationService, ToastService, router] },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Recently Updated')
    expect(wrapper.text()).toContain('Vintage Denim Jacket')
    expect(wrapper.text()).toContain('Merino Wool Sweater')
  })
})
