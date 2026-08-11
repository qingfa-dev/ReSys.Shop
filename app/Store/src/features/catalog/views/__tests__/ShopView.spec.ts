import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import ShopView from '../ShopView.vue'
import ProductGridCard from '../../components/ProductGridCard.vue'
import Paginator from 'primevue/paginator'
import Select from 'primevue/select'
import { useFilters } from '../../composables/useFilters'
import { useTaxonomy } from '../../composables/useTaxonomy'
import { useProducts } from '../../composables/useProducts'
import type { StoreProductListItemResponse, TaxonomyGroup, FilterableOptionType } from '../../types'

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
    prices: [{ id: 'pr-1', amount: 45, currency: 'USD', compareAtAmount: null, countryIso: 'US' }],
    stock: { totalOnHand: 5, totalReserved: 0, totalAvailable: 5, backorderable: false, locations: [] },
  },
  classifications: [],
}

// Fixture: Taxonomy group with one root taxon for the filter panel accordion.
const taxonomyGroup: TaxonomyGroup = {
  taxonomy: { id: 't1', name: 'Categories', presentation: 'Categories' },
  tree: [
    {
      id: 'r1',
      name: 'Men',
      presentation: null,
      permalink: 'men',
      depth: 0,
      hasChildren: false,
      children: [],
    },
  ],
}

// Fixture: Filterable option type with a single value for the checkbox group.
const optionType: FilterableOptionType = {
  id: 'ot-1',
  name: 'Color',
  presentation: 'Color',
  position: 0,
  filterable: true,
  values: [
    { id: 'v-1', name: 'Red', presentation: null, position: 0, optionTypeId: 'ot-1', optionTypeName: 'Color' },
  ],
}

// Router: Memory-history router with shop and product detail routes.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/shop', component: ShopView },
      { path: '/products/:id', component: { template: '<div />' } },
    ],
  })
}

// Mount: PrimeVue + ToastService so mounted loads work.
async function mountView(initialQuery?: Record<string, string>) {
  const router = createTestRouter()
  await router.push(initialQuery ? { path: '/shop', query: initialQuery } : '/shop')
  await router.isReady()
  const wrapper = mount(ShopView, {
    global: {
      plugins: [PrimeVue, ToastService, router],
    },
  })
  await flushPromises()
  return wrapper
}

// Seed: Populate taxonomy and product composable singletons with fixtures.
function seedStores() {
  const taxonomy = useTaxonomy()
  taxonomy.taxonomyGroups.splice(0)
  taxonomy.taxonomyGroups.push(taxonomyGroup)
  taxonomy.optionTypes.splice(0)
  taxonomy.optionTypes.push(optionType as never)
  const list = useProducts()
  list.items.splice(0)
  list.items.push(product)
  list.totalCount = 25
  list.pageSize = 20
  list.page = 1
  list.isInitialLoad = false
  return { taxonomy, list }
}

describe('ShopView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    // Reset: Clear singleton state between tests
    const filters = useFilters()
    filters.selectedTaxonIds.splice(0)
    filters.selectedOptionValueIds.splice(0)
    filters.searchQuery = ''
    filters.minPrice = null
    filters.maxPrice = null
    filters.sortField = '-CreatedAtUtc'
    const taxonomy = useTaxonomy()
    taxonomy.taxonomyGroups.splice(0)
    taxonomy.optionTypes.splice(0)
    taxonomy.collections.splice(0)
    const list = useProducts()
    list.items.splice(0)
    list.totalCount = 0
    list.page = 1
    list.isInitialLoad = true
    list.loading = false
    list.error = null
  })

  it('renders the filter panel, toolbar result count and product grid', async () => {
    const wrapper = await mountView()
    seedStores()
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Categories')
    expect(wrapper.text()).toContain('Color')
    expect(wrapper.text()).toContain('25 products')
    expect(wrapper.findAllComponents(ProductGridCard)).toHaveLength(1)
    expect(wrapper.text()).toContain('Classic Tee')
  })

  it('removing a taxon filter chip calls useFilters.toggleTaxon', async () => {
    const wrapper = await mountView()
    seedStores()
    const filters = useFilters()
    filters.selectedTaxonIds.push('r1')
    await wrapper.vm.$nextTick()

    await wrapper.find('[aria-label="Remove filter Men"]').trigger('click')

    expect(filters.selectedTaxonIds).not.toContain('r1')
  })

  it('checking an option value checkbox calls useFilters.toggleOptionValue', async () => {
    const wrapper = await mountView()
    seedStores()
    await wrapper.vm.$nextTick()

    await wrapper.find('[data-pc-name="checkbox"] input').trigger('change')

    const filters = useFilters()
    expect(filters.selectedOptionValueIds).toContain('v-1')
  })

  it('renders the paginator with store totals and forwards page changes', async () => {
    const wrapper = await mountView()
    const { list } = seedStores()
    await wrapper.vm.$nextTick()

    const paginator = wrapper.findComponent(Paginator)
    expect(paginator.exists()).toBe(true)
    expect(paginator.props('rows')).toBe(20)
    expect(paginator.props('totalRecords')).toBe(25)
    expect(paginator.props('first')).toBe(0)

    await wrapper.find('[aria-label="Next Page"]').trigger('click')

    expect(list.page).toBe(2)
  })

  it('changes the sort via the sort select', async () => {
    const wrapper = await mountView()
    seedStores()
    await wrapper.vm.$nextTick()

    const select = wrapper.findComponent(Select)
    const options = select.props('options') as { value: string; label: string }[]
    expect(options.some(o => o.value === 'Price' && o.label === 'Price: Low to High')).toBe(true)

    select.vm.$emit('change', { value: 'Price', originalEvent: {} })
    await wrapper.vm.$nextTick()

    const filters = useFilters()
    expect(filters.sortField).toBe('Price')
  })

  it('switches the grid to single-column list mode via the layout toggle', async () => {
    const wrapper = await mountView()
    seedStores()
    await wrapper.vm.$nextTick()

    const anchor = wrapper.find('a.group.block')
    const grid = anchor.element.parentElement
    if (!grid?.classList.contains('grid-cols-2')) {
      throw new Error(`ANCHOR PARENT: ${grid?.outerHTML.slice(0, 200) ?? 'none'}`)
    }

    const buttons = wrapper.find('[data-pc-name="selectbutton"]').findAll('button')
    await buttons[1]!.trigger('click')
    await wrapper.vm.$nextTick()

    expect(grid?.classList.contains('grid-cols-1')).toBe(true)
    expect(grid?.classList.contains('grid-cols-2')).toBe(false)
  })

  it('shows the empty state and clears filters on button click', async () => {
    const wrapper = await mountView()
    seedStores()
    const list = useProducts()
    list.items.splice(0)
    list.totalCount = 0
    list.loading = false
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('No products found')
    expect(wrapper.findAllComponents(ProductGridCard)).toHaveLength(0)

    const clearButton = wrapper.findAll('button').find(b => b.text().trim() === 'Clear filters')
    expect(clearButton?.exists()).toBe(true)
    await clearButton!.trigger('click')

    const filters = useFilters()
    expect(filters.searchQuery).toBe('')
    expect(filters.selectedTaxonIds).toEqual([])
  })

  it('pre-populates filters from the ?taxon= and ?q= route query on mount', async () => {
    const wrapper = await mountView({ taxon: 'r1', q: 'tee' })
    seedStores()
    await wrapper.vm.$nextTick()

    const filters = useFilters()
    expect(filters.selectedTaxonIds).toContain('r1')
    expect(filters.searchQuery).toBe('tee')
    await wrapper.vm.$nextTick()
    expect(wrapper.find('[aria-label="Remove filter Men"]').exists()).toBe(true)
  })

  it('re-applies a changed ?taxon= query when navigating within /shop', async () => {
    const wrapper = await mountView({ taxon: 'r1' })
    seedStores()
    await wrapper.vm.$nextTick()
    vi.clearAllMocks()

    const router = wrapper.vm.$router
    await router.push({ path: '/shop', query: { taxon: 'r2' } })
    await flushPromises()

    const filters = useFilters()
    expect(filters.selectedTaxonIds).toContain('r2')
  })

  it('skips re-toggling a taxon already selected from the route query', async () => {
    const wrapper = await mountView({ taxon: 'r1' })
    seedStores()
    const filters = useFilters()
    filters.selectedTaxonIds.push('r1')
    await wrapper.vm.$nextTick()
    vi.clearAllMocks()

    const router = wrapper.vm.$router
    await router.push({ path: '/shop', query: { taxon: 'r1', q: 'tee' } })
    await flushPromises()

    // The watch fired (setSearch ran in the same pass) but the selected taxon is skipped
    expect(filters.selectedTaxonIds).toContain('r1')
    expect(filters.searchQuery).toBe('tee')
  })
})
