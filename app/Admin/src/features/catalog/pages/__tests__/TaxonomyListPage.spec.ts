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
import { ROUTE } from '../../routes'
import TaxonomyListPage from '../TaxonomyListPage.vue'

vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: (key: string) => {
      const map: Record<string, string> = {
        'catalog.taxonomies.titles.list': 'Taxonomies',
        'catalog.taxonomies.descriptions.list': 'Organize your products into root hierarchies (e.g. Categories, Brands).',
        'catalog.taxonomies.actions.create': 'New Taxonomy',
        'catalog.taxonomies.messages.empty_list': 'No taxonomies found.',
        'catalog.taxonomies.messages.delete_success': 'Taxonomy deleted successfully.',
      }
      return map[key] ?? key
    },
  }),
}))

const mockGetTaxonomies = vi.fn<(...args: unknown[]) => unknown>()
const mockDeleteTaxonomy = vi.fn<(...args: unknown[]) => unknown>()

vi.mock('../../api', () => ({
  TaxonomyApi: {
    getMany: (...args: unknown[]) => mockGetTaxonomies(...args),
    delete: (...args: unknown[]) => mockDeleteTaxonomy(...args),
  },
}))

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: { template: '<div />' } },
    { path: '/catalog/taxonomies', name: ROUTE.TAXONOMIES.LIST, component: { template: '<div />' } },
    { path: '/catalog/taxonomies/new', name: ROUTE.TAXONOMIES.CREATE, component: { template: '<div />' } },
    { path: '/catalog/taxonomies/:id', name: ROUTE.TAXONOMIES.VIEW, component: { template: '<div />' } },
    { path: '/catalog/taxonomies/:id/edit', name: ROUTE.TAXONOMIES.EDIT, component: { template: '<div />' } },
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
      isSuccess: true,
      items: [],
      page: 1, pageSize: 20, totalCount: 0,
    })
    const wrapper = mount(TaxonomyListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Taxonomies')
  })

  it('displays empty state when no taxonomies', async () => {
    mockGetTaxonomies.mockResolvedValue({
      isSuccess: true,
      items: [],
      page: 1, pageSize: 20, totalCount: 0,
    })
    const wrapper = mount(TaxonomyListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('No taxonomies found.')
  })

  it('displays taxonomies in table when data exists', async () => {
    mockGetTaxonomies.mockResolvedValue({
      isSuccess: true,
      items: [
        { id: '1', name: 'Categories', presentation: 'Categories', position: 1, createdAt: '2026-01-01', updatedAt: '2026-01-01' },
      ],
      page: 1, pageSize: 20, totalCount: 1,
    })
    const wrapper = mount(TaxonomyListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Categories')
  })

  it('displays error state on API failure', async () => {
    mockGetTaxonomies.mockResolvedValue({
      isSuccess: false,
      message: 'Server error', statusCode: 500, errors: [{ code: 'ERR', message: 'Server error' }],
    })
    const wrapper = mount(TaxonomyListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Server error')
  })

  it('has a create button', async () => {
    mockGetTaxonomies.mockResolvedValue({
      isSuccess: true,
      items: [],
      page: 1, pageSize: 20, totalCount: 0,
    })
    const wrapper = mount(TaxonomyListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('New Taxonomy')
  })
})
