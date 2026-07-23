import { describe, it, expect, vi, beforeEach, beforeAll } from 'vitest'

beforeAll(() => {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: (query: string) => ({
      matches: false,
      media: query,
      onchange: null,
      addListener: () => {},
      removeListener: () => {},
      addEventListener: () => {},
      removeEventListener: () => {},
      dispatchEvent: () => false,
    }),
  })
})
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createWebHistory } from 'vue-router'
import PrimeVue from 'primevue/config'
import ConfirmationService from 'primevue/confirmationservice'
import ToastService from 'primevue/toastservice'
import { ROUTE_CATALOG } from '../../routers/route-names'
import TaxonomyListPage from '../TaxonomyListPage.vue'

const mockGetTaxonomies = vi.fn<(...args: unknown[]) => unknown>()
const mockDeleteTaxonomy = vi.fn<(...args: unknown[]) => unknown>()

vi.mock('../../api/taxonomies', () => ({
  getTaxonomies: (...args: unknown[]) => mockGetTaxonomies(...args),
  deleteTaxonomy: (...args: unknown[]) => mockDeleteTaxonomy(...args),
}))

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: { template: '<div />' } },
    { path: '/catalog/taxonomies', name: ROUTE_CATALOG.TAXONOMIES.LIST, component: { template: '<div />' } },
    { path: '/catalog/taxonomies/new', name: ROUTE_CATALOG.TAXONOMIES.CREATE, component: { template: '<div />' } },
    { path: '/catalog/taxonomies/:id', name: ROUTE_CATALOG.TAXONOMIES.VIEW, component: { template: '<div />' } },
    { path: '/catalog/taxonomies/:id/edit', name: ROUTE_CATALOG.TAXONOMIES.EDIT, component: { template: '<div />' } },
  ],
})

function createTestPlugins() {
  return [PrimeVue, ConfirmationService, ToastService, router]
}

describe('TaxonomyListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders page header', async () => {
    mockGetTaxonomies.mockResolvedValue({
      success: true,
      data: [],
      meta: { page: 1, pageSize: 20, totalCount: 0, totalPages: 0 },
    })
    const wrapper = mount(TaxonomyListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Taxonomies')
  })

  it('displays empty state when no taxonomies', async () => {
    mockGetTaxonomies.mockResolvedValue({
      success: true,
      data: [],
      meta: { page: 1, pageSize: 20, totalCount: 0, totalPages: 0 },
    })
    const wrapper = mount(TaxonomyListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('No taxonomies')
  })

  it('displays taxonomies in table when data exists', async () => {
    mockGetTaxonomies.mockResolvedValue({
      success: true,
      data: [
        { id: '1', name: 'Categories', presentation: 'Categories', position: 1, createdAt: '2026-01-01', updatedAt: '2026-01-01' },
      ],
      meta: { page: 1, pageSize: 20, totalCount: 1, totalPages: 1 },
    })
    const wrapper = mount(TaxonomyListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Categories')
  })

  it('displays error state on API failure', async () => {
    mockGetTaxonomies.mockResolvedValue({
      success: false,
      error: { message: 'Server error', statusCode: 500, title: 'Error', detail: null, errors: {}, errorCode: 'ERR' },
    })
    const wrapper = mount(TaxonomyListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Server error')
  })

  it('has a create button', async () => {
    mockGetTaxonomies.mockResolvedValue({
      success: true,
      data: [],
      meta: { page: 1, pageSize: 20, totalCount: 0, totalPages: 0 },
    })
    const wrapper = mount(TaxonomyListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Add Taxonomy')
  })
})
