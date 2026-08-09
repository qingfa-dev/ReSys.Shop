import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { nextTick } from 'vue'
import PrimeVue from 'primevue/config'
import TaxonTree from '../TaxonTree.vue'
import { useFilters } from '@/features/catalog/composables/useFilters'
import type { TaxonTreeNode } from '@/features/catalog/types'

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

// Fixture: Root taxon with three leaf children for checkbox filtering.
const taxonomyTree: TaxonTreeNode[] = [
  {
    id: 'root-1',
    name: 'Clothing',
    presentation: 'Clothing',
    permalink: '/clothing',
    depth: 0,
    hasChildren: true,
    children: [
      {
        id: 'leaf-1',
        name: 'T-Shirts',
        presentation: null,
        permalink: '/clothing/t-shirts',
        depth: 1,
        hasChildren: false,
        children: [],
      },
      {
        id: 'leaf-2',
        name: 'Jeans',
        presentation: null,
        permalink: '/clothing/jeans',
        depth: 1,
        hasChildren: false,
        children: [],
      },
      {
        id: 'leaf-3',
        name: 'Sneakers',
        presentation: null,
        permalink: '/clothing/sneakers',
        depth: 1,
        hasChildren: false,
        children: [],
      },
    ],
  },
]

// Mount: PrimeVue so tree renders correctly.
function mountTree() {
  return mount(TaxonTree, {
    props: { nodes: taxonomyTree },
    global: {
      plugins: [PrimeVue],
    },
  })
}

describe('TaxonTree', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    // Reset: Clear singleton filter state between tests
    const filters = useFilters()
    filters.selectedTaxonIds.splice(0)
  })

  it('renders root and child labels with roots expanded by default', () => {
    const wrapper = mountTree()

    expect(wrapper.text()).toContain('Clothing')
    expect(wrapper.text()).toContain('T-Shirts')
    expect(wrapper.text()).toContain('Jeans')
    expect(wrapper.text()).toContain('Sneakers')
    expect(wrapper.text()).toContain('3')
  })

  it('calls useFilters.toggleTaxon with the leaf id when its checkbox is checked', async () => {
    const wrapper = mountTree()
    const filters = useFilters()
    vi.spyOn(filters, 'toggleTaxon')

    const leafNode = wrapper.findAll('.p-tree-node').find(n => n.text().includes('T-Shirts'))
    expect(leafNode).toBeDefined()

    await leafNode!.find('.p-tree-node-checkbox').trigger('click')

    expect(filters.toggleTaxon).toHaveBeenCalledWith('leaf-1')
  })

  it('reflects pre-selected taxon ids as checked checkboxes', async () => {
    const wrapper = mountTree()
    const filters = useFilters()
    filters.selectedTaxonIds.push('leaf-2')
    await nextTick()

    const leafNode = wrapper.findAll('.p-tree-node').find(n => n.text().includes('Jeans'))
    expect(leafNode!.find('.p-tree-node-checkbox[data-p-checked="true"]').exists()).toBe(true)
  })

  it('unchecks every selected leaf when a checked parent is unchecked', async () => {
    const wrapper = mountTree()
    const filters = useFilters()
    filters.selectedTaxonIds.push('root-1', 'leaf-1', 'leaf-2', 'leaf-3')
    await nextTick()
    // Mimic: Restore the real splice semantics of toggleTaxon so
    // removals mutate the live array while the setter iterates it.
    vi.spyOn(filters, 'toggleTaxon').mockImplementation((id: string) => {
      const idx = filters.selectedTaxonIds.indexOf(id)
      if (idx === -1) filters.selectedTaxonIds.push(id)
      else filters.selectedTaxonIds.splice(idx, 1)
    })

    const rootNode = wrapper.findAll('.p-tree-node').find(n => n.text().includes('Clothing'))
    await rootNode!.find('.p-tree-node-checkbox').trigger('click')

    // Regression: the setter iterates a snapshot, so a cascade uncheck removes every
    // selected node instead of skipping the element after each splice.
    expect(filters.toggleTaxon).toHaveBeenCalledTimes(4)
    expect(filters.toggleTaxon).toHaveBeenCalledWith('leaf-1')
    expect(filters.toggleTaxon).toHaveBeenCalledWith('leaf-2')
    expect(filters.toggleTaxon).toHaveBeenCalledWith('leaf-3')
    expect(filters.selectedTaxonIds).toEqual([])
  })
})
