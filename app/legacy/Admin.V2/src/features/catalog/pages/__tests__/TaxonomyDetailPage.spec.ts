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
import TaxonomyDetailPage from '../TaxonomyDetailPage.vue'

const mockGetTaxonomy = vi.fn<(...args: unknown[]) => unknown>()
const mockUpdateTaxonomy = vi.fn<(...args: unknown[]) => unknown>()
const mockGetTaxons = vi.fn<(...args: unknown[]) => unknown>()

vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: (key: string) => {
      const map: Record<string, string> = {
        'catalog.taxonomies.titles.create': 'Create Taxonomy',
        'catalog.taxonomies.actions.edit': 'Edit',
        'catalog.taxonomies.titles.edit': 'Edit Taxonomy',
        'catalog.taxonomies.actions.save_create': 'Create Taxonomy',
        'catalog.taxonomies.actions.save_edit': 'Update Taxonomy',
        'catalog.taxonomies.actions.cancel': 'Cancel',
        'catalog.taxonomies.labels.name': 'Internal Name',
        'catalog.taxonomies.labels.presentation': 'Display Name',
        'catalog.taxa.titles.manager': 'Manage Categories',
        'catalog.taxa.actions.add_taxon': 'Add Taxon',
        'catalog.taxa.actions.edit_taxon': 'Edit',
        'catalog.taxa.actions.cancel': 'Cancel',
        'catalog.taxa.messages.create_success': 'Taxon created',
        'catalog.taxa.messages.update_success': 'Taxon updated',
        'catalog.taxa.messages.delete_success': 'Taxon removed',
        'catalog.taxa.messages.empty_tree': 'No Categories Found',
        'catalog.taxa.labels.name': 'Internal Name',
        'catalog.taxa.labels.presentation': 'Display Name',
        'catalog.taxa.labels.slug': 'URL Slug',
      }
      return map[key] ?? key
    },
  }),
}))


vi.mock('../../api', () => ({
  TaxonomyApi: {
    get: (...args: unknown[]) => mockGetTaxonomy(...args),
    create: vi.fn<(...args: unknown[]) => unknown>(),
    update: (...args: unknown[]) => mockUpdateTaxonomy(...args),
  },
  TaxonApi: {
    getMany: (...args: unknown[]) => mockGetTaxons(...args),
    create: vi.fn<(...args: unknown[]) => unknown>(),
    update: vi.fn<(...args: unknown[]) => unknown>(),
    delete: vi.fn<(...args: unknown[]) => unknown>(),
  },
}))

function makeRouter(initialRoute: string) {
  const router = createRouter({
    history: createWebHistory(),
    routes: [
      { path: '/catalog/taxonomies/new', name: ROUTE.TAXONOMIES.CREATE, component: TaxonomyDetailPage },
      { path: '/catalog/taxonomies/:id', name: ROUTE.TAXONOMIES.VIEW, component: TaxonomyDetailPage },
      { path: '/catalog/taxonomies/:id/edit', name: ROUTE.TAXONOMIES.EDIT, component: TaxonomyDetailPage },
      { path: '/catalog/taxonomies', name: ROUTE.TAXONOMIES.LIST, component: { template: '<div />' } },
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
      isSuccess: true, value: { id: 'abc', name: 'Clothing', presentation: null, position: 0, createdAt: '', updatedAt: '' },
    })
    mockGetTaxons.mockResolvedValue({
      isSuccess: true, value: [
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
    expect(wrapper.text()).toContain('Manage Categories')
    expect(wrapper.text()).toContain('Men')
    expect(wrapper.text()).toContain('Tops')
  })

  it('shows Add Taxon button', async () => {
    mockGetTaxonomy.mockResolvedValue({
      isSuccess: true, value: { id: 'abc', name: 'Test', presentation: null, position: 0, createdAt: '', updatedAt: '' },
    })
    mockGetTaxons.mockResolvedValue({ isSuccess: true, value: [] })
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
      isSuccess: true, value: { id: 'abc', name: 'Empty', presentation: null, position: 0, createdAt: '', updatedAt: '' },
    })
    mockGetTaxons.mockResolvedValue({ isSuccess: true, value: [] })
    const router = makeRouter('/catalog/taxonomies/abc')
    await router.isReady()
    const wrapper = mount(TaxonomyDetailPage, { global: { plugins: [...plugins, router] } })
    await flushPromises()
    await flushPromises()
    expect(wrapper.text()).toContain('No Categories Found')
  })
})
