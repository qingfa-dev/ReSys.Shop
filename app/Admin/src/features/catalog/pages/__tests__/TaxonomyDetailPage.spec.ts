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
import TaxonomyDetailPage from '../TaxonomyDetailPage.vue'

const mockGetTaxonomy = vi.fn<(...args: unknown[]) => unknown>()
const mockGetTaxons = vi.fn<(...args: unknown[]) => unknown>()

vi.mock('../../api/taxonomies', () => ({
  getTaxonomy: (...args: unknown[]) => mockGetTaxonomy(...args),
  createTaxonomy: vi.fn<(...args: unknown[]) => unknown>(),
  updateTaxonomy: vi.fn<(...args: unknown[]) => unknown>(),
  getTaxons: (...args: unknown[]) => mockGetTaxons(...args),
  createTaxon: vi.fn<(...args: unknown[]) => unknown>(),
  updateTaxon: vi.fn<(...args: unknown[]) => unknown>(),
  deleteTaxon: vi.fn<(...args: unknown[]) => unknown>(),
}))

function makeRouter(initialRoute: string) {
  const router = createRouter({
    history: createWebHistory(),
    routes: [
      { path: '/catalog/taxonomies/new', name: 'catalog.taxonomies.create', component: TaxonomyDetailPage },
      { path: '/catalog/taxonomies/:id', name: 'catalog.taxonomies.view', component: TaxonomyDetailPage },
      { path: '/catalog/taxonomies/:id/edit', name: 'catalog.taxonomies.edit', component: TaxonomyDetailPage },
      { path: '/catalog/taxonomies', name: 'catalog.taxonomies.list', component: { template: '<div />' } },
    ],
  })
  router.push(initialRoute)
  return router
}

const plugins = [PrimeVue, ConfirmationService, ToastService]

describe('TaxonomyDetailPage', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('renders in view mode and loads taxons', async () => {
    mockGetTaxonomy.mockResolvedValue({
      success: true, data: { id: 'abc', name: 'Clothing', presentation: null, position: 0, createdAt: '', updatedAt: '' },
    })
    mockGetTaxons.mockResolvedValue({
      success: true, data: [
        { id: '1', name: 'Men', depth: 0, lft: 1, rgt: 4, taxonomyId: 'abc', parentId: null, slug: 'men', position: 0, presentation: null, description: null, childrenCount: 2, hideFromNav: false, automatic: false, createdAt: '', updatedAt: '' },
        { id: '2', name: 'Tops', depth: 1, lft: 2, rgt: 3, taxonomyId: 'abc', parentId: '1', slug: 'tops', position: 0, presentation: null, description: null, childrenCount: 0, hideFromNav: false, automatic: false, createdAt: '', updatedAt: '' },
        { id: '3', name: 'Women', depth: 0, lft: 5, rgt: 6, taxonomyId: 'abc', parentId: null, slug: 'women', position: 1, presentation: null, description: null, childrenCount: 0, hideFromNav: false, automatic: false, createdAt: '', updatedAt: '' },
      ],
    })
    const router = makeRouter('/catalog/taxonomies/abc')
    await router.isReady()
    const wrapper = mount(TaxonomyDetailPage, { global: { plugins: [...plugins, router] } })
    await flushPromises()
    await flushPromises()
    expect(wrapper.text()).toContain('Clothing')
    expect(wrapper.text()).toContain('Taxons')
    expect(wrapper.text()).toContain('Men')
    expect(wrapper.text()).toContain('Tops')
  })

  it('shows Add Taxon button', async () => {
    mockGetTaxonomy.mockResolvedValue({
      success: true, data: { id: 'abc', name: 'Test', presentation: null, position: 0, createdAt: '', updatedAt: '' },
    })
    mockGetTaxons.mockResolvedValue({ success: true, data: [] })
    const router = makeRouter('/catalog/taxonomies/abc')
    await router.isReady()
    const wrapper = mount(TaxonomyDetailPage, { global: { plugins: [...plugins, router] } })
    await flushPromises()
    await flushPromises()
    expect(wrapper.text()).toContain('Add Taxon')
  })

  it('renders in create mode', async () => {
    const router = makeRouter('/catalog/taxonomies/new')
    await router.isReady()
    const wrapper = mount(TaxonomyDetailPage, { global: { plugins: [...plugins, router] } })
    await flushPromises()
    expect(wrapper.text()).toContain('Create Taxonomy')
  })

  it('shows empty state for taxons', async () => {
    mockGetTaxonomy.mockResolvedValue({
      success: true, data: { id: 'abc', name: 'Empty', presentation: null, position: 0, createdAt: '', updatedAt: '' },
    })
    mockGetTaxons.mockResolvedValue({ success: true, data: [] })
    const router = makeRouter('/catalog/taxonomies/abc')
    await router.isReady()
    const wrapper = mount(TaxonomyDetailPage, { global: { plugins: [...plugins, router] } })
    await flushPromises()
    await flushPromises()
    expect(wrapper.text()).toContain('No taxons')
  })
})
